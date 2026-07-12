using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.DTOs.User;
using SyncroBE.Infrastructure.Auth;
using SyncroBE.Infrastructure.Data;
using System.Security.Claims;


namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/account")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly SyncroDbContext _context;
        private readonly TotpService _totp;
        private readonly IConfiguration _config;

        public AccountController(SyncroDbContext context, TotpService totp, IConfiguration config)
        {
            _context = context;
            _totp = totp;
            _config = config;
        }

        // GET: api/account/me
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetMyProfile()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    UserRole = u.UserRole,
                    UserName = u.UserName,
                    UserLastname = u.UserLastname,
                    UserEmail = u.UserEmail,
                    Telefono = u.Telefono,
                    TelefonoPersonal = u.TelefonoPersonal,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLogin = u.LastLogin,
                    TwoFactorEnabled = u.TwoFactorEnabled
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            return Ok(user);
        }

        // PUT: api/account/password
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return BadRequest(new { message = "Contraseña actual incorrecta" });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.MustChangePassword = false;
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Ok("Contraseña actualizada correctamente");
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "Error al actualizar la contraseña. Por favor intente de nuevo." });
            }
        }

        // GET: api/account/2fa/setup
        // Genera un nuevo secret y URI para que el usuario escanee con su app de autenticación.
        // Guarda el secret en el usuario (sin activar 2FA aún).
        [HttpGet("2fa/setup")]
        public async Task<ActionResult<TotpSetupResponseDto>> SetupTwoFactor()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            var issuer = _config["Jwt:Issuer"] ?? "SyncroCR";
            var (secret, uri) = _totp.GenerateSetup(user.UserEmail, issuer);

            user.TwoFactorSecret = secret;
            await _context.SaveChangesAsync();

            return Ok(new TotpSetupResponseDto { Secret = secret, OtpauthUri = uri });
        }

        // POST: api/account/2fa/enable
        // Valida el código con el secret guardado y activa el 2FA.
        [HttpPost("2fa/enable")]
        public async Task<IActionResult> EnableTwoFactor([FromBody] TotpEnableDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            if (string.IsNullOrEmpty(user.TwoFactorSecret))
                return BadRequest(new { message = "Primero debe generar la configuración de 2FA" });

            if (!_totp.Verify(user.TwoFactorSecret, dto.Code))
                return BadRequest(new { message = "Código incorrecto. Verifique su aplicación de autenticación." });

            user.TwoFactorEnabled = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Doble factor de autenticación activado correctamente" });
        }

        // DELETE: api/account/2fa/disable
        // Desactiva el 2FA del usuario.
        [HttpDelete("2fa/disable")]
        public async Task<IActionResult> DisableTwoFactor()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Doble factor de autenticación desactivado" });
        }

        // ─────────────────────────────────────────────────────────
        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
