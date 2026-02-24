using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.DTOs.Vacations;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;

namespace SyncroBE.Infrastructure.Services
{
    public class VacationService : IVacationService
    {
        private readonly SyncroDbContext _context;

        public VacationService(SyncroDbContext context)
        {
            _context = context;
        }

        public async Task<VacationBalanceDto> GetBalanceAsync(int userId)
        {
            var balance = await EnsureBalanceRowAsync(userId);

            return new VacationBalanceDto
            {
                UserId = userId,
                AvailableDays = balance.AvailableDays,
                LastAccrualDate = balance.LastAccrualDate?.ToString("yyyy-MM-dd")
            };
        }

        public async Task AssignDaysAsync(int userId, AssignVacationDaysDto dto)
        {
            if (dto.Days < 0)
                throw new Exception("Los días no pueden ser negativos.");

            var balance = await EnsureBalanceRowAsync(userId);

            if (dto.IsSetOperation)
                balance.AvailableDays = dto.Days;
            else
                balance.AvailableDays += dto.Days;

            balance.UpdatedAt = DateTime.Now;

            _context.VacationMovements.Add(new VacationMovement
            {
                UserId = userId,
                MovementType = dto.IsSetOperation ? "ASSIGN" : "ADJUST",
                Days = dto.Days,
                Description = dto.Reason,
                CreatedAt = DateTime.Now,
                CreatedBy = dto.CreatedBy
            });

            await _context.SaveChangesAsync();
        }

        public async Task<Vacation> CreateVacationAsync(CreateVacationDto dto, int? createdBy = null)
        {
            if (dto.EndDate.Date < dto.StartDate.Date)
                throw new Exception("La fecha final no puede ser menor a la inicial.");

            
            var daysRequestedInt = CountBusinessDays(dto.StartDate.Date, dto.EndDate.Date);

            if (daysRequestedInt <= 0)
                throw new Exception("El rango seleccionado no contiene días hábiles.");

            var daysRequested = (decimal)daysRequestedInt;

            // validar solapamiento
            var hasOverlap = await _context.Vacations.AnyAsync(v =>
                v.UserId == dto.UserId &&
                v.Status == "APPROVED" &&
                dto.StartDate.Date <= v.EndDate &&
                dto.EndDate.Date >= v.StartDate);

            if (hasOverlap)
                throw new Exception("El usuario ya tiene vacaciones en ese rango.");

            var balance = await EnsureBalanceRowAsync(dto.UserId);

            if (balance.AvailableDays < daysRequested)
                throw new Exception($"Saldo insuficiente. Disponible: {balance.AvailableDays}, solicitado: {daysRequested}");

            using var tx = await _context.Database.BeginTransactionAsync();

            var vacation = new Vacation
            {
                UserId = dto.UserId,
                StartDate = dto.StartDate.Date,
                EndDate = dto.EndDate.Date,
                DaysRequested = daysRequested,
                Reason = dto.Reason,
                Status = "APPROVED",
                CreatedAt = DateTime.Now
            };

            _context.Vacations.Add(vacation);

            balance.AvailableDays -= daysRequested;
            balance.UpdatedAt = DateTime.Now;

            _context.VacationMovements.Add(new VacationMovement
            {
                UserId = dto.UserId,
                MovementType = "USE",
                Days = daysRequested,
                Description = $"Vacaciones {dto.StartDate:yyyy-MM-dd} a {dto.EndDate:yyyy-MM-dd}",
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return vacation;
        }

        public async Task<List<Vacation>> GetUserVacationsAsync(int userId)
        {
            return await _context.Vacations
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task CancelVacationAsync(int vacationId, int? createdBy = null)
        {
            var vacation = await _context.Vacations.FirstOrDefaultAsync(v => v.VacationId == vacationId);
            if (vacation == null)
                throw new Exception("Vacación no encontrada.");

            if (vacation.Status == "CANCELLED")
                throw new Exception("La vacación ya está cancelada.");

            var balance = await EnsureBalanceRowAsync(vacation.UserId);

            using var tx = await _context.Database.BeginTransactionAsync();

            vacation.Status = "CANCELLED";

            // devolver días
            balance.AvailableDays += vacation.DaysRequested;
            balance.UpdatedAt = DateTime.Now;

            _context.VacationMovements.Add(new VacationMovement
            {
                UserId = vacation.UserId,
                MovementType = "REFUND",
                Days = vacation.DaysRequested,
                Description = $"Cancelación de vacaciones #{vacation.VacationId}",
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }

        public async Task RunMonthlyAccrualAsync()
        {
            var today = DateTime.Today;
            var currentMonth = new DateTime(today.Year, today.Month, 1);

            var userIds = await _context.Users
                .Select(u => u.UserId)
                .ToListAsync();

            foreach (var userId in userIds)
            {
                var balance = await EnsureBalanceRowAsync(userId);

                if (balance.LastAccrualDate == null)
                {
                    balance.AvailableDays += 1;
                    balance.LastAccrualDate = currentMonth;
                    balance.UpdatedAt = DateTime.Now;

                    _context.VacationMovements.Add(new VacationMovement
                    {
                        UserId = userId,
                        MovementType = "ACCRUAL",
                        Days = 1,
                        Description = $"Acumulación mensual {currentMonth:yyyy-MM}",
                        CreatedAt = DateTime.Now
                    });

                    continue;
                }

                var lastMonth = new DateTime(balance.LastAccrualDate.Value.Year, balance.LastAccrualDate.Value.Month, 1);
                var monthsPending = ((currentMonth.Year - lastMonth.Year) * 12) + (currentMonth.Month - lastMonth.Month);

                if (monthsPending > 0)
                {
                    balance.AvailableDays += monthsPending;
                    balance.LastAccrualDate = currentMonth;
                    balance.UpdatedAt = DateTime.Now;

                    _context.VacationMovements.Add(new VacationMovement
                    {
                        UserId = userId,
                        MovementType = "ACCRUAL",
                        Days = monthsPending,
                        Description = $"Acumulación mensual ({monthsPending} mes(es)) hasta {currentMonth:yyyy-MM}",
                        CreatedAt = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task<UserVacationBalance> EnsureBalanceRowAsync(int userId)
        {
            var balance = await _context.UserVacationBalances
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (balance != null) return balance;

            balance = new UserVacationBalance
            {
                UserId = userId,
                AvailableDays = 0,
                LastAccrualDate = null,
                UpdatedAt = DateTime.Now
            };

            _context.UserVacationBalances.Add(balance);
            await _context.SaveChangesAsync();

            return balance;
        }

        private int CountBusinessDays(DateTime start, DateTime end)
        {
            if (end < start)
                return 0;

            int count = 0;
            var d = start.Date;

            while (d <= end.Date)
            {
                var dayOfWeek = d.DayOfWeek; 

                bool isWeekend = dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;

                if (!isWeekend)
                {
                    count++;
                }

                d = d.AddDays(1);
            }

            return count;
        }
        public Task<int> CalculateBusinessDaysAsync(DateTime startDate, DateTime endDate)
        {
            if (endDate.Date < startDate.Date)
                return Task.FromResult(0);

            var days = CountBusinessDays(startDate.Date, endDate.Date);
            return Task.FromResult(days);
        }
    }
}