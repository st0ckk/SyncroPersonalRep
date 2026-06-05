using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SyncroBE.API.Authorization;
using SyncroBE.Infrastructure.Data;
using System.Security.Claims;

namespace SyncroBE.API.Controllers;

[ApiController]
[Route("api/route-incidents")]
[Authorize]
public class RouteIncidentsController : ControllerBase
{
    private readonly SyncroDbContext _context;

    public RouteIncidentsController(SyncroDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Authorize(Roles = "Chofer,SuperUsuario,Administrador")]
    public async Task<IActionResult> Create([FromBody] CreateIncidentDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.IncidentType))
            return BadRequest(new { message = "El tipo de incidente es requerido." });

        var now = DateTime.UtcNow;

        try
        {
            var sql = @"
                INSERT INTO route_incident (route_id, driver_user_id, incident_type, description, occurred_at, created_at)
                VALUES (@routeId, @driverUserId, @incidentType, @description, @occurredAt, @createdAt)";

            var routeId   = dto.RouteId.HasValue ? (object)dto.RouteId.Value : DBNull.Value;
            var desc      = string.IsNullOrWhiteSpace(dto.Description) ? (object)DBNull.Value : dto.Description.Trim();

            var result = await _context.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@routeId",      routeId),
                new SqlParameter("@driverUserId", userId.Value),
                new SqlParameter("@incidentType", dto.IncidentType.Trim()),
                new SqlParameter("@description",  desc),
                new SqlParameter("@occurredAt",   now),
                new SqlParameter("@createdAt",    now));

            return Ok(new { message = "Incidente registrado." });
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            return StatusCode(500, new { message = "Error al guardar el incidente.", detail = inner });
        }
    }

    [HttpGet]
    [ScreenOrRole("rutas-monitoreo", "SuperUsuario", "Administrador", "Vendedor")]
    public async Task<IActionResult> GetAll([FromQuery] string? date, [FromQuery] int? driverUserId)
    {
        DateTime start, end;

        if (!string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var d))
        {
            start = d.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            end   = d.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        }
        else
        {
            start = DateTime.UtcNow.Date;
            end   = start.AddDays(1);
        }

        var query = _context.RouteIncidents
            .Where(i => i.OccurredAt >= start && i.OccurredAt <= end);

        if (driverUserId.HasValue)
            query = query.Where(i => i.DriverUserId == driverUserId.Value);

        var incidents = await query
            .OrderByDescending(i => i.OccurredAt)
            .Join(_context.Users,
                  i => i.DriverUserId,
                  u => u.UserId,
                  (i, u) => new
                  {
                      i.IncidentId,
                      i.RouteId,
                      i.DriverUserId,
                      DriverName    = (u.UserName + " " + u.UserLastname).Trim(),
                      i.IncidentType,
                      i.Description,
                      OccurredAt    = i.OccurredAt.ToString("o"),
                  })
            .ToListAsync();

        return Ok(incidents);
    }

    private int? GetCurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");
        return int.TryParse(raw, out var id) ? id : null;
    }
}

public record CreateIncidentDto(
    int? RouteId,
    string IncidentType,
    string? Description
);
