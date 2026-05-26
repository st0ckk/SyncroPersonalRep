using SyncroBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.Interfaces
{
    public interface ICashRegisterRepository
    {
        Task AddAsync(CashRegister cashRegister);
        Task AddManualMovementAsync(CashRegisterMovement movement);
        Task<IEnumerable<CashRegister>> GetAllAsync(User? user);
        Task<CashRegister?> GetById(int id);
        Task<IEnumerable<CashRegister>> GetUserOpenRegisters(int id);
        Task<IEnumerable<CashRegisterMovement>> GetRegisterMovements(int id);
        Task<IEnumerable<CashRegister>> FilterAsync(DateTime? startDate, DateTime? endDate, string searchTerm, string status, User? user);
        Task<IEnumerable<CashRegisterMovement>> FilterMovementsAsync(int id, DateTime? startDate, DateTime? endDate, string searchTerm, string type);
        Task CloseRegisterAsync(CashRegister account);
        Task<decimal> GetRegisterExpectedAmount(int id);
    }
}
