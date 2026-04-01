using SyncroBE.Application.DTOs.Route;
using SyncroBE.Domain.Entities;

namespace SyncroBE.Application.Interfaces
{
    public interface IRouteRepository
    {
        Task<IEnumerable<RouteDto>> GetAsync(DateTime? date, int? driverUserId, bool includeInactive);
        Task<RouteDto?> GetDtoByIdAsync(int id);
        Task<DeliveryRoute?> GetByIdAsync(int id);

        Task AddAsync(DeliveryRoute route);
        Task UpdateAsync(DeliveryRoute route);

        Task DeactivateAsync(int id);
        Task ActivateAsync(int id);

        Task<bool> DriverHasRouteAsync(int driverUserId, DateTime routeDate, int? excludeRouteId = null);
        Task<IEnumerable<RouteDto>> GetByDriverAndDateAsync(int driverUserId, DateTime routeDate);
    }
}