using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;
using SyncroBE.Application.DTOs.Schedule;


namespace SyncroBE.Infrastructure.Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly SyncroDbContext _context;

        public ScheduleRepository(SyncroDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ScheduleDto>> GetAsync(DateTime? from, DateTime? to, int? userId, bool includeInactive)
        {
            var q = _context.EmployeeSchedules
                .AsNoTracking()
                .AsQueryable();

            if (!includeInactive)
                q = q.Where(s => s.IsActive);

            if (userId.HasValue)
                q = q.Where(s => s.UserId == userId.Value);

            if (from.HasValue)
                q = q.Where(s => s.StartAt >= from.Value);

            if (to.HasValue)
            {
                var endExclusive = to.Value.Date.AddDays(1);
                q = q.Where(s => s.StartAt < endExclusive);
            }
            return await q
                .OrderBy(s => s.StartAt)
                .Select(s => new ScheduleDto
                {
                    ScheduleId = s.ScheduleId,
                    UserId = s.UserId,
                    UserName =
                        ((s.User != null ? s.User.UserName : "") ?? "") + " " +
                        ((s.User != null ? s.User.UserLastname : "") ?? ""),
                    StartAt = s.StartAt,
                    EndAt = s.EndAt,
                    Notes = s.Notes,
                    IsActive = s.IsActive
                })
                .ToListAsync();
        }


        public async Task<EmployeeSchedule?> GetByIdAsync(int id)
        {
            return await _context.EmployeeSchedules
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.ScheduleId == id);
        }

        public async Task AddAsync(EmployeeSchedule schedule)
        {
            _context.EmployeeSchedules.Add(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(EmployeeSchedule schedule)
        {
            _context.EmployeeSchedules.Update(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int id)
        {
            var s = await _context.EmployeeSchedules.FindAsync(id);
            if (s == null) return;

            s.IsActive = false;
            s.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task ActivateAsync(int id)
        {
            var s = await _context.EmployeeSchedules.FindAsync(id);
            if (s == null) return;

            s.IsActive = true;
            s.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        public async Task<bool> HasOverlapAsync(int userId, DateTime startAt, DateTime endAt, int? excludeScheduleId = null)
        {
            var q = _context.EmployeeSchedules
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.IsActive);

            if (excludeScheduleId.HasValue)
                q = q.Where(s => s.ScheduleId != excludeScheduleId.Value);

            // overlap: start < otherEnd AND end > otherStart
            return await q.AnyAsync(s => s.StartAt < endAt && s.EndAt > startAt);
        }

    }
}
