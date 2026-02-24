using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.DTOs.Sale;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;
using System.Security.Claims;

namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/sales")]
    [Authorize(Roles = "SuperUsuario,Administrador,Vendedor")]
    public class SalesController : ControllerBase
    {
        private readonly SyncroDbContext _context;
        private readonly IAuditService _audit;

        public SalesController(SyncroDbContext context, IAuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        // ── GET: Listar ventas activas ──
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sales = await _context.Purchases
                .Include(p => p.Client)
                .Include(p => p.User)
                .Include(p => p.Tax)
                .Include(p => p.SaleDetails)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.PurchaseDate)
                .Select(p => new SaleDto
                {
                    PurchaseId = p.PurchaseId,
                    ClientId = p.ClientId,
                    ClientName = p.Client.ClientName,
                    UserName = p.User.UserName + " " + (p.User.UserLastname ?? ""),
                    PurchaseDate = p.PurchaseDate,
                    PurchasePaid = p.PurchasePaid,
                    TaxName = p.Tax != null ? p.Tax.TaxName : null,
                    TaxPercentage = p.TaxPercentage,
                    Subtotal = p.Subtotal,
                    TaxAmount = p.TaxAmount,
                    Total = p.Total,
                    IsActive = p.IsActive,
                    Details = p.SaleDetails.Select(d => new SaleDetailDto
                    {
                        SaleDetailId = d.SaleDetailId,
                        ProductId = d.ProductId,
                        ProductName = d.ProductName,
                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice,
                        LineTotal = d.LineTotal
                    }).ToList()
                })
                .ToListAsync();

            return Ok(sales);
        }

        // ── GET by Id ──
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _context.Purchases
                .Include(p => p.Client)
                .Include(p => p.User)
                .Include(p => p.Tax)
                .Include(p => p.SaleDetails)
                .FirstOrDefaultAsync(p => p.PurchaseId == id && p.IsActive);

            if (p == null) return NotFound("Venta no encontrada");

            var dto = new SaleDto
            {
                PurchaseId = p.PurchaseId,
                ClientId = p.ClientId,
                ClientName = p.Client.ClientName,
                UserName = p.User.UserName + " " + (p.User.UserLastname ?? ""),
                PurchaseDate = p.PurchaseDate,
                PurchasePaid = p.PurchasePaid,
                TaxName = p.Tax != null ? p.Tax.TaxName : null,
                TaxPercentage = p.TaxPercentage,
                Subtotal = p.Subtotal,
                TaxAmount = p.TaxAmount,
                Total = p.Total,
                IsActive = p.IsActive,
                Details = p.SaleDetails.Select(d => new SaleDetailDto
                {
                    SaleDetailId = d.SaleDetailId,
                    ProductId = d.ProductId,
                    ProductName = d.ProductName,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    LineTotal = d.LineTotal
                }).ToList()
            };

            return Ok(dto);
        }

        // ── POST: Crear venta ──
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSaleDto dto)
        {
            // Validar usuario del token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("No se pudo determinar el usuario");

            // Validar cliente
            var client = await _context.Clients.FindAsync(dto.ClientId);
            if (client == null || !client.IsActive)
                return BadRequest("Cliente no encontrado o inactivo");

            // Validar que hay al menos un detalle
            if (dto.Details == null || !dto.Details.Any())
                return BadRequest("La venta debe tener al menos un producto");

            // Validar impuesto si se envía
            decimal taxPercentage = 0;
            if (dto.TaxId.HasValue)
            {
                var tax = await _context.Taxes.FindAsync(dto.TaxId.Value);
                if (tax == null || !tax.IsActive)
                    return BadRequest("Impuesto no encontrado o inactivo");
                taxPercentage = tax.Percentage;
            }

            // Validar productos y stock
            var productIds = dto.Details.Select(d => d.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            var saleDetails = new List<SaleDetail>();
            decimal subtotal = 0;

            foreach (var detail in dto.Details)
            {
                var product = products.FirstOrDefault(p => p.ProductId == detail.ProductId);

                if (product == null || !product.IsActive)
                    return BadRequest($"Producto con ID {detail.ProductId} no encontrado o inactivo");

                if (detail.Quantity <= 0)
                    return BadRequest($"La cantidad para '{product.ProductName}' debe ser mayor a 0");

                if (product.ProductQuantity < detail.Quantity)
                    return BadRequest($"Stock insuficiente para '{product.ProductName}'. Disponible: {product.ProductQuantity}, solicitado: {detail.Quantity}");

                // Descontar stock
                product.ProductQuantity -= detail.Quantity;

                var lineTotal = product.ProductPrice * detail.Quantity;
                subtotal += lineTotal;

                saleDetails.Add(new SaleDetail
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Quantity = detail.Quantity,
                    UnitPrice = product.ProductPrice
                });
            }

            var taxAmount = subtotal * taxPercentage / 100;
            var total = subtotal + taxAmount;

            var purchase = new Purchase
            {
                UserId = userId,
                ClientId = dto.ClientId,
                PurchaseDate = DateTime.UtcNow,
                PurchasePaid = dto.PurchasePaid,
                TaxId = dto.TaxId,
                TaxPercentage = taxPercentage,
                Subtotal = subtotal,
                TaxAmount = taxAmount,
                Total = total,
                IsActive = true,
                SaleDetails = saleDetails
            };

            _context.Purchases.Add(purchase);

            // Actualizar conteo de compras del cliente
            client.ClientPurchases++;
            client.ClientLastPurchase = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _audit.LogAsync("Purchase", purchase.PurchaseId.ToString(), "SALE_CREATED",
                userId, $"Venta #{purchase.PurchaseId} creada. Total: {total:C}");

            return Ok(new
            {
                purchaseId = purchase.PurchaseId,
                total = purchase.Total,
                message = $"Venta #{purchase.PurchaseId} registrada exitosamente"
            });
        }
    }
}