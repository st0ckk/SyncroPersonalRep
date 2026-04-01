using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.DTOs.ElectronicInvoice;
using SyncroBE.Application.DTOs.ClientAccount;
using SyncroBE.Application.DTOs.Quote;
using SyncroBE.Application.DTOs.QuoteDetails;
using SyncroBE.Application.DTOs.Sale;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/sales")]
    [Authorize(Roles = "SuperUsuario,Administrador,Vendedor")]
    public class SalesController : ControllerBase
    {
        private readonly ISaleRepository _repository;
        private readonly SyncroDbContext _context;
        private readonly IAuditService _audit;
        private readonly IElectronicInvoiceService _electronicInvoice;
        private readonly ILogger<SalesController> _logger;

        public SalesController(
            ISaleRepository repository,
            SyncroDbContext context,
            IAuditService audit,
            IElectronicInvoiceService electronicInvoice,
            ILogger<SalesController> logger)
        {
            _repository = repository;
            _context = context;
            _audit = audit;
            _electronicInvoice = electronicInvoice;
            _logger = logger;
        }

        // ── GET: Listar ventas activas ──
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Validar usuario del token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("No se pudo determinar el usuario");

            var data = await _repository.GetAllAsync(userId);
            var result = data.Select(MapToSaleDto);

            return Ok(result);
        }

        // ── GET by Id ──
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _repository.GetById(id);
            if (data == null)
                return NotFound();

            var dto = MapToSaleDto(data);

            return Ok(dto);
        }

        //Consigue todas las ventas
        [HttpGet("filter")]
        public async Task<IActionResult> Filter(DateTime? startDate, DateTime? endDate, string searchTerm = "", string status = "", string paidStatus = "")
        {
            var data = await _repository.FilterAsync(startDate, endDate, searchTerm, status, paidStatus);
            var result = data.Select(MapToSaleDto);

            return Ok(result);
        }


        // ── POST: Crear venta ──
        [HttpPost]
        public async Task<IActionResult> Create(CreateUpdateSaleDto dto)
        {
            Debug.WriteLine(dto.ClientId);
            // Validar usuario del token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("No se pudo determinar el usuario");
            var saleDetails = new List<SaleDetail>();

            var sale = new Purchase
            {
                UserId = userId,
                ClientId = dto.ClientId,
                DiscountId = dto.DiscountId,
                RouteId = dto.RouteId,
                ClientAccountId = dto.ClientAccountId,
                PurchaseOrderNumber = dto.PurchaseOrderNumber,
                PurchasePaid = dto.PurchasePaid,
                TaxId = dto.TaxId,
                TaxPercentage = dto.TaxPercentage,
                Subtotal = dto.Subtotal,
                TaxAmount = dto.TaxAmount,
                Total = dto.Total,
                PurchaseDiscountApplied = dto.PurchaseDiscountApplied,
                PurchaseDiscountPercentage = dto.PurchaseDiscountPercentage,
                PurchaseDiscountReason = dto.PurchaseDiscountReason,
                PurchasePaymentMethod = dto.PurchasePaymentMethod,
            };


            foreach (CreateUpdateSaleDetailDto item in dto.saleDetails)
            {
                saleDetails.Add(new SaleDetail
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.LineTotal
                });
            }

            await _repository.AddAsync(sale, saleDetails);

            // ── Electronic Invoice (optional) ──
            ElectronicInvoiceDto? invoiceResult = null;
            if (dto.GenerateElectronicInvoice)
            {
                try
                {
                    var invoice = await _electronicInvoice.GenerateAndSendAsync(
                        sale.PurchaseId, dto.ElectronicInvoiceDocumentType);

                    invoiceResult = new ElectronicInvoiceDto
                    {
                        InvoiceId = invoice.InvoiceId,
                        PurchaseId = invoice.PurchaseId,
                        Clave = invoice.Clave,
                        ConsecutiveNumber = invoice.ConsecutiveNumber,
                        DocumentType = invoice.DocumentType,
                        HaciendaStatus = invoice.HaciendaStatus,
                        HaciendaMessage = invoice.HaciendaMessage,
                        EmissionDate = invoice.EmissionDate,
                        SentAt = invoice.SentAt,
                        InvoiceTotal = invoice.InvoiceTotal,
                        CreatedAt = invoice.CreatedAt
                    };

                    _logger.LogInformation(
                        "Electronic invoice generated for purchase {PurchaseId}: {Clave} ({Status})",
                        sale.PurchaseId, invoice.Clave, invoice.HaciendaStatus);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to generate electronic invoice for purchase {PurchaseId}. Sale was saved successfully.",
                        sale.PurchaseId);

                    // Sale is still OK — invoice failure shouldn't block the sale
                    invoiceResult = new ElectronicInvoiceDto
                    {
                        PurchaseId = sale.PurchaseId,
                        HaciendaStatus = "error",
                        HaciendaMessage = $"Error al generar factura: {ex.Message}"
                    };
                }
            }

            return Ok(new
            {
                message = "Venta registrada exitosamente",
                purchaseId = sale.PurchaseId,
                purchaseOrderNumber = sale.PurchaseOrderNumber,
                electronicInvoice = invoiceResult
            });
        }

        // ── UPDATE: Actualizar venta y gestionar inventario ──
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, CreateUpdateSaleDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ValidationProblem(ModelState);

                var sale = await _repository.GetById(id);
                if (sale is null)
                    return NotFound();

                var saleItems = new List<SaleDetail>();

                // mappeo
                sale.DiscountId = dto.DiscountId;
                sale.RouteId = dto.RouteId;
                sale.PurchasePaid = dto.PurchasePaid;
                sale.TaxId = dto.TaxId;
                sale.TaxPercentage = dto.TaxPercentage;
                sale.Subtotal = dto.Subtotal;
                sale.TaxAmount = dto.TaxAmount;
                sale.Total = dto.Total;
                sale.PurchaseDiscountApplied = dto.PurchaseDiscountApplied;
                sale.PurchaseDiscountPercentage = dto.PurchaseDiscountPercentage;
                sale.PurchaseDiscountReason = dto.PurchaseDiscountReason;
                sale.PurchasePaymentMethod = dto.PurchasePaymentMethod;

                foreach(CreateUpdateSaleDetailDto item in dto.saleDetails)
                {
                    saleItems.Add(new SaleDetail
                    {
                        PurchaseId = sale.PurchaseId,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.LineTotal
                    });
                }



                await _repository.UpdateAsync(sale, saleItems);
                return Ok();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return BadRequest();
            }
        }

        // ── DELETE: Eliminar venta (soft-delete) y restaurar inventario ──
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Validar usuario del token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("No se pudo determinar el usuario");

            // Buscar la venta con sus detalles
            var purchase = await _context.Purchases
                .Include(p => p.SaleDetails)
                .FirstOrDefaultAsync(p => p.PurchaseId == id && p.IsActive);

            if (purchase == null)
                return NotFound("Venta no encontrada o ya fue eliminada");

            // Restaurar stock de cada producto
            foreach (var detail in purchase.SaleDetails)
            {
                var product = await _context.Products.FindAsync(detail.ProductId);
                if (product != null)
                {
                    product.ProductQuantity += detail.Quantity;
                }
            }

            // Revertir conteo de compras del cliente
            var client = await _context.Clients.FindAsync(purchase.ClientId);
            if (client != null && client.ClientPurchases > 0)
            {
                client.ClientPurchases--;
            }

            // Marcar como inactiva
            purchase.IsActive = false;

            await _context.SaveChangesAsync();

            // Auditoria basica (placeholder para implementacion real)
            await _audit.LogAsync("Purchase", purchase.PurchaseId.ToString(), "SALE_DELETED",
                userId, $"Venta #{purchase.PurchaseId} eliminada. Total: {purchase.Total:C}. Stock restaurado.");

            //Si se pago con cuenta de credito, se ingresa el monto al balance
            if (purchase.ClientAccountId != null)
            {
                var account = await _context.ClientAccounts.FirstOrDefaultAsync(ca => ca.ClientAccountId == purchase.ClientAccountId);
                var oldBalanceAmount = account.ClientAccountCurrentBalance;
                account.ClientAccountCurrentBalance -= purchase.Total;

                //Se agrega el movimiento
                var movement = new ClientAccountMovement
                {
                    ClientAccountId = account.ClientAccountId,
                    ClientAccountMovementDate = DateTime.Now,
                    ClientAccountMovementDescription = $"Orden #{purchase.PurchaseOrderNumber}",
                    ClientAccountMovementAmount = purchase.Total,
                    ClientAccountMovementNewBalance = account.ClientAccountCurrentBalance,
                    ClientAccountMovementOldBalance = oldBalanceAmount,
                    ClientAccountMovementType = "credit",
                };

                _context.ClientAccountMovements.Add(movement);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = $"Venta #{purchase.PurchaseId} eliminada exitosamente. Inventario restaurado.",
                purchaseId = purchase.PurchaseId
            });
        }

        // ── Helper: Map Purchase → SaleDto ──
        private static SaleDto MapToSaleDto(Purchase p)
        {
            return new SaleDto
            {
                PurchaseId = p.PurchaseId,
                ClientId = p.ClientId,
                DiscountId = p.DiscountId,
                RouteId = p.RouteId,
                ClientAccountId = p.ClientAccountId,
                ClientName = p.Client.ClientName,
                UserName = p.User.UserName + " " + (p.User.UserLastname ?? ""),
                PurchaseOrderNumber = p.PurchaseOrderNumber,
                PurchaseDate = p.PurchaseDate,
                PurchasePaid = p.PurchasePaid,
                TaxName = p.Tax != null ? p.Tax.TaxName : null,
                TaxPercentage = p.TaxPercentage,
                Subtotal = p.Subtotal,
                TaxAmount = p.TaxAmount,
                Total = p.Total,
                IsActive = p.IsActive,
                PurchaseDiscountApplied = p.PurchaseDiscountApplied,
                PurchaseDiscountPercentage = p.PurchaseDiscountPercentage,
                PurchaseDiscountReason = p.PurchaseDiscountReason,
                PurchasePaymentMethod = p.PurchasePaymentMethod,

                // Electronic invoice info
                InvoiceId = p.Invoice?.InvoiceId,
                InvoiceClave = p.Invoice?.Clave,
                InvoiceHaciendaStatus = p.Invoice?.HaciendaStatus,
                InvoiceConsecutiveNumber = p.Invoice?.ConsecutiveNumber,

                saleDetails = p.SaleDetails.Select(d => new SaleDetailDto
                {
                    SaleDetailId = d.SaleDetailId,
                    ProductId = d.ProductId,
                    ProductName = d.ProductName,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    LineTotal = d.LineTotal
                }).ToList()
            };
        }
    }
}