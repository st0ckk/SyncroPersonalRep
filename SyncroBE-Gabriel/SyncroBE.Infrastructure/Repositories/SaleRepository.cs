using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Infrastructure.Repositories
{
    public class SaleRepository : ISaleRepository
    {
        private readonly SyncroDbContext _context;

        public SaleRepository(SyncroDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Purchase purchase, List<SaleDetail> saleItems)
        {

            Debug.WriteLine($"Obtuve el {purchase.ClientAccountId}");
            var productIds = saleItems.Select(sd => sd.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            //Establecimiento del numero de venta
            var latestSale = await _context.Purchases.OrderByDescending(p => p.PurchaseOrderNumber).FirstOrDefaultAsync();

            int numberForPurchase = (latestSale != null ? int.Parse(latestSale.PurchaseOrderNumber.Split('-')[2]) : 0) + 1;

            purchase.PurchaseOrderNumber = $"PUR-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}-{numberForPurchase:D4}";
            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            //Agrega cada item a la tabla de detalles
            foreach (SaleDetail sd in saleItems)
            {
                sd.PurchaseId = purchase.PurchaseId;
                var product = products.FirstOrDefault(p => p.ProductId == sd.ProductId);
                product.ProductQuantity -= sd.Quantity;
            }

            await _context.SaleDetails.AddRangeAsync(saleItems);

            await _context.SaveChangesAsync();

            //Si se pago con cuenta de credito, se ingresa el monto al balance
            if (purchase.ClientAccountId != null)
            {
                var account = await _context.ClientAccounts.FirstOrDefaultAsync(ca => ca.ClientAccountId == purchase.ClientAccountId);
                InsertIntoClientAccountMovements(purchase, account, false);
            }


            //Se agrega el movimiento a su caja asignada
            if (purchase.PurchasePaid)
            {
                purchase.IsActive = false;
                InsertIntoRegisterMovements(purchase);
            }

            await _context.SaveChangesAsync();

        }

        public async Task<IEnumerable<Purchase>> FilterAsync(DateTime? startDate, DateTime? endDate, string searchTerm, string state, string paidState)
        {
            var data = _context.Purchases
                .Include(sd => sd.SaleDetails)
                .Include(c => c.Client)
                .Include(u => u.User)
                .Include(i => i.Invoices)
                .Include(ca => ca.ClientAccount)
                .Include(cr => cr.Register)
                .AsQueryable();

            //Verificar rango de fechas
            if (startDate != null && endDate != null)
            {
                //Valida si ambas fechas son lo mismo
                if (startDate.Value.Equals(endDate.Value))
                {
                    data = data.Where(p => p.PurchaseDate.Date == startDate.Value);
                }
                else
                {
                    data = data.Where(p => p.PurchaseDate.Date >= startDate.Value && p.PurchaseDate.Date <= endDate.Value);
                }

            }

            //Verificar termino de busqueda
            if (searchTerm != "")
            {
                data = data.Where(p => p.ClientId.Contains(searchTerm)
                || p.PurchaseOrderNumber.Contains(searchTerm)
                || p.Client.ClientName.Contains(searchTerm)
                || p.User.UserName.Contains(searchTerm)
                || p.User.UserLastname.Contains(searchTerm));
            }

            //Verificar estado(venta inactiva o activa)
            switch (state)
            {
                case "active":
                    data = data.Where(p => p.IsActive);
                    break;

                case "inactive":
                    data = data.Where(p => !p.IsActive);
                    break;

                default:
                    break;
            }

            //Verificar estado de pago
            switch (paidState)
            {
                case "paid":
                    data = data.Where(p => p.PurchasePaid);
                    break;

                case "notPaid":
                    data = data.Where(p => !p.PurchasePaid);
                    break;

                default:
                    break;
            }

            return await data.OrderDescending().ToListAsync();
        }

        public async Task<IEnumerable<Purchase>> GetAllAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            // Verifica si el usuario es vendedor para sacar unicamente sus ventas
            if (user.UserRole == "Vendedor")
            {
                return await _context.Purchases
                    .Include(sd => sd.SaleDetails)
                    .Include(c => c.Client)
                    .Include(u => u.User)
                    .Include(i => i.Invoices)
                    .Include(ca => ca.ClientAccount)
                    .Include(cr => cr.Register)
                    .Where(u => u.UserId == user.UserId)
                    .OrderDescending()
                    .ToListAsync();
            }
            else
            {
                return await _context.Purchases
                    .Include(sd => sd.SaleDetails)
                    .Include(c => c.Client)
                    .Include(u => u.User)
                    .Include(i => i.Invoices)
                    .Include(ca => ca.ClientAccount)
                    .Include(cr => cr.Register)
                    .OrderDescending()
                    .ToListAsync();
            }


        }

        public async Task<Purchase?> GetById(int id)
        {
            return await _context.Purchases
                .Include(sd => sd.SaleDetails)
                .Include(c => c.Client)
                .Include(u => u.User)
                .Include(i => i.Invoices)
                .Include(ca => ca.ClientAccount)
                .Include(cr => cr.Register)
                .FirstOrDefaultAsync(p => p.PurchaseId == id);
        }

        public async Task UpdateAsync(Purchase purchase, List<SaleDetail> saleItems)
        {
            var product = new Product();
            var products = await _context.Products
                .Where(p => p.IsActive)
                .ToListAsync();

            //Si se pago con cuenta de credito, se ingresa el monto al balance
            if (purchase.ClientAccountId != null && purchase.PurchasePaid && purchase.IsActive)
            {
                var account = await _context.ClientAccounts.FirstOrDefaultAsync(ca => ca.ClientAccountId == purchase.ClientAccountId);
                InsertIntoClientAccountMovements(purchase, account,true);
            }

            //Se agrega el movimiento a su caja asignada
            if (purchase.PurchasePaid && purchase.IsActive)
            {
                purchase.IsActive = false;
                InsertIntoRegisterMovements(purchase);
            }
                

            _context.Purchases.Update(purchase);
            await _context.SaveChangesAsync();

            List<SaleDetail> existingSaleItems = await _context.SaleDetails.Where(s => s.PurchaseId == purchase.PurchaseId).ToListAsync();

            //Elimina los items que no esta en la actualizacion
            foreach (SaleDetail sd in existingSaleItems)
            {
                if (!saleItems.Contains(sd))
                {
                    product = products.FirstOrDefault(p => p.ProductId == sd.ProductId);
                    product.ProductQuantity += sd.Quantity;
                    _context.SaleDetails.Remove(sd);
                }
            }

            //Agrega cada item cotizado a la tabla de detalles
            foreach (SaleDetail sd in saleItems)
            {
                Debug.WriteLine(sd);
                product = products.FirstOrDefault(p => p.ProductId == sd.ProductId);
                product.ProductQuantity -= sd.Quantity;
            }

            await _context.SaleDetails.AddRangeAsync(saleItems);

            await _context.SaveChangesAsync();
        }

        public void InsertIntoRegisterMovements(Purchase sale)
        {
            var registerMovement = new CashRegisterMovement
            {
                CashRegisterId = sale.CashRegisterId,
                PurchaseId = sale.PurchaseId,
                UserId = sale.UserId,
                CashRegisterMovementType = "income",
                CashRegisterMovementDescription = $"Colocacion de venta - {sale.PurchaseOrderNumber}",
                CashRegisterMovementAmount = sale.Total,
            };
            _context.CashRegisterMovements.Add(registerMovement);
        }

        public void InsertIntoClientAccountMovements(Purchase sale, ClientAccount account, bool ifPaid)
        {
            var oldBalanceAmount = account.ClientAccountCurrentBalance;
            var accountMovement = new ClientAccountMovement();

            //Se agrega el movimiento
            if(ifPaid)
            {
                account.ClientAccountCurrentBalance = account.ClientAccountCurrentBalance - sale.Total;
                accountMovement = new ClientAccountMovement
                {
                    ClientAccountId = account.ClientAccountId,
                    ClientAccountMovementDate = DateTime.Now,
                    ClientAccountMovementDescription = $"Pago de Orden #{sale.PurchaseOrderNumber}",
                    ClientAccountMovementAmount = sale.Total,
                    ClientAccountMovementNewBalance = account.ClientAccountCurrentBalance,
                    ClientAccountMovementOldBalance = oldBalanceAmount,
                    ClientAccountMovementType = "credit",
                };
            }
            else
            {
                account.ClientAccountCurrentBalance = account.ClientAccountCurrentBalance + sale.Total;
                accountMovement = new ClientAccountMovement
                {
                    ClientAccountId = account.ClientAccountId,
                    ClientAccountMovementDate = DateTime.Now,
                    ClientAccountMovementDescription = $"Orden #{sale.PurchaseOrderNumber}",
                    ClientAccountMovementAmount = sale.Total,
                    ClientAccountMovementNewBalance = account.ClientAccountCurrentBalance,
                    ClientAccountMovementOldBalance = oldBalanceAmount,
                    ClientAccountMovementType = "debit",
                };
            }


                _context.ClientAccountMovements.Add(accountMovement);
        }

        public async Task<IEnumerable<Purchase>> ConfirmOrdersUnderAccount(string accountId)
        {
            string account = $"credit-{accountId}";
            return await _context.Purchases
                .Include(sd => sd.SaleDetails)
                .Include(c => c.Client)
                .Include(u => u.User)
                .Include(i => i.Invoices)
                .Include(ca => ca.ClientAccount)
                .Include(cr => cr.Register)
                .Where(p => p.PurchasePaymentMethod == account && p.IsActive)
                .ToListAsync();
        }
    }
}