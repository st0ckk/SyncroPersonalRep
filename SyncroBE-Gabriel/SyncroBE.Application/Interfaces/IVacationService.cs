using SyncroBE.Application.DTOs.Vacations;
using SyncroBE.Domain.Entities;

namespace SyncroBE.Application.Interfaces
{
    public interface IVacationService
    {
        Task<VacationBalanceDto> GetBalanceAsync(int userId);
        Task AssignDaysAsync(int userId, AssignVacationDaysDto dto);
        Task<Vacation> CreateVacationAsync(CreateVacationDto dto, int? createdBy = null);
        Task<List<Vacation>> GetUserVacationsAsync(int userId);
        Task CancelVacationAsync(int vacationId, int? createdBy = null);
        Task RunMonthlyAccrualAsync();
        Task<int> CalculateBusinessDaysAsync(DateTime startDate, DateTime endDate);
    }
}