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
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    UserRole = u.UserRole,
                    UserName = u.UserName,
                    UserLastname = u.UserLastname,
                    UserEmail = u.UserEmail,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLogin = u.LastLogin
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var user = await _context.Users
                .Where(u => u.UserId == id)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    UserRole = u.UserRole,
                    UserName = u.UserName,
                    UserLastname = u.UserLastname,
                    UserEmail = u.UserEmail,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLogin = u.LastLogin
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
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Syncro123*"),
                IsActive = true,
                MustChangePassword = true, // 🔥 FORZAR CAMBIO
                CreatedAt = DateTime.UtcNow
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

            await _context.SaveChangesAsync();

            return Ok("Usuario actualizado correctamente");
        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeUserRoleDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.UserRole = dto.UserRole;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Usuario no encontrado");

            user.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok("Usuario desactivado correctamente");
        }
    }
}
