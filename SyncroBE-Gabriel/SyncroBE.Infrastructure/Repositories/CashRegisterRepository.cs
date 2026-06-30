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
    public class CashRegisterRepository : ICashRegisterRepository
    {
        private readonly SyncroDbContext _context;

        public CashRegisterRepository(SyncroDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CashRegister cashRegister)
        {
            //Establecimiento del numero de cuenta
            var latestRegister = await _context.CashRegisters.OrderByDescending(cr => cr.CashRegisterId).FirstOrDefaultAsync();

            int numberForRegister = (latestRegister != null ? int.Parse(latestRegister.CashRegisterNumber.Split('-')[2]) : 0) + 1;

            cashRegister.CashRegisterNumber = $"REG-{DateTime.Now.Year}{DateTime.Now.Month:D2}{DateTime.Now.Day:D2}-{numberForRegister:D4}";
            _context.CashRegisters.Add(cashRegister);

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CashRegister>> FilterAsync(DateTime? startDate, DateTime? endDate, string searchTerm, string status, User? user)
        {

            var data = _context.CashRegisters
                .Include(crm => crm.Movements)
                .ThenInclude(u => u.User)
                .Include(u => u.User)
                .AsQueryable();


            
            //Filtra los datos si el usuario es regular
            if (user.UserRole != "Administrador" && user.UserRole != "SuperUsuario")
            {
                data = data.Where(cr => cr.UserId == user.UserId);
            }
            

            //Verificar rango de fechas
            if (startDate != null && endDate != null)
            {
                //Valida si ambas fechas son lo mismo
                if (startDate.Value.Equals(endDate.Value))
                {
                    data = data.Where(cr => cr.CashRegisterOpeningDate.Date == startDate.Value);
                }
                else
                {
                    data = data.Where(cr => cr.CashRegisterOpeningDate.Date >= startDate.Value && cr.CashRegisterOpeningDate.Date <= endDate.Value);
                }

            }

            //Verificar termino de busqueda
            if (searchTerm != "")
            {
                data = data.Where(cr => cr.CashRegisterNumber.Contains(searchTerm)
                || cr.User.UserName.Contains(searchTerm)
                || cr.User.UserLastname.Contains(searchTerm));
            }


            //Verificar estado(cotizacion expirada o activa)
            if (status != "")
            {
                switch (status)
                {
                    case "open":
                        data = data.Where(cr => cr.CashRegisterStatus == "open");
                        break;
                    case "closed":
                        data = data.Where(cr => cr.CashRegisterStatus == "closed");
                        break;
                    default:
                        break;
                }
            }


            return await data.OrderDescending().ToListAsync();
        }

        public async Task<IEnumerable<CashRegister>> GetAllAsync(User? user)
        {
            var data = _context.CashRegisters
                .Include(crm => crm.Movements)
                .ThenInclude(u => u.User)
                .Include(u => u.User)
                .AsQueryable();


            //Filtra los datos si el usuario es regular
            if (user.UserRole != "Administrador" && user.UserRole != "SuperUsuario")
            {
                data = data.Where(cr => cr.UserId == user.UserId);
            }

            

            return await data.OrderDescending().ToListAsync();
        }

        public async Task<CashRegister?> GetById(int id)
        {
            return await _context.CashRegisters
                .Include(crm => crm.Movements)
                .Include(u => u.User)
                .FirstOrDefaultAsync(cr => cr.CashRegisterId == id);
        }

        public async Task<decimal> GetRegisterExpectedAmount(int id)
        {
            var register = await _context.CashRegisters
                .FirstOrDefaultAsync(cr => cr.CashRegisterId == id);

            var movements = await _context.CashRegisterMovements
                .Where(crm => crm.CashRegisterId == id)
                .ToListAsync();

            decimal expectedAmount = register.CashRegisterOpeningAmount;

            // Sumatoria de los montos de movimientos
            foreach(CashRegisterMovement c in movements)
            {
                switch(c.CashRegisterMovementType)
                {
                    case "income":
                        expectedAmount += c.CashRegisterMovementAmount;
                        break;
                    case "expense":
                        expectedAmount -= c.CashRegisterMovementAmount;
                        break;
                    default:
                        break;
                }

            }

            return expectedAmount;   
        }

        public async Task<IEnumerable<CashRegister>> GetUserOpenRegisters(int id)
        {
            return await _context.CashRegisters
                .Where(crm => crm.UserId == id && crm.CashRegisterStatus == "open")
                .ToListAsync();
        }

        public async Task CloseRegisterAsync(CashRegister register)
        {
            _context.CashRegisters.Update(register);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CashRegisterMovement>> FilterMovementsAsync(int id, DateTime? startDate, DateTime? endDate, string searchTerm, string type)
        {
            var data = _context.CashRegisterMovements
                .Include(cr => cr.CashRegister)
                .Include(u => u.User)
                .Include(p => p.Purchase)
                .Where(m => m.CashRegister.CashRegisterId == id)
                .AsQueryable();

            //Verificar rango de fechas
            if (startDate != null && endDate != null)
            {
                //Valida si ambas fechas son lo mismo
                if (startDate.Value.Equals(endDate.Value))
                {
                    data = data.Where(m => m.CashRegisterMovementDate.Date == startDate.Value);
                }
                else
                {
                    data = data.Where(m => m.CashRegisterMovementDate.Date >= startDate.Value && m.CashRegisterMovementDate.Date <= endDate.Value);
                }

            }

            //Verificar termino de busqueda
            if (searchTerm != "")
            {
                data = data.Where(m => m.CashRegisterMovementDescription.Contains(searchTerm)
                || m.User.UserName.Contains(searchTerm)
                || m.User.UserLastname.Contains(searchTerm));
            }


            //Verificar estado(cotizacion expirada o activa)
            if (type != "")
            {
                switch (type)
                {
                    case "income":
                        data = data.Where(m => m.CashRegisterMovementType == "income");
                        break;
                    case "expense":
                        data = data.Where(m => m.CashRegisterMovementType == "expense");
                        break;
                    default:
                        break;
                }
            }


            return await data.OrderDescending().ToListAsync();
        }

        public async Task AddManualMovementAsync(CashRegisterMovement movement)
        {
            _context.CashRegisterMovements.Add(movement);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CashRegisterMovement>> GetRegisterMovements(int id)
        {
            return await _context.CashRegisterMovements
                .Include(cr => cr.CashRegister)
                .Include(u => u.User)
                .Include(p => p.Purchase)
                .Where(m => m.CashRegisterId == id)
                .OrderDescending()
                .ToListAsync();
        }
    }
}
