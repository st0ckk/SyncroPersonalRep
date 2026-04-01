using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.DTOs.User;
using SyncroBE.Infrastructure.Data;

namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "SuperUsuario,Administrador")]
    public class UsersController : ControllerBase
    {
        private readonly SyncroDbContext _context;

        public UsersController(SyncroDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var now = DateTime.UtcNow;

            var users = await _context.Users
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
                    MustChangePassword = u.MustChangePassword,
                    CreatedAt = u.CreatedAt,
                    LastLogin = u.LastLogin,
                    FailedLoginAttempts = u.FailedLoginAttempts,
                    LockoutEnd = u.LockoutEnd,
                    IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd.Value > now
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var now = DateTime.UtcNow;

            var user = await _context.Users
                .Where(u => u.UserId == id)
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
                    MustChangePassword = u.MustChangePassword,
                    CreatedAt = u.CreatedAt,
                    LastLogin = u.LastLogin,
                    FailedLoginAttempts = u.FailedLoginAttempts,
                    LockoutEnd = u.LockoutEnd,
                    IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd.Value > now
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("Usuario no encontrado");

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.UserEmail == dto.UserEmail))
                return BadRequest("El correo ya está registrado");

            var user = new User
            {
                UserName = dto.UserName,
                UserLastname = dto.UserLastname,
                UserEmail = dto.UserEmail,
                UserRole = dto.UserRole,
                Telefono = dto.Telefono,
                TelefonoPersonal = dto.TelefonoPersonal,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Syncro123*"),
                IsActive = true,
                MustChangePassword = true,
                FailedLoginAttempts = 0,
                LockoutEnd = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Usuario no encontrado");

            if (await _context.Users.AnyAsync(u => u.UserEmail == dto.UserEmail && u.UserId != id))
                return BadRequest("El correo ya está en uso");

            user.UserName = dto.UserName;
            user.UserLastname = dto.UserLastname;
            user.UserEmail = dto.UserEmail;
            user.Telefono = dto.Telefono;
            user.TelefonoPersonal = dto.TelefonoPersonal;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Usuario actualizado correctamente");
        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeUserRoleDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.UserRole = dto.UserRole;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("Usuario no encontrado");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Syncro123*");
            user.MustChangePassword = true;

            // desbloquear cuenta para que pueda entrar con la temporal
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Contraseña restablecida correctamente.",
                temporaryPassword = "Syncro123*",
                mustChangePassword = true
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Usuario no encontrado");

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Usuario desactivado correctamente");
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetMyProfile()
        {
            var userIdClaim = User.FindFirst("sub")?.Value
                              ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("No se encontró el id del usuario en el token.");

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Id de usuario inválido.");

            var now = DateTime.UtcNow;

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
                    MustChangePassword = u.MustChangePassword,
                    CreatedAt = u.CreatedAt,
                    LastLogin = u.LastLogin,
                    FailedLoginAttempts = u.FailedLoginAttempts,
                    LockoutEnd = u.LockoutEnd,
                    IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd.Value > now
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("Usuario no encontrado.");

            return Ok(user);
        }
    }
}