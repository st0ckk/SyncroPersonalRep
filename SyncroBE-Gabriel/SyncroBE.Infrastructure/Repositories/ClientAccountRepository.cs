using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Infrastructure.Repositories
{
    public class ClientAccountRepository : IClientAccountRepository
    {
        private readonly SyncroDbContext _context;
        public ClientAccountRepository(SyncroDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ClientAccount account)
        {
            //Establecimiento del numero de cuenta
            var latestAccount = await _context.ClientAccounts.OrderByDescending(ca => ca.ClientAccountNumber).FirstOrDefaultAsync();

            int numberForAccount = (latestAccount != null ? int.Parse(latestAccount.ClientAccountNumber.Split('-')[2]) : 0) + 1;

            account.ClientAccountNumber = $"CCA-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}-{numberForAccount:D4}";
            _context.ClientAccounts.Add(account);

            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<ClientAccount>> FilterAsync(DateTime? startDate, DateTime? endDate, string searchTerm, string status)
        {
            var data = _context.ClientAccounts
                .Include(c => c.Client)
                .Include(u => u.User)
                .Include(m => m.Movements)
                .AsQueryable();

            //Verificar rango de fechas
            if (startDate != null && endDate != null)
            {
                //Valida si ambas fechas son lo mismo
                if (startDate.Value.Equals(endDate.Value))
                {
                    data = data.Where(ca => ca.ClientAccountOpeningDate.Date == startDate.Value);
                }
                else
                {
                    data = data.Where(ca => ca.ClientAccountOpeningDate.Date >= startDate.Value && ca.ClientAccountOpeningDate.Date <= endDate.Value);
                }

            }

            //Verificar termino de busqueda
            if (searchTerm != "")
            {
                data = data.Where(ca => ca.ClientAccountNumber.Contains(searchTerm)
                || ca.User.UserName.Contains(searchTerm)
                || ca.User.UserLastname.Contains(searchTerm)
                || ca.Client.ClientName.Contains(searchTerm)
                || ca.Client.ClientId.Contains(searchTerm));
            }


            //Verificar estado(cotizacion expirada o activa)
            if (status != "")
            {
                switch (status)
                {
                    case "active":
                        data = data.Where(ca => ca.ClientAccountStatus == "active");
                        break;
                    case "suspended":
                        data = data.Where(ca => ca.ClientAccountStatus == "suspended");
                        break;
                    case "closed":
                        data = data.Where(ca => ca.ClientAccountStatus == "closed");
                        break;
                    default:
                        break;
                }
            }


            return await data.ToListAsync();
        }

        public async Task<IEnumerable<ClientAccountMovement>> FilterMovementsAsync(int id, DateTime? startDate, DateTime? endDate, string searchTerm, string type)
        {
            var data = _context.ClientAccountMovements
                .Include(ca => ca.ClientAccount)
                .Where(m => m.ClientAccount.ClientAccountId == id)
                .AsQueryable();

            //Verificar rango de fechas
            if (startDate != null && endDate != null)
            {
                //Valida si ambas fechas son lo mismo
                if (startDate.Value.Equals(endDate.Value))
                {
                    data = data.Where(m => m.ClientAccountMovementDate.Date == startDate.Value);
                }
                else
                {
                    data = data.Where(m => m.ClientAccountMovementDate.Date >= startDate.Value && m.ClientAccountMovementDate.Date <= endDate.Value);
                }

            }

            //Verificar termino de busqueda
            if (searchTerm != "")
            {
                data = data.Where(m => m.ClientAccountMovementDescription.Contains(searchTerm));
            }


            //Verificar estado(cotizacion expirada o activa)
            if (type != "")
            {
                switch (type)
                {
                    case "debit":
                        data = data.Where(m => m.ClientAccountMovementType == "debit");
                        break;
                    case "credit":
                        data = data.Where(m => m.ClientAccountMovementType == "credit");
                        break;
                    default:
                        break;
                }
            }


            return await data.ToListAsync();
        }

        public async Task<IEnumerable<ClientAccount>> GetAllActiveAsync()
        {
             return await _context.ClientAccounts
                .Include(c => c.Client)
                .Include(u => u.User)
                .Include(m => m.Movements)
                .Where(ca => ca.ClientAccountStatus == "active" 
                || ca.ClientAccountStatus == "suspended")
                .ToListAsync();
        }

        public async Task<IEnumerable<ClientAccount>> GetAllAsync()
        {
            return await _context.ClientAccounts
                .Include(c => c.Client)
                .Include(u => u.User)
                .Include(m => m.Movements)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClientAccount>> GetByClient(string client)
        {
            return await _context.ClientAccounts
               .Include(c => c.Client)
               .Include(u => u.User)
               .Include(m => m.Movements)
               .Where(ca => ca.ClientId == client)
               .ToListAsync();
        }

        public async Task<ClientAccount?> GetById(int id)
        {
            return await _context.ClientAccounts
                .Include(c => c.Client)
                .Include(u => u.User)
                .Include(m => m.Movements)
                .FirstOrDefaultAsync(ca => ca.ClientAccountId == id);
        }
        public async Task UpdateAsync(ClientAccount account)
        {
            _context.ClientAccounts.Update(account);
            await _context.SaveChangesAsync();
        }

        public async Task CloseAccountAsync(ClientAccount account)
        {
            account.ClientAccountStatus = "closed";
            _context.ClientAccounts.Update(account);
            await _context.SaveChangesAsync();
        }

    }
}
