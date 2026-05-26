using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SyncroBE.Application.Configuration;
using SyncroBE.Application.DTOs.User;
using SyncroBE.Application.Interfaces;
using SyncroBE.Infrastructure.Auth;
using SyncroBE.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly SyncroDbContext _context;
        private readonly IConfiguration _config;
        private readonly IAuditService _audit;
        private readonly TotpService _totp;
        private readonly EmailSettings _email;
        private readonly ILogger<AuthController> _logger;

        private const int MaxFailedAttempts = 3;
        private const int LockoutMinutes = 15;

        public AuthController(SyncroDbContext context, IConfiguration config, IAuditService audit, TotpService totp, IOptions<EmailSettings> emailOptions, ILogger<AuthController> logger)
        {
            _context = context;
            _config = config;
            _audit = audit;
            _totp = totp;
            _email = emailOptions.Value;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.UserEmail == dto.Email);

            if (user == null || !user.IsActive)
                return Unauthorized(new { message = "Credenciales inválidas" });

            // ── Verificar si está bloqueado ──
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                var remaining = (int)Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                return StatusCode(423, new
                {
                    message = $"Cuenta bloqueada. Intente de nuevo en {remaining} minuto(s).",
                    lockoutEnd = user.LockoutEnd.Value,
                    remainingMinutes = remaining
                });
            }

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value <= DateTime.UtcNow)
            {
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
            }

            // ── Verificar contraseña ──
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);

                    await _audit.LogAsync("User", user.UserId.ToString(), "ACCOUNT_LOCKED",
                        user.UserId, $"Bloqueado por {MaxFailedAttempts} intentos fallidos");

                    await _context.SaveChangesAsync();

                    return StatusCode(423, new
                    {
                        message = $"Cuenta bloqueada por {LockoutMinutes} minutos debido a {MaxFailedAttempts} intentos fallidos.",
                        lockoutEnd = user.LockoutEnd.Value,
                        remainingMinutes = LockoutMinutes
                    });
                }

                await _context.SaveChangesAsync();

                var attemptsLeft = MaxFailedAttempts - user.FailedLoginAttempts;
                return Unauthorized(new
                {
                    message = $"Credenciales inválidas. {attemptsLeft} intento(s) restante(s)."
                });
            }

            // ── Login exitoso: resetear contadores ──
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // ── Si tiene 2FA activo, emitir token temporal ──
            if (user.TwoFactorEnabled)
            {
                var tempToken = GenerateTempToken(user.UserId);
                return Ok(new { requiresTwoFactor = true, tempToken });
            }

            await _audit.LogAsync("User", user.UserId.ToString(), "LOGIN_SUCCESS", user.UserId);

            return Ok(new
            {
                token = GenerateFullToken(user),
                mustChangePassword = user.MustChangePassword,
                twoFactorEnabled = user.TwoFactorEnabled,
                user = new
                {
                    user.UserId,
                    user.UserName,
                    user.UserLastname,
                    user.UserEmail,
                    user.UserRole
                }
            });
        }

        [HttpPost("verify-totp")]
        public async Task<IActionResult> VerifyTotp([FromBody] TotpVerifyDto dto)
        {
            // Validar el temp token manualmente (no usa [Authorize])
            var principal = ValidateTempToken(dto.TempToken);
            if (principal == null)
                return Unauthorized(new { message = "Token inválido o expirado" });

            var pendingClaim = principal.FindFirst("totp_pending")?.Value;
            if (pendingClaim != "true")
                return Unauthorized(new { message = "Token inválido" });

            var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { message = "Token inválido" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
                return Unauthorized(new { message = "Usuario no válido para 2FA" });

            if (!_totp.Verify(user.TwoFactorSecret, dto.Code))
                return BadRequest(new { message = "Código incorrecto" });

            await _audit.LogAsync("User", user.UserId.ToString(), "LOGIN_SUCCESS_2FA", user.UserId);

            return Ok(new
            {
                token = GenerateFullToken(user),
                mustChangePassword = user.MustChangePassword,
                twoFactorEnabled = true,
                user = new
                {
                    user.UserId,
                    user.UserName,
                    user.UserLastname,
                    user.UserEmail,
                    user.UserRole
                }
            });
        }

        // ─────────────────────────────────────────────────────────
        // Password Recovery: email link + 2FA confirmation
        // ─────────────────────────────────────────────────────────

        [HttpPost("recover/request")]
        public async Task<IActionResult> RecoverRequest([FromBody] RecoverRequestDto dto)
        {
            const string genericMsg = "Si el correo existe y tiene 2FA activo, recibirás un enlace en tu bandeja de entrada.";

            var user = _context.Users.FirstOrDefault(u => u.UserEmail == dto.Email);
            if (user == null || !user.IsActive || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
                return Ok(new { message = genericMsg });

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim("recovery_pending", "true")
            };
            var resetToken = BuildToken(claims, DateTime.UtcNow.AddMinutes(30));

            var appUrl = _config["AppUrl"]?.TrimEnd('/') ?? "http://localhost:5173";
            var resetLink = $"{appUrl}/recuperar-contrasena?token={resetToken}";

            try
            {
                await SendRecoveryEmailAsync(user.UserEmail, user.UserName, resetLink);
                _logger.LogInformation("Recovery email sent to {Email}", user.UserEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send recovery email to {Email}", user.UserEmail);
            }

            return Ok(new { message = genericMsg });
        }

        [HttpPost("recover/set-password")]
        public async Task<IActionResult> RecoverSetPassword([FromBody] RecoverSetPasswordDto dto)
        {
            var principal = ValidateTempToken(dto.ResetToken);
            if (principal == null)
                return Unauthorized(new { message = "El enlace es inválido o ya expiró. Solicita uno nuevo." });

            if (principal.FindFirst("recovery_pending")?.Value != "true")
                return Unauthorized(new { message = "Token inválido" });

            var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { message = "Token inválido" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
                return Unauthorized(new { message = "Usuario no válido" });

            if (!_totp.Verify(user.TwoFactorSecret, dto.TotpCode))
                return BadRequest(new { message = "Código 2FA incorrecto" });

            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
                return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres" });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.MustChangePassword = false;
            await _context.SaveChangesAsync();

            await _audit.LogAsync("User", user.UserId.ToString(), "PASSWORD_RESET", user.UserId, "Recuperación vía email + 2FA");

            return Ok(new { message = "Contraseña actualizada correctamente" });
        }

        private async Task SendRecoveryEmailAsync(string toEmail, string userName, string resetLink)
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_email.FromEmail, _email.FromName),
                Subject = "Recuperación de contraseña — SyncroCR",
                IsBodyHtml = true,
                Body = $@"<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <style>
    body {{ margin:0; padding:0; background:#0f172a; font-family:Arial,sans-serif; color:#e2e8f0; }}
    .wrap {{ max-width:520px; margin:40px auto; background:#162033; border:1px solid #1e3a5f; border-radius:12px; overflow:hidden; }}
    .header {{ background:linear-gradient(135deg,#0ea5e9,#6366f1); padding:28px 32px; text-align:center; }}
    .header h1 {{ margin:0; font-size:22px; color:#fff; letter-spacing:2px; }}
    .body {{ padding:28px 32px; }}
    .body p {{ margin:0 0 16px; font-size:14px; line-height:1.6; color:#cbd5e1; }}
    .btn {{ display:inline-block; padding:13px 28px; background:linear-gradient(135deg,#0ea5e9,#6366f1); color:#fff; text-decoration:none; border-radius:8px; font-size:15px; font-weight:600; margin:8px 0 20px; }}
    .hint {{ font-size:12px; color:#64748b; word-break:break-all; }}
    .footer {{ text-align:center; font-size:11px; color:#334155; padding:16px 32px 24px; }}
  </style>
</head>
<body>
  <div class='wrap'>
    <div class='header'><h1>❄ SyncroCR</h1></div>
    <div class='body'>
      <p>Hola <strong>{userName}</strong>,</p>
      <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta. Haz clic en el botón de abajo — el enlace es válido por <strong>30 minutos</strong>.</p>
      <p style='text-align:center'>
        <a class='btn' href='{resetLink}'>Restablecer contraseña</a>
      </p>
      <p class='hint'>Si el botón no funciona, copia este enlace en tu navegador:<br>{resetLink}</p>
      <p style='margin-top:24px;font-size:13px;color:#475569;'>Si no solicitaste este cambio, ignora este correo. Tu contraseña no será modificada.</p>
    </div>
    <div class='footer'>Distribuidora Sion &bull; Este es un correo automático, no respondas.</div>
  </div>
</body>
</html>"
            };

            message.To.Add(new MailAddress(toEmail));

            using var client = new SmtpClient(_email.SmtpHost, _email.SmtpPort)
            {
                Credentials = new NetworkCredential(_email.Username, _email.Password),
                EnableSsl = _email.UseSsl
            };

            await client.SendMailAsync(message);
        }

        // ─────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────

        private string GenerateFullToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.UserEmail),
                new Claim(ClaimTypes.Role, user.UserRole)
            };

            return BuildToken(claims, DateTime.UtcNow.AddHours(8));
        }

        private string GenerateTempToken(int userId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("totp_pending", "true")
            };

            return BuildToken(claims, DateTime.UtcNow.AddMinutes(5));
        }

        private string BuildToken(Claim[] claims, DateTime expires)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private ClaimsPrincipal? ValidateTempToken(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                }, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
