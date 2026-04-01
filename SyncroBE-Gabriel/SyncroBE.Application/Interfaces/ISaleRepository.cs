using SyncroBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.Interfaces
{
    public interface ISaleRepository
    {
        Task AddAsync(Purchase purchase, List<SaleDetail> saleItems);
        Task<IEnumerable<Purchase>> GetAllAsync(int userId);
        Task<Purchase?> GetById(int id);
        Task<IEnumerable<Purchase>> FilterAsync(DateTime? startDate, DateTime? endDate, string searchTerm, string state, string paidState);
        Task UpdateAsync(Purchase Purchase, List<SaleDetail> saleItems);
    }
}
