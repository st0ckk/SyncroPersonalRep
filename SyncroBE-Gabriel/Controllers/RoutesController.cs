using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.DTOs.Route;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;
using System.Globalization;
using System.Security.Claims;

namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/routes")]
    [Authorize(Roles = "SuperUsuario,Administrador,Vendedor,Chofer")]
    public class RoutesController : ControllerBase
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IClientRepository _clientRepository;
        private readonly SyncroDbContext _context;

        public RoutesController(
            IRouteRepository routeRepository,
            IClientRepository clientRepository,
            SyncroDbContext context)
        {
            _routeRepository = routeRepository;
            _clientRepository = clientRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] DateTime? date,
            [FromQuery] int? driverUserId,
            [FromQuery] bool includeInactive = false)
        {
            var routes = await _routeRepository.GetAsync(date, driverUserId, includeInactive);
            return Ok(routes);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var route = await _routeRepository.GetDtoByIdAsync(id);
            if (route == null) return NotFound("Ruta no encontrada");

            return Ok(route);
        }

        [HttpGet("my/today")]
        [Authorize(Roles = "Chofer,SuperUsuario,Administrador")]
        public async Task<IActionResult> GetMyToday([FromQuery] DateTime? date)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("No se pudo determinar el usuario desde el token.");

            var targetDate = (date ?? GetTodayCostaRica()).Date;
            var routes = await _routeRepository.GetByDriverAndDateAsync(userId.Value, targetDate);

            return Ok(routes);
        }

        [HttpGet("my/dates")]
        [Authorize(Roles = "Chofer,SuperUsuario,Administrador")]
        public async Task<IActionResult> GetMyRouteDates([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("No se pudo determinar el usuario desde el token.");

            var fromDate = from.Date;
            var toDate = to.Date;

            var dates = await _context.DeliveryRoutes
                .AsNoTracking()
                .Where(r =>
                    r.DriverUserId == userId.Value &&
                    r.IsActive &&
                    r.RouteDate >= fromDate &&
                    r.RouteDate <= toDate)
                .Select(r => r.RouteDate)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            return Ok(dates.Select(d => d.ToString("yyyy-MM-dd")));
        }

        [HttpPut("my/{routeId:int}/stops/{stopId:int}/status")]
        [Authorize(Roles = "Chofer,SuperUsuario,Administrador")]
        public async Task<IActionResult> UpdateMyStopStatus(
            int routeId,
            int stopId,
            [FromBody] UpdateStopStatusDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("No se pudo determinar el usuario desde el token.");

            var status = (dto.Status ?? "").Trim();

            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "EnRoute",
                "Delivered",
                "Cancelled"
            };

            if (!allowed.Contains(status))
                return BadRequest("Estado inválido. Use: EnRoute, Delivered, Cancelled.");

            if (status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(dto.Note))
                return BadRequest("Debe indicar una nota/motivo al cancelar.");

            DeliveryRouteStop? stop;
            try
            {
                stop = await GetAuthorizedStopAsync(routeId, stopId, userId.Value);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            if (stop == null)
                return NotFound("Parada no encontrada.");

            stop.Status = status;
            stop.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.Note))
                stop.Notes = dto.Note.Trim();

            if (status.Equals("Delivered", StringComparison.OrdinalIgnoreCase))
                stop.DeliveredAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await UpdateRouteStatusAutoAsync(routeId);

            return Ok(new
            {
                stop.RouteStopId,
                stop.RouteId,
                stop.Status,
                stop.Notes,
                stop.DeliveredAt,
                stop.DeliveryPhotoPath,
                stop.DeliveryPhotoUploadedAt,
                stop.UpdatedAt
            });
        }

        [HttpPost("my/{routeId:int}/stops/{stopId:int}/photo")]
        [Authorize(Roles = "Chofer,SuperUsuario,Administrador")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadMyStopPhoto(
            int routeId,
            int stopId,
            [FromForm] UploadStopPhotoDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("No se pudo determinar el usuario desde el token.");

            if (dto.Photo == null || dto.Photo.Length == 0)
                return BadRequest("Debe adjuntar una imagen.");

            if (dto.Photo.Length > 10_000_000)
                return BadRequest("La imagen no puede superar los 10 MB.");

            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".webp", ".heic"
            };

            var extension = Path.GetExtension(dto.Photo.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
                return BadRequest("Formato no permitido. Use JPG, PNG, WEBP o HEIC.");

            DeliveryRouteStop? stop;
            try
            {
                stop = await GetAuthorizedStopAsync(routeId, stopId, userId.Value);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            if (stop == null)
                return NotFound("Parada no encontrada.");

            var relativeDirectory = Path.Combine(
                "uploads",
                "route-deliveries",
                routeId.ToString(CultureInfo.InvariantCulture));

            var physicalDirectory = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                relativeDirectory);

            Directory.CreateDirectory(physicalDirectory);

            if (!string.IsNullOrWhiteSpace(stop.DeliveryPhotoPath))
            {
                var previousFile = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    stop.DeliveryPhotoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (System.IO.File.Exists(previousFile))
                {
                    System.IO.File.Delete(previousFile);
                }
            }

            var safeFileName = $"stop-{stopId}-{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension.ToLowerInvariant()}";
            var physicalPath = Path.Combine(physicalDirectory, safeFileName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await dto.Photo.CopyToAsync(stream);
            }

            stop.DeliveryPhotoPath = $"/{relativeDirectory.Replace('\\', '/')}/{safeFileName}";
            stop.DeliveryPhotoUploadedAt = DateTime.UtcNow;
            stop.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                stop.RouteStopId,
                stop.RouteId,
                stop.DeliveryPhotoPath,
                stop.DeliveryPhotoUploadedAt,
                stop.UpdatedAt
            });
        }

        [HttpPost]
        [Authorize(Roles = "SuperUsuario,Administrador,Vendedor")]
        public async Task<IActionResult> Create([FromBody] RouteCreateDto dto)
        {
            if (dto.Stops == null || dto.Stops.Count == 0)
                return BadRequest("La ruta debe tener al menos una parada.");

            if (dto.Stops.Select(s => s.StopOrder).Distinct().Count() != dto.Stops.Count)
                return BadRequest("Los StopOrder no pueden repetirse.");

            if (await _routeRepository.DriverHasRouteAsync(dto.DriverUserId, dto.RouteDate))
                return Conflict("Ese chofer ya tiene una ruta activa asignada para esa fecha.");

            var stopBuild = await BuildStopsAsync(dto.Stops);
            if (!stopBuild.IsValid)
                return BadRequest(stopBuild.Error);

            var entity = new DeliveryRoute
            {
                RouteName = dto.RouteName.Trim(),
                RouteDate = dto.RouteDate.Date,
                DriverUserId = dto.DriverUserId,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Draft" : dto.Status.Trim(),
                StartAtPlanned = dto.StartAtPlanned,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Stops = stopBuild.Stops
            };

            await _routeRepository.AddAsync(entity);

            return Ok(new { entity.RouteId });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperUsuario,Administrador,Vendedor")]
        public async Task<IActionResult> Update(int id, [FromBody] RouteUpdateDto dto)
        {
            if (id != dto.RouteId)
                return BadRequest("El id de la ruta no coincide con RouteId.");

            if (dto.Stops == null || dto.Stops.Count == 0)
                return BadRequest("La ruta debe tener al menos una parada.");

            if (dto.Stops.Select(s => s.StopOrder).Distinct().Count() != dto.Stops.Count)
                return BadRequest("Los StopOrder no pueden repetirse.");

            var entity = await _routeRepository.GetByIdAsync(id);
            if (entity == null)
                return NotFound("Ruta no encontrada.");

            if (await _routeRepository.DriverHasRouteAsync(dto.DriverUserId, dto.RouteDate, dto.RouteId))
                return Conflict("Ese chofer ya tiene una ruta activa asignada para esa fecha.");

            var stopBuild = await BuildStopsAsync(dto.Stops);
            if (!stopBuild.IsValid)
                return BadRequest(stopBuild.Error);

            entity.RouteName = dto.RouteName.Trim();
            entity.RouteDate = dto.RouteDate.Date;
            entity.DriverUserId = dto.DriverUserId;
            entity.Status = string.IsNullOrWhiteSpace(dto.Status) ? "Draft" : dto.Status.Trim();
            entity.StartAtPlanned = dto.StartAtPlanned;
            entity.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            entity.Stops.Clear();
            foreach (var stop in stopBuild.Stops.OrderBy(s => s.StopOrder))
            {
                entity.Stops.Add(stop);
            }

            await _routeRepository.UpdateAsync(entity);

            return Ok();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperUsuario,Administrador,Vendedor")]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _routeRepository.DeactivateAsync(id);
            return NoContent();
        }

        [HttpPut("{id:int}/activate")]
        [Authorize(Roles = "SuperUsuario,Administrador,Vendedor")]
        public async Task<IActionResult> Activate(int id)
        {
            await _routeRepository.ActivateAsync(id);
            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var userIdStr =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("sub");

            return int.TryParse(userIdStr, out var userId) ? userId : null;
        }

        private async Task<DeliveryRouteStop?> GetAuthorizedStopAsync(int routeId, int stopId, int userId)
        {
            var stop = await _context.DeliveryRouteStops
                .Include(s => s.Route)
                .FirstOrDefaultAsync(s => s.RouteStopId == stopId && s.RouteId == routeId);

            if (stop == null)
                return null;

            var isDriver = User.IsInRole("Chofer");
            if (isDriver && stop.Route.DriverUserId != userId)
                return null;

            if (isDriver)
            {
                var todayCR = GetTodayCostaRica();
                if (stop.Route.RouteDate.Date != todayCR)
                    throw new InvalidOperationException("Solo puedes actualizar estados o fotos en rutas del día de hoy.");
            }

            return stop;
        }

        private async Task<(bool IsValid, string? Error, List<DeliveryRouteStop> Stops)> BuildStopsAsync(
            IEnumerable<RouteStopCreateUpdateDto> stopDtos)
        {
            var result = new List<DeliveryRouteStop>();

            foreach (var stopDto in stopDtos.OrderBy(s => s.StopOrder))
            {
                var client = await _clientRepository.GetByIdAsync(stopDto.ClientId);

                if (client == null)
                    return (false, $"Cliente no encontrado: {stopDto.ClientId}", new List<DeliveryRouteStop>());

                if (client.Location == null)
                    return (false, $"El cliente {client.ClientName} no tiene ubicación registrada.", new List<DeliveryRouteStop>());

                result.Add(new DeliveryRouteStop
                {
                    ClientId = client.ClientId,
                    ClientNameSnapshot = client.ClientName,
                    AddressSnapshot = client.Location.Address ?? client.ExactAddress,
                    StopOrder = stopDto.StopOrder,
                    PlannedArrival = stopDto.PlannedArrival,
                    Latitude = client.Location.Latitude,
                    Longitude = client.Location.Longitude,
                    Notes = string.IsNullOrWhiteSpace(stopDto.Notes) ? null : stopDto.Notes.Trim(),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                });
            }

            return (true, null, result);
        }

        private async Task UpdateRouteStatusAutoAsync(int routeId)
        {
            var route = await _context.DeliveryRoutes
                .Include(r => r.Stops)
                .FirstOrDefaultAsync(r => r.RouteId == routeId);

            if (route == null) return;

            var anyEnRoute = route.Stops.Any(s =>
                string.Equals(s.Status, "EnRoute", StringComparison.OrdinalIgnoreCase));

            var allFinished = route.Stops.All(s =>
                string.Equals(s.Status, "Delivered", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s.Status, "Cancelled", StringComparison.OrdinalIgnoreCase));

            if (allFinished)
                route.Status = "Completed";
            else if (anyEnRoute)
                route.Status = "InProgress";
            else
                route.Status = string.IsNullOrWhiteSpace(route.Status) ? "Scheduled" : route.Status;

            route.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private static DateTime GetTodayCostaRica()
        {
            TimeZoneInfo tz;

            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
            }
            catch
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");
            }

            var nowCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            return nowCR.Date;
        }
    }
}