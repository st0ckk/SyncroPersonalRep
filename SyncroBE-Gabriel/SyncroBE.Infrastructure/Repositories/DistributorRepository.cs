using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncroBE.Infrastructure.Repositories
{
    public class DistributorRepository : IDistributorRepository
    {
        private readonly SyncroDbContext _context;

        public DistributorRepository(SyncroDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Distributor>> GetAllAsync()
        {
            return await _context.Distributors
                                 .AsNoTracking()
                                 .Where(d => d.IsActive)
                                 .OrderBy(d => d.Name)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Distributor>> GetLookupAsync()
        {
            return await _context.Distributors
                                 .AsNoTracking()
                                 .Where(d => d.IsActive)
                                 .Select(d => new Distributor
                                 {
                                     DistributorId = d.DistributorId,
                                     DistributorCode = d.DistributorCode,
                                     Name = d.Name
                                 })
                                 .ToListAsync();
        }

        public async Task<Distributor?> GetByIdAsync(int id)
        {
            return await _context.Distributors
                                 .FirstOrDefaultAsync(d => d.DistributorId == id && d.IsActive);
        }

        public async Task<Distributor?> GetByIdIncludingInactiveAsync(int id)
        {
            return await _context.Distributors
                                 .FirstOrDefaultAsync(d => d.DistributorId == id);
        }

        public async Task<bool> CodeExistsAsync(string distributorCode)
        {
            return await _context.Distributors
                                 .AnyAsync(d => d.DistributorCode == distributorCode);
        }

        public async Task AddAsync(Distributor distributor)
        {
            _context.Distributors.Add(distributor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Distributor distributor)
        {
            _context.Distributors.Update(distributor);
            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAsync(Distributor distributor)
        {
            distributor.IsActive = false;
            _context.Distributors.Update(distributor);
            await _context.SaveChangesAsync();
        }

        public async Task ActivateAsync(Distributor distributor)
        {
            distributor.IsActive = true;
            _context.Distributors.Update(distributor);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Distributor>> GetInactiveAsync()
        {
            return await _context.Distributors
                                 .AsNoTracking()
                                 .Where(d => !d.IsActive)
                                 .OrderBy(d => d.Name)
                                 .ToListAsync();
        }

    }
}