using System.Collections.Generic;
using System.Threading.Tasks;
using SyncroBE.Domain.Entities;

namespace SyncroBE.Application.Interfaces
{
    public interface IDistributorRepository
    {
        Task<IEnumerable<Distributor>> GetAllAsync();
        Task<IEnumerable<Distributor>> GetLookupAsync();
        Task<Distributor?> GetByIdAsync(int id);
        Task<bool> CodeExistsAsync(string distributorCode);
        Task AddAsync(Distributor distributor);
        Task UpdateAsync(Distributor distributor);
        Task DeactivateAsync(Distributor distributor);
        Task ActivateAsync(Distributor distributor);
        Task<Distributor?> GetByIdIncludingInactiveAsync(int id);
        Task<IEnumerable<Distributor>> GetInactiveAsync();


    }
}
