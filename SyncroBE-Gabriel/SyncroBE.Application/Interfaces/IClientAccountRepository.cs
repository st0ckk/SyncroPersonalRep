using SyncroBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.Interfaces
{
    public interface IClientAccountRepository
    {
        Task AddAsync(ClientAccount account);
        Task<IEnumerable<ClientAccount>> GetAllAsync();
        Task<IEnumerable<ClientAccount>> GetAllActiveAsync();
        Task<ClientAccount?> GetById(int id);
        Task<IEnumerable<ClientAccount>> GetByClient(string client);
        Task<IEnumerable<ClientAccount>> FilterAsync(DateTime? startDate, DateTime? endDate, string searchTerm, string state);
        Task<IEnumerable<ClientAccountMovement>> FilterMovementsAsync(int id, DateTime? startDate, DateTime? endDate, string searchTerm, string type);
        Task UpdateAsync(ClientAccount account);
        Task CloseAccountAsync(ClientAccount account);
    }
}
