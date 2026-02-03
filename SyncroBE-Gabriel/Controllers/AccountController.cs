using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroBE.Application.DTOs.User;
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

        public AccountController(SyncroDbContext context)
        {
            _context = context;
        }

        // PUT: api/account/password
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound("Usuario no encontrado");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return BadRequest("Contraseña actual incorrecta");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.MustChangePassword = false;

            await _context.SaveChangesAsync();

            return Ok("Contraseña actualizada correctamente");
        }
    }
}
