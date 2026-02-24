using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;

namespace SyncroBE.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly SyncroDbContext _context;

    public ProductRepository(SyncroDbContext context)
    {
        _context = context;
    }
    //visualizar todos los productos activos
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        var query = _context.Products.AsNoTracking();
        var sql = query.ToQueryString();
        Console.WriteLine(sql);
        return await _context.Products
                        .AsNoTracking()
                        .Include(p => p.Distributor) // 👈 importante
                        .Where(p => p.IsActive)
                        .ToListAsync();
    }
    //visualizar todos productos activo/inactivo
    public async Task<IEnumerable<Product>> GetAllIncludingInactiveAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .ToListAsync();
    }

    //visualizar desactivados
    public async Task<IEnumerable<Product>> GetInactiveAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Distributor)
            .Where(p => !p.IsActive)
            .ToListAsync();
    }




    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Distributor)
            .FirstOrDefaultAsync(p => p.ProductId == id);
    }


    public async Task AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null) return;

        product.IsActive = false;
        await _context.SaveChangesAsync();
    }

    //desactivar producto
    public async Task DeactivateAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return;

        product.IsActive = false;
        await _context.SaveChangesAsync();
    }

    //activar producto
    public async Task ActivateAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return;

        product.IsActive = true;
        await _context.SaveChangesAsync();
    }
    public async Task<IEnumerable<Product>> FilterAsync(
        string? name,
        int? distributorId)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Distributor)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(p =>
                p.ProductName.Contains(name));

        if (distributorId.HasValue)
            query = query.Where(p =>
                p.DistributorId == distributorId);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchAsync(string query)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Distributor)
            .Where(p => p.IsActive &&
                        p.ProductName.Contains(query))
            .Take(10)
            .ToListAsync();
    }
}
