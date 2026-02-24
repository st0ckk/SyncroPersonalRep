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
    public class DiscountRepository : IDiscountRepository
    {
        private readonly SyncroDbContext _context;

        public DiscountRepository(SyncroDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Discount>> GetAllAsync()
        {
            return await _context.Discounts
                .Where(d => d.IsActive)
                .ToListAsync();
        }

        public async Task<Discount?> GetById(int id)
        {
            return await _context.Discounts
                .FirstOrDefaultAsync(q => q.DiscountId == id);
        }

        public async Task<IEnumerable<Discount>> GetLookupAsync()
        {
            return await _context.Discounts
                                 .AsNoTracking()
                                 .Where(d => d.IsActive)
                                 .Select(d => new Discount
                                 {
                                     DiscountId = d.DiscountId,
                                     DiscountName = d.DiscountName,
                                     DiscountPercentage = d.DiscountPercentage
                                 })
                                 .ToListAsync();
        }
    }
}
