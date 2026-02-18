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
    public class ClientRepository : IClientRepository
    {
        private readonly SyncroDbContext _context;

        public ClientRepository(SyncroDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Client>> GetActiveAsync()
        {
            return await _context.Clients
                .Include(c => c.Province)
                .Include(c => c.Canton)
                .Include(c => c.District)
                .Include(c => c.Location)
                .Where(c => c.IsActive)
                .ToListAsync();
        }


        public async Task<IEnumerable<Client>> GetInactiveAsync()
        {
            return await _context.Clients
                .Include(c => c.Province)
                .Include(c => c.Canton)
                .Include(c => c.District)
                .Include(c => c.Location)
                .Where(c => !c.IsActive)
                .ToListAsync();
        }


        public async Task<Client?> GetByIdAsync(string clientId)
        {
            return await _context.Clients
                .Include(c => c.Province)
                .Include(c => c.Canton)
                .Include(c => c.District)
                .Include(c => c.Location)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);
        }

        public async Task<IEnumerable<Client>> GetLookupAsync()
        {
            return await _context.Clients
                .AsNoTracking()
                .Include(c => c.Province)
                .Where(c => c.IsActive)
                .OrderBy(c => c.ClientName)
                .Select(c => new Client
                {
                    ClientId = c.ClientId,
                    ClientName = c.ClientName,
                    ClientType = c.ClientType,
                    Province = c.Province
                })
                .ToListAsync();
        }



        public async Task AddAsync(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Client client)
        {
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAsync(string clientId)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null) return;

            client.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task ActivateAsync(string clientId)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null) return;

            client.IsActive = true;
            await _context.SaveChangesAsync();
        }
    }
}
