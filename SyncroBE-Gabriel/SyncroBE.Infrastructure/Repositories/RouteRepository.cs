using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.DTOs.Route;
using SyncroBE.Application.DTOs.Sale;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;
using System.Diagnostics;

namespace SyncroBE.Infrastructure.Repositories
{
    public class RouteRepository : IRouteRepository
    {
        private readonly SyncroDbContext _context;

        public RouteRepository(SyncroDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RouteDto>> GetAsync(DateTime? date, int? driverUserId, bool includeInactive)
        {
            var query = _context.DeliveryRoutes
                .AsNoTracking()
                .Include(r => r.DriverUser)
                .Include(r => r.Stops)
                .AsQueryable();


            if (!includeInactive)
                query = query.Where(r => r.IsActive);

            if (driverUserId.HasValue)
                query = query.Where(r => r.DriverUserId == driverUserId.Value);

            if (date.HasValue)
            {
                var targetDate = date.Value.Date;
                query = query.Where(r => r.RouteDate.Date == targetDate);
            }



            var routes = await query
                .OrderBy(r => r.RouteDate)
                .ThenBy(r => r.StartAtPlanned)
                .ToListAsync();

            return routes.Select(Map);
        }

        public async Task<RouteDto?> GetDtoByIdAsync(int id)
        {
            var entity = await _context.DeliveryRoutes
                .AsNoTracking()
                .Include(r => r.DriverUser)
                .Include(r => r.Stops)
                .FirstOrDefaultAsync(r => r.RouteId == id);

            return entity == null ? null : Map(entity);
        }

        public async Task<DeliveryRoute?> GetByIdAsync(int id)
        {
            return await _context.DeliveryRoutes
                .Include(r => r.Stops)
                .FirstOrDefaultAsync(r => r.RouteId == id);
        }

        public async Task AddAsync(DeliveryRoute route)
        {
            _context.DeliveryRoutes.Add(route);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DeliveryRoute route)
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int id)
        {
            var entity = await _context.DeliveryRoutes.FindAsync(id);
            if (entity == null) return;

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task ActivateAsync(int id)
        {
            var entity = await _context.DeliveryRoutes.FindAsync(id);
            if (entity == null) return;

            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DriverHasRouteAsync(int driverUserId, DateTime routeDate, int? excludeRouteId = null)
        {
            var date = routeDate.Date;

            var query = _context.DeliveryRoutes
                .AsNoTracking()
                .Where(r =>
                    r.DriverUserId == driverUserId &&
                    r.RouteDate.Date == date &&
                    r.IsActive);

            if (excludeRouteId.HasValue)
                query = query.Where(r => r.RouteId != excludeRouteId.Value);

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<RouteDto>> GetByDriverAndDateAsync(int driverUserId, DateTime routeDate)
        {
            var date = routeDate.Date;

            var routes = await _context.DeliveryRoutes
                .AsNoTracking()
                .Include(r => r.DriverUser)
                .Include(r => r.Stops)
                .Include(r => r.Purchases)
                .ThenInclude(c => c.Client)
                .Include(r => r.Purchases)
                .ThenInclude(sd => sd.SaleDetails)
                .Include(r => r.Purchases)
                .ThenInclude(u => u.User)
                .Where(r =>
                    r.DriverUserId == driverUserId &&
                    r.RouteDate.Date == date &&
                    r.IsActive)
                .OrderBy(r => r.StartAtPlanned)
                .ToListAsync();

            return routes.Select(Map);
        }

        private static RouteDto Map(DeliveryRoute route)
        {
            return new RouteDto
            {
                RouteId = route.RouteId,
                RouteName = route.RouteName,
                RouteDate = route.RouteDate,
                DriverUserId = route.DriverUserId,
                DriverName = $"{route.DriverUser?.UserName ?? ""} {route.DriverUser?.UserLastname ?? ""}".Trim(),
                Status = route.Status,
                StartAtPlanned = route.StartAtPlanned,
                EndAtEstimated = route.EndAtEstimated,
                EstimatedDurationMinutes = route.EstimatedDurationMinutes,
                EstimatedDistanceKm = route.EstimatedDistanceKm,
                Polyline = route.Polyline,
                Notes = route.Notes,
                IsActive = route.IsActive,
                StopCount = route.Stops?.Count ?? 0,
                Stops = route.Stops?
                    .OrderBy(s => s.StopOrder)
                    .Select(s => new RouteStopDto
                    {
                        RouteStopId = s.RouteStopId,
                        ClientId = s.ClientId,
                        ClientNameSnapshot = s.ClientNameSnapshot,
                        AddressSnapshot = s.AddressSnapshot,
                        StopOrder = s.StopOrder,
                        PlannedArrival = s.PlannedArrival,
                        EstimatedTravelMinutesFromPrevious = s.EstimatedTravelMinutesFromPrevious,
                        Latitude = s.Latitude,
                        Longitude = s.Longitude,
                        Status = s.Status,
                        Notes = s.Notes,
                        DeliveryPhotoPath = s.DeliveryPhotoPath,
                        DeliveryPhotoUploadedAt = s.DeliveryPhotoUploadedAt,
                        DeliveredAt = s.DeliveredAt
                    })
                    .ToList() ?? new List<RouteStopDto>(),
                Purchases = route.Purchases?.Select(p => new SaleDto
                {
                    PurchaseId = p.PurchaseId,
                    UserName = $"{p.User.UserName} {p.User.UserLastname}",
                    ClientName = p.Client.ClientName,
                    RouteId = p.RouteId,
                    PurchaseOrderNumber = p.PurchaseOrderNumber,
                    PurchaseDate = p.PurchaseDate,
                    saleDetails = p.SaleDetails.Select(d => new SaleDetailDto
                    {
                        SaleDetailId = d.SaleDetailId,
                        ProductId = d.ProductId,
                        ProductName = d.ProductName,
                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice,
                        LineTotal = d.LineTotal
                    }).ToList()
                })
                .ToList() ?? new List<SaleDto>(),
            };
        }
    }
}