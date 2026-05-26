using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.DTOs.Reports;
using SyncroBE.Infrastructure.Data;
using System.Text;

namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize(Roles = "SuperUsuario,Administrador,Vendedor,Chofer")]
    public class ReportsController : ControllerBase
    {
        private readonly SyncroDbContext _db;

        public ReportsController(SyncroDbContext db)
        {
            _db = db;
        }

        // ══════════════════════════════════════════
        // GET /api/reports/sales
        // Reporte de ventas con filtros
        // ══════════════════════════════════════════
        [HttpGet("sales")]
        public async Task<IActionResult> SalesReport(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? status = null,
            [FromQuery] string? paymentMethod = null,
            [FromQuery] string? paidStatus = null,
            [FromQuery] int? userId = null)
        {
            var query = _db.Purchases
                .Include(p => p.Client)
                .Include(p => p.User)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(p => p.PurchaseDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.PurchaseDate <= endDate.Value.Date.AddDays(1));

            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status == "active";
                query = query.Where(p => p.IsActive == isActive);
            }

            if (!string.IsNullOrEmpty(paymentMethod))
                query = query.Where(p => p.PurchasePaymentMethod == paymentMethod);

            if (!string.IsNullOrEmpty(paidStatus))
            {
                bool isPaid = paidStatus == "paid";
                query = query.Where(p => p.PurchasePaid == isPaid);
            }

            if (userId.HasValue)
                query = query.Where(p => p.UserId == userId.Value);

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

            return Ok(new SalesReportDto
            {
                Rows = rows,
                Summary = summary,
                TotalRows = rows.Count
            });
        }

        // ══════════════════════════════════════════
        // GET /api/reports/sales/export
        // ══════════════════════════════════════════
        [HttpGet("sales/export")]
        public async Task<IActionResult> ExportSalesReport(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? status = null,
            [FromQuery] string? paymentMethod = null,
            [FromQuery] string? paidStatus = null,
            [FromQuery] int? userId = null)
        {
            var query = _db.Purchases
                .Include(p => p.Client)
                .Include(p => p.User)
                .AsQueryable();

            if (startDate.HasValue) query = query.Where(p => p.PurchaseDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(p => p.PurchaseDate <= endDate.Value.Date.AddDays(1));
            if (!string.IsNullOrEmpty(status)) { bool a = status == "active"; query = query.Where(p => p.IsActive == a); }
            if (!string.IsNullOrEmpty(paymentMethod)) query = query.Where(p => p.PurchasePaymentMethod == paymentMethod);
            if (!string.IsNullOrEmpty(paidStatus)) { bool pd = paidStatus == "paid"; query = query.Where(p => p.PurchasePaid == pd); }
            if (userId.HasValue) query = query.Where(p => p.UserId == userId.Value);

            var data = await query.OrderByDescending(p => p.PurchaseDate).Take(100_000).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("id,orden,fecha,cliente_id,cliente,vendedor,subtotal,impuesto,total,metodo_pago,pagado,estado");
            foreach (var p in data)
            {
                sb.AppendLine($"{p.PurchaseId},{Esc(p.PurchaseOrderNumber)},{p.PurchaseDate:yyyy-MM-dd HH:mm},{p.ClientId},{Esc(p.Client.ClientName)},{Esc(p.User.UserName + " " + (p.User.UserLastname ?? ""))},{p.Subtotal},{p.TaxAmount},{p.Total},{p.PurchasePaymentMethod},{(p.PurchasePaid ? "Sí" : "No")},{(p.IsActive ? "Activa" : "Inactiva")}");
            }

            // Totales
            sb.AppendLine($",,,,,,{data.Sum(p => p.Subtotal)},{data.Sum(p => p.TaxAmount)},{data.Sum(p => p.Total)},,,");
            sb.AppendLine($",,,,,Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC,,,,,,");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv",
                $"reporte_ventas_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        // ══════════════════════════════════════════
        // GET /api/reports/inventory
        // Reporte de inventario
        // ══════════════════════════════════════════
        [HttpGet("inventory")]
        public async Task<IActionResult> InventoryReport(
            [FromQuery] string? status = null,
            [FromQuery] int? distributorId = null)
        {
            var query = _db.Products
                .Include(p => p.Distributor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status == "active";
                query = query.Where(p => p.IsActive == isActive);
            }

            if (distributorId.HasValue)
                query = query.Where(p => p.DistributorId == distributorId.Value);

            var data = await query.OrderBy(p => p.ProductName).ToListAsync();

            var rows = data.Select(p => new InventoryReportRow
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductType = p.ProductType ?? "—",
                Quantity = p.ProductQuantity,
                Price = p.ProductPrice,
                InventoryValue = p.ProductQuantity * p.ProductPrice,
                DistributorName = p.Distributor?.Name ?? "—",
                IsActive = p.IsActive
            }).ToList();

            var summary = new InventoryReportSummary
            {
                TotalProducts = rows.Count,
                TotalUnits = rows.Sum(r => r.Quantity),
                TotalInventoryValue = rows.Sum(r => r.InventoryValue),
                LowStockCount = rows.Count(r => r.Quantity > 0 && r.Quantity <= 10),
                OutOfStockCount = rows.Count(r => r.Quantity == 0)
            };

            return Ok(new InventoryReportDto
            {
                Rows = rows,
                Summary = summary
            });
        }

        // ══════════════════════════════════════════
        // GET /api/reports/inventory/export
        // ══════════════════════════════════════════
        [HttpGet("inventory/export")]
        public async Task<IActionResult> ExportInventoryReport(
            [FromQuery] string? status = null,
            [FromQuery] int? distributorId = null)
        {
            var query = _db.Products.Include(p => p.Distributor).AsQueryable();
            if (!string.IsNullOrEmpty(status)) { bool a = status == "active"; query = query.Where(p => p.IsActive == a); }
            if (distributorId.HasValue) query = query.Where(p => p.DistributorId == distributorId.Value);

            var data = await query.OrderBy(p => p.ProductName).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("id,producto,tipo,stock,precio_unitario,valor_inventario,distribuidor,estado");
            foreach (var p in data)
            {
                sb.AppendLine($"{p.ProductId},{Esc(p.ProductName)},{Esc(p.ProductType ?? "")},{p.ProductQuantity},{p.ProductPrice},{p.ProductQuantity * p.ProductPrice},{Esc(p.Distributor?.Name ?? "")},{(p.IsActive ? "Activo" : "Inactivo")}");
            }
            sb.AppendLine($",,Total,{data.Sum(p => p.ProductQuantity)},,{data.Sum(p => p.ProductQuantity * p.ProductPrice)},,");
            sb.AppendLine($",,,,,Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC,,");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv",
                $"reporte_inventario_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        // ══════════════════════════════════════════
        // GET /api/reports/invoices
        // Reporte de facturación electrónica
        // ══════════════════════════════════════════
        [HttpGet("invoices")]
        public async Task<IActionResult> InvoiceReport(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? haciendaStatus = null,
            [FromQuery] string? documentType = null)
        {
            var query = _db.Invoices
                .Include(i => i.Purchase).ThenInclude(p => p.Client)
                .Include(i => i.Purchase).ThenInclude(p => p.User)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(i => i.EmissionDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(i => i.EmissionDate <= endDate.Value.Date.AddDays(1));

            if (!string.IsNullOrEmpty(haciendaStatus))
                query = query.Where(i => i.HaciendaStatus == haciendaStatus);

            if (!string.IsNullOrEmpty(documentType))
                query = query.Where(i => i.DocumentType == documentType);

            var data = await query
                .OrderByDescending(i => i.EmissionDate)
                .Take(100_000)
                .ToListAsync();

            var rows = data.Select(i => new InvoiceReportRow
            {
                InvoiceId = i.InvoiceId,
                Clave = i.Clave,
                ConsecutiveNumber = i.ConsecutiveNumber,
                DocumentType = i.DocumentType,
                InvoiceTotal = i.InvoiceTotal,
                EmissionDate = i.EmissionDate,
                HaciendaStatus = i.HaciendaStatus,
                ClientName = i.Purchase?.Client?.ClientName ?? "—",
                UserName = i.Purchase?.User != null
                    ? i.Purchase.User.UserName + " " + (i.Purchase.User.UserLastname ?? "")
                    : "—",
                PurchaseId = i.PurchaseId,
                PurchaseOrderNumber = i.Purchase?.PurchaseOrderNumber ?? "—"
            }).ToList();

            var summary = new InvoiceReportSummary
            {
                TotalInvoices = rows.Count,
                TotalAmount = rows.Sum(r => r.InvoiceTotal),
                AcceptedCount = rows.Count(r => r.HaciendaStatus == "accepted"),
                RejectedCount = rows.Count(r => r.HaciendaStatus == "rejected"),
                PendingCount = rows.Count(r => r.HaciendaStatus != "accepted" && r.HaciendaStatus != "rejected")
            };

            return Ok(new InvoiceReportDto
            {
                Rows = rows,
                Summary = summary,
                TotalRows = rows.Count
            });
        }

        // ══════════════════════════════════════════
        // GET /api/reports/invoices/export
        // ══════════════════════════════════════════
        [HttpGet("invoices/export")]
        public async Task<IActionResult> ExportInvoiceReport(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? haciendaStatus = null,
            [FromQuery] string? documentType = null)
        {
            var query = _db.Invoices
                .Include(i => i.Purchase).ThenInclude(p => p.Client)
                .Include(i => i.Purchase).ThenInclude(p => p.User)
                .AsQueryable();

            if (startDate.HasValue) query = query.Where(i => i.EmissionDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(i => i.EmissionDate <= endDate.Value.Date.AddDays(1));
            if (!string.IsNullOrEmpty(haciendaStatus)) query = query.Where(i => i.HaciendaStatus == haciendaStatus);
            if (!string.IsNullOrEmpty(documentType)) query = query.Where(i => i.DocumentType == documentType);

            var data = await query.OrderByDescending(i => i.EmissionDate).Take(100_000).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("id,clave,consecutivo,tipo_doc,total,fecha_emision,estado_hacienda,cliente,vendedor,orden");
            foreach (var i in data)
            {
                sb.AppendLine($"{i.InvoiceId},{i.Clave},{i.ConsecutiveNumber},{i.DocumentType},{i.InvoiceTotal},{i.EmissionDate:yyyy-MM-dd HH:mm},{i.HaciendaStatus},{Esc(i.Purchase?.Client?.ClientName ?? "")},{Esc(i.Purchase?.User != null ? i.Purchase.User.UserName : "")},{i.Purchase?.PurchaseOrderNumber}");
            }
            sb.AppendLine($",,,,{data.Sum(i => i.InvoiceTotal)},,,,,");
            sb.AppendLine($",,,,Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC,,,,,");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv",
                $"reporte_facturacion_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        // ── Helper ──
        private static string Esc(string? v)
        {
            if (string.IsNullOrEmpty(v)) return "";
            if (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
                return $"\"{v.Replace("\"", "\"\"")}\"";
            return v;
        }
    }
}
