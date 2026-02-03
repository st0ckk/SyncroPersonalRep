using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;
using System;
using System.Collections.Generic;
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

        //Traiga todas las cotizaciones, incluido los detalles
        public async Task<IEnumerable<Quote>> GetAllAsync()
        {
            
            return await _context.Quotes.
                Include(qd => qd.QuoteDetails).
                Include(c => c.Client).
                ToListAsync();
            
            //return await _context.Quotes.ToListAsync();
        }
    }
}
