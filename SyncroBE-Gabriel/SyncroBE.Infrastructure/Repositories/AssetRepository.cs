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
    public class AssetRepository : IAssetRepository
    {
        private readonly SyncroDbContext _context;

        public AssetRepository(SyncroDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Asset>> GetAllAsync()
        {
            return await _context.Assets
                .AsNoTracking()
                .Include(a => a.User)
                .Where(a => a.IsActive)
                .OrderBy(a => a.AssetName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> GetByUserIdAsync(int userId)
        {
            return await _context.Assets
                .AsNoTracking()
                .Include(a => a.User)
                .Where(a => a.UserId == userId && a.IsActive)
                .OrderBy(a => a.AssetName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> GetInactiveAsync()
        {
            return await _context.Assets
                .AsNoTracking()
                .Include(a => a.User)
                .Where(a => !a.IsActive)
                .OrderBy(a => a.AssetName)
                .ToListAsync();
        }

        public async Task<Asset?> GetByIdAsync(int id)
        {
            return await _context.Assets
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AssetId == id);
        }

        public async Task AddAsync(Asset asset)
        {
            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Asset asset)
        {
            _context.Assets.Update(asset);
            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return;

            asset.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task ActivateAsync(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return;

            asset.IsActive = true;
            await _context.SaveChangesAsync();
        }
    }
}