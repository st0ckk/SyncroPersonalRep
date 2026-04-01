using SyncroBE.Application.DTOs.RouteTemplate;
using SyncroBE.Domain.Entities;

namespace SyncroBE.Application.Interfaces
{
    public interface IRouteTemplateRepository
    {
        Task<IEnumerable<RouteTemplateDto>> GetAsync(bool includeInactive);
        Task<RouteTemplateDto?> GetDtoByIdAsync(int id);
        Task<RouteTemplate?> GetByIdAsync(int id);

        Task AddAsync(RouteTemplate template);
        Task UpdateAsync(RouteTemplate template);

        Task DeactivateAsync(int id);
        Task ActivateAsync(int id);
    }
}