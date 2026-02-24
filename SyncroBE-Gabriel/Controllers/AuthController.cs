using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SyncroBE.Application.DTOs.User;
using SyncroBE.Application.Interfaces;
using SyncroBE.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
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

        private const int MaxFailedAttempts = 3;
        private const int LockoutMinutes = 15;

        public AuthController(SyncroDbContext context, IConfiguration config, IAuditService audit)
        {
            _context = context;
            _config = config;
            _audit = audit;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.UserEmail == dto.Email);

            // ── Usuario no existe o está inactivo ──
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

            // ── Si el lockout ya expiró, resetear ──
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

            await _audit.LogAsync("User", user.UserId.ToString(), "LOGIN_SUCCESS", user.UserId);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.UserEmail),
                new Claim(ClaimTypes.Role, user.UserRole)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                mustChangePassword = user.MustChangePassword,
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
    }
}