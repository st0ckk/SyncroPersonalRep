using SyncroBE.Application.DTOs.Hacienda;

namespace SyncroBE.Application.Interfaces
{
    public interface IHaciendaLookupService
    {
        Task<HaciendaContributorDto?> LookupContributorAsync(string identificacion);
        Task<object?> SearchCabysAsync(string query, int top = 10);
    }
}
