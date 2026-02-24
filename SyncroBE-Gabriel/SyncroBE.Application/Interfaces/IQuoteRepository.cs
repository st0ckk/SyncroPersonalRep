using SyncroBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.Interfaces
{
    public interface IQuoteRepository
    {
        Task AddAsync(Quote quote,List<QuoteDetail> quoteItems);
        Task<IEnumerable<Quote>> GetAllAsync();
        Task<Quote?> GetById(int id);
        Task<Quote?> GetLatestByClient(string client);
        Task<IEnumerable<Quote>> FilterAsync(DateTime? startDate,DateTime? endDate, string searchTerm, string state);
        Task UpdateAsync(Quote quote, List<QuoteDetail> quoteItems);


    }
}
