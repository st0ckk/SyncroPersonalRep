using SyncroBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.Interfaces
{
    public interface IClientRepository
    {
        Task<IEnumerable<Client>> GetActiveAsync();
        Task<IEnumerable<Client>> GetInactiveAsync();
        Task<Client?> GetByIdAsync(string clientId);

        Task AddAsync(Client client);
        Task UpdateAsync(Client client);

        Task DeactivateAsync(string clientId);
        Task ActivateAsync(string clientId);
    }
}

