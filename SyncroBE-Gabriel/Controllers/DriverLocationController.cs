using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroBE.Application.Interfaces;
using System.Security.Claims;

namespace SyncroBE.API.Controllers;

[ApiController]
[Route("api/driver-locations")]
[Authorize]
public class DriverLocationController : ControllerBase
{
    // Considera "activo" a cualquier chofer que envió ubicación en los últimos 5 minutos
    private static readonly TimeSpan ActiveWindow = TimeSpan.FromMinutes(5);

    private readonly IDriverLocationStore _store;

    public DriverLocationController(IDriverLocationStore store)
    {
        _store = store;
    }

    /// <summary>
    /// El chofer envía su posición GPS actual.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Chofer,SuperUsuario,Administrador")]
    public IActionResult UpdateMyLocation([FromBody] UpdateLocationRequest req)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var name = User.FindFirstValue(ClaimTypes.Name)
                   ?? User.FindFirstValue(ClaimTypes.Email)
                   ?? $"Chofer #{userId}";

        _store.Set(userId.Value, req.DriverName ?? name, req.Latitude, req.Longitude);

        return NoContent();
    }

    /// <summary>
    /// El monitor obtiene la última ubicación de todos los choferes activos.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperUsuario,Administrador,Vendedor,Chofer")]
    public IActionResult GetActive()
    {
        var entries = _store.GetActive(ActiveWindow);

        var result = entries.Select(e => new
        {
            e.DriverId,
            e.DriverName,
            e.Latitude,
            e.Longitude,
            UpdatedAt = e.UpdatedAt.ToString("o"),   // ISO 8601 UTC
            SecondsAgo = (int)(DateTime.UtcNow - e.UpdatedAt).TotalSeconds
        });

        return Ok(result);
    }

    private int? GetCurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");

        return int.TryParse(raw, out var id) ? id : null;
    }
}

public record UpdateLocationRequest(
    double Latitude,
    double Longitude,
    string? DriverName
);
