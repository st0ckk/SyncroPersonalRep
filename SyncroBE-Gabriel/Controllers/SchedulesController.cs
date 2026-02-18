using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroBE.Application.DTOs.Schedule;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using System.Security.Claims;

namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/schedules")]
    [Authorize(Roles = "SuperUsuario,Administrador")]
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleRepository _repo;

        public SchedulesController(IScheduleRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? userId,
            [FromQuery] bool includeInactive = false)
        {
            var result = await _repo.GetAsync(from, to, userId, includeInactive);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ScheduleCreateDto dto)
        {
            if (dto.EndAt <= dto.StartAt)
                return BadRequest("EndAt debe ser mayor que StartAt");

            // ✅ Validación overlap
            if (await _repo.HasOverlapAsync(dto.UserId, dto.StartAt, dto.EndAt))
                return Conflict("Ya existe un horario que se traslapa con ese rango.");

            var creatorIdStr =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("sub");

            if (!int.TryParse(creatorIdStr, out var creatorId))
                return Unauthorized("No se pudo determinar el usuario creador desde el token.");

            var note = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();


            var entity = new EmployeeSchedule
            {
                UserId = dto.UserId,
                StartAt = dto.StartAt,
                EndAt = dto.EndAt,
                Notes = note,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = creatorId
            };

            await _repo.AddAsync(entity);
            return Ok(entity.ScheduleId);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ScheduleUpdateDto dto)
        {
            if (id != dto.ScheduleId)
                return BadRequest("El id de la ruta no coincide con ScheduleId");

            if (dto.EndAt <= dto.StartAt)
                return BadRequest("EndAt debe ser mayor que StartAt");

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                return NotFound("Horario no encontrado");

            if (dto.IsActive && await _repo.HasOverlapAsync(dto.UserId, dto.StartAt, dto.EndAt, dto.ScheduleId))
                return Conflict("Ya existe un horario que se traslapa con ese rango.");

            var note = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

            entity.UserId = dto.UserId;
            entity.StartAt = dto.StartAt;
            entity.EndAt = dto.EndAt;
            entity.Notes = note;
            entity.IsActive = dto.IsActive;

            await _repo.UpdateAsync(entity);
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _repo.DeactivateAsync(id);
            return NoContent();
        }

        [HttpPut("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            await _repo.ActivateAsync(id);
            return NoContent();
        }

    }
}
