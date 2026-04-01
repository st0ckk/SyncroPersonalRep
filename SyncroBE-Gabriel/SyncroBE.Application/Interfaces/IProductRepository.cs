using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncroBE.Domain.Entities;


namespace SyncroBE.Application.Interfaces
{
    //**
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetAllIncludingInactiveAsync();
        Task<IEnumerable<Product>> GetInactiveAsync();

        Task<Product?> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);

        Task DeactivateAsync(int id);
        Task ActivateAsync(int id);
        Task<IEnumerable<Product>> FilterAsync(string? name, int? distributorId);
        Task<IEnumerable<Product>> SearchAsync(string query);

    }

}
