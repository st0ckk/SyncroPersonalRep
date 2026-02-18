using SyncroBE.Application.DTOs.Schedule;
using SyncroBE.Domain.Entities;

namespace SyncroBE.Application.Interfaces
{
    public interface IScheduleRepository
    {
        Task<IEnumerable<ScheduleDto>> GetAsync(DateTime? from, DateTime? to, int? userId, bool includeInactive);
        Task<EmployeeSchedule?> GetByIdAsync(int id);
        Task AddAsync(EmployeeSchedule schedule);
        Task UpdateAsync(EmployeeSchedule schedule);
        Task DeactivateAsync(int id);
        Task ActivateAsync(int id);
        Task<bool> HasOverlapAsync(int userId, DateTime startAt, DateTime endAt, int? excludeScheduleId = null);

    }
}
