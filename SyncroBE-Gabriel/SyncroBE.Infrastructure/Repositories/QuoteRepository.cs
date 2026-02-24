using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Infrastructure.Repositories
{
    public class QuoteRepository : IQuoteRepository
    {
        private readonly SyncroDbContext _context;

        public QuoteRepository(SyncroDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Quote quote, List<QuoteDetail> quoteItems)
        {
            //Establecimiento del numero de cotizacion
            var latestQuote = await _context.Quotes.OrderByDescending(q => q.QuoteNumber).FirstOrDefaultAsync();

            int numberForQuote = (latestQuote != null ? int.Parse(latestQuote.QuoteNumber.Split('-')[2]) : 0) + 1;

            quote.QuoteNumber = $"COT-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}-{numberForQuote:D4}";
            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            //Agrega cada item cotizado a la tabla de detalles
            foreach(QuoteDetail qd in quoteItems)
                qd.QuoteId = quote.QuoteId;

            await _context.QuotesDetail.AddRangeAsync(quoteItems);

            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Quote>> GetAllAsync()
        {
            return await _context.Quotes
                .Include(qd => qd.QuoteDetails)
                .Include(u => u.User)
                .ToListAsync();
        }

        public async Task<Quote?> GetById(int id)
        {
            return await _context.Quotes
                .Include(qd => qd.QuoteDetails)
                .Include(u => u.User)
                .FirstOrDefaultAsync(q => q.QuoteId == id);
        }

        public async Task<Quote?> GetLatestByClient(string client)
        {
            return await _context.Quotes
                .Include(u => u.User)
                .Include(qd => qd.QuoteDetails)
                .OrderByDescending(q => q.QuoteNumber)
                .FirstOrDefaultAsync(q => q.ClientId == client);
        }

        public async Task<IEnumerable<Quote>> FilterAsync(DateTime? startDate, DateTime? endDate, string searchTerm, string status)
        {
            var data = _context.Quotes
                .Include(qd => qd.QuoteDetails)
                .Include(u => u.User)
                .AsQueryable();

            Debug.WriteLine("This is a start:" + startDate);
            Debug.WriteLine("This is a end:" + endDate);
            Debug.WriteLine(DateTime.Now);

            //Verificar rango de fechas
            if (startDate != null && endDate != null)
            {
                //Valida si ambas fechas son lo mismo
                if(startDate.Value.Equals(endDate.Value))
                {
                    data = data.Where(q => q.QuoteDate.Date == startDate.Value);
                }
                else
                {
                    data = data.Where(q => q.QuoteDate.Date >= startDate.Value && q.QuoteDate.Date <= endDate.Value);
                }

            }
            
            //Verificar termino de busqueda
            if(searchTerm != "") 
            {
                data = data.Where(q => q.QuoteNumber.Contains(searchTerm) 
                || q.QuoteCustomer.Contains(searchTerm) 
                || q.User.UserName.Contains(searchTerm)
                || q.User.UserLastname.Contains(searchTerm));
            }

            
            //Verificar estado(cotizacion expirada o activa)
            if(status != "")
            {
                switch(status) 
                {
                    case "expired":
                        data = data.Where(q => DateTime.Now > q.QuoteValidDate || q.QuoteStatus == "expired");
                        break;
                    case "pending":
                        data = data.Where(q => DateTime.Now < q.QuoteValidDate && q.QuoteStatus == "pending");
                        break;
                    case "approved":
                        data = data.Where(q => DateTime.Now < q.QuoteValidDate && q.QuoteStatus == "approved");
                        break;
                    case "rejected":
                        data = data.Where(q => DateTime.Now < q.QuoteValidDate && q.QuoteStatus == "rejected");
                        break;
                    default:
                        break;
                }
            }
            

            return await data.ToListAsync();
        }

        //Actualiza cotizacion
        public async Task UpdateAsync(Quote quote, List<QuoteDetail> quoteItems)
        {
            _context.Quotes.Update(quote);
            await _context.SaveChangesAsync();

            List<QuoteDetail> existingQuoteItems = await _context.QuotesDetail.Where(q => q.QuoteId == quote.QuoteId).ToListAsync();

            //Elimina los items que no esta en la actualizacion
            foreach (QuoteDetail qd in existingQuoteItems)
            {
                if (!quoteItems.Contains(qd))
                {
                    _context.QuotesDetail.Remove(qd);
                }
            }

            await _context.QuotesDetail.AddRangeAsync(quoteItems);

            await _context.SaveChangesAsync();
        }
    }
}
