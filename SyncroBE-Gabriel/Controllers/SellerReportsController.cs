using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncroBE.API.Helpers;
using SyncroBE.Application.DTOs.Reports;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;
using System.Security.Claims;
using System.Text;

namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/seller-reports")]
    [Authorize(Roles = "SuperUsuario,Administrador,Vendedor,Chofer")]
    public class SellerReportsController : ControllerBase
    {
        private readonly SyncroDbContext _db;

        public SellerReportsController(SyncroDbContext db)
        {
            _db = db;
        }

        private int GetUserId()
        {
            var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(str, out var id) ? id : 0;
        }

        // Filtro de estado de la venta.
        //   "all"      → incluye ventas anuladas (IsActive == false)
        //   "inactive" → únicamente ventas anuladas
        //   resto      → únicamente ventas activas (comportamiento por defecto)
        private static IQueryable<Purchase> ApplyStatusFilter(IQueryable<Purchase> query, string? status)
        {
            return status switch
            {
                "all" => query,
                "inactive" => query.Where(p => !p.IsActive),
                _ => query.Where(p => p.IsActive),
            };
        }

        // ══════════════════════════════════════════
        // GET /api/seller-reports/my-sales
        // Reporte de ventas del vendedor logueado
        // ══════════════════════════════════════════
        [HttpGet("my-sales")]
        public async Task<IActionResult> MySales(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? paidStatus = null,
            [FromQuery] string? status = null,
            [FromQuery] string? clientId = null)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var query = _db.Purchases
                .Include(p => p.Client)
                .Include(p => p.User)
                .Where(p => p.UserId == userId);

            query = ApplyStatusFilter(query, status);

            if (startDate.HasValue)
                query = query.Where(p => p.PurchaseDate >= ReportDates.StartUtc(startDate.Value));

            if (endDate.HasValue)
                query = query.Where(p => p.PurchaseDate < ReportDates.EndUtcExclusive(endDate.Value));

            if (!string.IsNullOrEmpty(paidStatus))
            {
                bool isPaid = paidStatus == "paid";
                query = query.Where(p => p.PurchasePaid == isPaid);
            }

            if (!string.IsNullOrEmpty(clientId))
                query = query.Where(p => p.ClientId == clientId);

            var data = await query
                .OrderByDescending(p => p.PurchaseDate)
                .Take(100_000)
                .ToListAsync();

            var rows = data.Select(p => new SalesReportRow
            {
                PurchaseId = p.PurchaseId,
                PurchaseOrderNumber = p.PurchaseOrderNumber,
                PurchaseDate = p.PurchaseDate,
                ClientId = p.ClientId,
                ClientName = p.Client.ClientName,
                UserName = p.User.UserName + " " + (p.User.UserLastname ?? ""),
                UserId = p.UserId,
                Subtotal = p.Subtotal,
                TaxAmount = p.TaxAmount,
                Total = p.Total,
                PaymentMethod = p.PurchasePaymentMethod,
                IsPaid = p.PurchasePaid,
                IsActive = p.IsActive
            }).ToList();

            var summary = new SalesReportSummary
            {
                TotalSales = rows.Count,
                TotalSubtotal = rows.Sum(r => r.Subtotal),
                TotalTax = rows.Sum(r => r.TaxAmount),
                TotalRevenue = rows.Sum(r => r.Total),
                PaidCount = rows.Count(r => r.IsPaid),
                UnpaidCount = rows.Count(r => !r.IsPaid)
            };

            return Ok(new MySellerSalesReportDto
            {
                Rows = rows,
                Summary = summary,
                TotalRows = rows.Count
            });
        }

        // ══════════════════════════════════════════
        // GET /api/seller-reports/my-sales/export
        // ══════════════════════════════════════════
        [HttpGet("my-sales/export")]
        public async Task<IActionResult> ExportMySales(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? paidStatus = null,
            [FromQuery] string? status = null)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var query = _db.Purchases
                .Include(p => p.Client)
                .Where(p => p.UserId == userId);

            query = ApplyStatusFilter(query, status);

            if (startDate.HasValue) query = query.Where(p => p.PurchaseDate >= ReportDates.StartUtc(startDate.Value));
            if (endDate.HasValue) query = query.Where(p => p.PurchaseDate < ReportDates.EndUtcExclusive(endDate.Value));
            if (!string.IsNullOrEmpty(paidStatus)) { bool pd = paidStatus == "paid"; query = query.Where(p => p.PurchasePaid == pd); }

            var data = await query.OrderByDescending(p => p.PurchaseDate).Take(100_000).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("orden,fecha,cliente,subtotal,impuesto,total,metodo_pago,pagado");
            foreach (var p in data)
                sb.AppendLine($"{Esc(p.PurchaseOrderNumber)},{p.PurchaseDate:yyyy-MM-dd},{Esc(p.Client.ClientName)},{p.Subtotal},{p.TaxAmount},{p.Total},{p.PurchasePaymentMethod},{(p.PurchasePaid ? "Sí" : "No")}");

            sb.AppendLine($",,,{data.Sum(p => p.Subtotal)},{data.Sum(p => p.TaxAmount)},{data.Sum(p => p.Total)},,");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv",
                $"mis_ventas_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        // ══════════════════════════════════════════
        // GET /api/seller-reports/my-top-products
        // Top productos más vendidos por este vendedor
        // ══════════════════════════════════════════
        [HttpGet("my-top-products")]
        public async Task<IActionResult> MyTopProducts(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? productType = null)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var query = _db.SaleDetails
                .Include(sd => sd.Purchase)
                .Include(sd => sd.Product)
                .Where(sd => sd.Purchase.UserId == userId && sd.Purchase.IsActive);

            if (startDate.HasValue)
                query = query.Where(sd => sd.Purchase.PurchaseDate >= ReportDates.StartUtc(startDate.Value));

            if (endDate.HasValue)
                query = query.Where(sd => sd.Purchase.PurchaseDate < ReportDates.EndUtcExclusive(endDate.Value));

            if (!string.IsNullOrEmpty(productType))
                query = query.Where(sd => sd.Product.ProductType == productType);

            var grouped = await query
                .GroupBy(sd => new { sd.ProductId, sd.ProductName, sd.Product.ProductType })
                .Select(g => new SellerProductRow
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    ProductType = g.Key.ProductType ?? "—",
                    UnitsSold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.LineTotal),
                    AvgUnitPrice = g.Average(x => x.UnitPrice)
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(50)
                .ToListAsync();

            return Ok(new MyTopProductsReportDto
            {
                Rows = grouped,
                TotalUnitsSold = grouped.Sum(x => x.UnitsSold),
                TotalRevenue = grouped.Sum(x => x.Revenue)
            });
        }

        // ══════════════════════════════════════════
        // GET /api/seller-reports/my-top-clients
        // Top clientes de este vendedor
        // ══════════════════════════════════════════
        [HttpGet("my-top-clients")]
        public async Task<IActionResult> MyTopClients(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var query = _db.Purchases
                .Include(p => p.Client)
                .Where(p => p.UserId == userId && p.IsActive);

            if (startDate.HasValue)
                query = query.Where(p => p.PurchaseDate >= ReportDates.StartUtc(startDate.Value));

            if (endDate.HasValue)
                query = query.Where(p => p.PurchaseDate < ReportDates.EndUtcExclusive(endDate.Value));

            var grouped = await query
                .GroupBy(p => new { p.ClientId, p.Client.ClientName, p.Client.ClientType })
                .Select(g => new SellerClientRow
                {
                    ClientId = g.Key.ClientId,
                    ClientName = g.Key.ClientName,
                    ClientType = g.Key.ClientType ?? "—",
                    TotalPurchases = g.Count(),
                    TotalSpent = g.Sum(x => x.Total),
                    LastPurchaseDate = g.Max(x => x.PurchaseDate)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(50)
                .ToListAsync();

            return Ok(new MyTopClientsReportDto
            {
                Rows = grouped,
                TotalClients = grouped.Count,
                TotalRevenue = grouped.Sum(x => x.TotalSpent)
            });
        }

        private static string Esc(string? v)
        {
            if (string.IsNullOrEmpty(v)) return "";
            if (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
                return $"\"{v.Replace("\"", "\"\"")}\"";
            return v;
        }
    }
}
