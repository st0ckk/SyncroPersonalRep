using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroBE.Application.DTOs.RouteTemplate;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;

namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/route-templates")]
    [Authorize(Roles = "SuperUsuario,Administrador,Vendedor")]
    public class RouteTemplatesController : ControllerBase
    {
        private readonly IRouteTemplateRepository _routeTemplateRepository;
        private readonly IRouteRepository _routeRepository;
        private readonly IClientRepository _clientRepository;

        public RouteTemplatesController(
            IRouteTemplateRepository routeTemplateRepository,
            IRouteRepository routeRepository,
            IClientRepository clientRepository)
        {
            _routeTemplateRepository = routeTemplateRepository;
            _routeRepository = routeRepository;
            _clientRepository = clientRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] bool includeInactive = false)
        {
            var templates = await _routeTemplateRepository.GetAsync(includeInactive);
            return Ok(templates);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var template = await _routeTemplateRepository.GetDtoByIdAsync(id);
            if (template == null) return NotFound("Plantilla no encontrada.");

            return Ok(template);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RouteTemplateCreateDto dto)
        {
            if (dto.Stops == null || dto.Stops.Count == 0)
                return BadRequest("La plantilla debe tener al menos una parada.");

            if (dto.Stops.Select(s => s.StopOrder).Distinct().Count() != dto.Stops.Count)
                return BadRequest("Los StopOrder no pueden repetirse.");

            var stopBuild = await BuildTemplateStopsAsync(dto.Stops);
            if (!stopBuild.IsValid)
                return BadRequest(stopBuild.Error);

            var entity = new RouteTemplate
            {
                TemplateName = dto.TemplateName.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                DefaultDriverUserId = dto.DefaultDriverUserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Stops = stopBuild.Stops
            };

            await _routeTemplateRepository.AddAsync(entity);

            return Ok(new { entity.TemplateId });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] RouteTemplateUpdateDto dto)
        {
            if (id != dto.TemplateId)
                return BadRequest("El id no coincide con TemplateId.");

            if (dto.Stops == null || dto.Stops.Count == 0)
                return BadRequest("La plantilla debe tener al menos una parada.");

            if (dto.Stops.Select(s => s.StopOrder).Distinct().Count() != dto.Stops.Count)
                return BadRequest("Los StopOrder no pueden repetirse.");

            var entity = await _routeTemplateRepository.GetByIdAsync(id);
            if (entity == null)
                return NotFound("Plantilla no encontrada.");

            var stopBuild = await BuildTemplateStopsAsync(dto.Stops);
            if (!stopBuild.IsValid)
                return BadRequest(stopBuild.Error);

            entity.TemplateName = dto.TemplateName.Trim();
            entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            entity.DefaultDriverUserId = dto.DefaultDriverUserId;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            entity.Stops.Clear();
            foreach (var stop in stopBuild.Stops.OrderBy(s => s.StopOrder))
            {
                entity.Stops.Add(stop);
            }

            await _routeTemplateRepository.UpdateAsync(entity);

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _routeTemplateRepository.DeactivateAsync(id);
            return NoContent();
        }

        [HttpPut("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            await _routeTemplateRepository.ActivateAsync(id);
            return NoContent();
        }

        [HttpPost("{id:int}/instantiate")]
        public async Task<IActionResult> Instantiate(int id, [FromBody] InstantiateRouteFromTemplateDto dto)
        {
            var template = await _routeTemplateRepository.GetByIdAsync(id);
            if (template == null)
                return NotFound("Plantilla no encontrada.");

            if (template.Stops == null || !template.Stops.Any())
                return BadRequest("La plantilla no tiene paradas.");

            var driverUserId = dto.DriverUserId ?? template.DefaultDriverUserId;
            if (!driverUserId.HasValue)
                return BadRequest("Debes indicar un chofer o configurar un chofer por defecto en la plantilla.");

            if (await _routeRepository.DriverHasRouteAsync(driverUserId.Value, dto.RouteDate))
                return Conflict("Ese chofer ya tiene una ruta activa asignada para esa fecha.");

            var routeName = string.IsNullOrWhiteSpace(dto.RouteName)
                ? $"{template.TemplateName} - {dto.RouteDate:yyyy-MM-dd}"
                : dto.RouteName.Trim();

            var route = new DeliveryRoute
            {
                RouteName = routeName,
                RouteDate = dto.RouteDate.Date,
                DriverUserId = driverUserId.Value,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Scheduled" : dto.Status.Trim(),
                StartAtPlanned = dto.StartAtPlanned,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? template.Description : dto.Notes.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Stops = template.Stops
                    .OrderBy(s => s.StopOrder)
                    .Select(s => new DeliveryRouteStop
                    {
                        ClientId = s.ClientId,
                        ClientNameSnapshot = s.ClientNameSnapshot,
                        AddressSnapshot = s.AddressSnapshot,
                        StopOrder = s.StopOrder,
                        Latitude = s.Latitude,
                        Longitude = s.Longitude,
                        Status = "Pending",
                        Notes = s.Notes,
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList()
            };

            await _routeRepository.AddAsync(route);

            return Ok(new
            {
                route.RouteId,
                route.RouteName
            });
        }

        private async Task<(bool IsValid, string? Error, List<RouteTemplateStop> Stops)> BuildTemplateStopsAsync(
            IEnumerable<RouteTemplateStopCreateUpdateDto> stopDtos)
        {
            var result = new List<RouteTemplateStop>();

            foreach (var stopDto in stopDtos.OrderBy(s => s.StopOrder))
            {
                var client = await _clientRepository.GetByIdAsync(stopDto.ClientId);

                if (client == null)
                    return (false, $"Cliente no encontrado: {stopDto.ClientId}", new List<RouteTemplateStop>());

                if (client.Location == null)
                    return (false, $"El cliente {client.ClientName} no tiene ubicación registrada.", new List<RouteTemplateStop>());

                result.Add(new RouteTemplateStop
                {
                    ClientId = client.ClientId,
                    ClientNameSnapshot = client.ClientName,
                    AddressSnapshot = client.Location.Address ?? client.ExactAddress,
                    StopOrder = stopDto.StopOrder,
                    Latitude = client.Location.Latitude,
                    Longitude = client.Location.Longitude,
                    Notes = string.IsNullOrWhiteSpace(stopDto.Notes) ? null : stopDto.Notes.Trim(),
                    CreatedAt = DateTime.UtcNow
                });
            }

            return (true, null, result);
        }
    }
}