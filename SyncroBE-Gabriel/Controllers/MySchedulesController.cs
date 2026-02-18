using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroBE.Application.Interfaces;
using System.Security.Claims;

namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/me/schedules")]
    [Authorize] 
    public class MySchedulesController : ControllerBase
    {
        private readonly IScheduleRepository _repo;

        public MySchedulesController(IScheduleRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetMySchedules(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] bool includeInactive = false)
        {
            var userIdStr =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("sub");

            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("No se pudo determinar el usuario desde el token.");

            var result = await _repo.GetAsync(from, to, userId, includeInactive);
            return Ok(result);
        }
    }
}
