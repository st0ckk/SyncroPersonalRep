using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.DTOs.Dashboard;
using SyncroBE.Infrastructure.Data;
using System.Security.Claims;

namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly SyncroDbContext _db;
        private const int LOW_STOCK_THRESHOLD = 10;

        public DashboardController(SyncroDbContext db)
        {
            _db = db;
        }

        // ══════════════════════════════════════════
        // GET /api/dashboard/admin
        // Solo SuperUsuario y Administrador
        // ══════════════════════════════════════════
        [HttpGet("admin")]
        [Authorize(Roles = "SuperUsuario,Administrador")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var weekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var sevenDaysAgo = todayStart.AddDays(-6);

            // ── KPI: Usuarios activos (con login en los últimos 30 días) ──
            var activeUsers = await _db.Users
                .CountAsync(u => u.IsActive && u.LastLogin != null
                    && u.LastLogin >= now.AddDays(-30));

            // ── KPI: Ventas hoy / semana / mes ──
            var activeSales = _db.Purchases.Where(p => p.IsActive);

            var salesToday = await activeSales
                .CountAsync(p => p.PurchaseDate >= todayStart);

            var salesThisWeek = await activeSales
                .CountAsync(p => p.PurchaseDate >= weekStart);

            var salesThisMonth = await activeSales
                .CountAsync(p => p.PurchaseDate >= monthStart);

            var revenueToday = await activeSales
                .Where(p => p.PurchaseDate >= todayStart)
                .SumAsync(p => (decimal?)p.Total) ?? 0;

            var revenueThisWeek = await activeSales
                .Where(p => p.PurchaseDate >= weekStart)
                .SumAsync(p => (decimal?)p.Total) ?? 0;

            var revenueThisMonth = await activeSales
                .Where(p => p.PurchaseDate >= monthStart)
                .SumAsync(p => (decimal?)p.Total) ?? 0;

            // ── KPI: Productos con bajo stock ──
            var lowStockCount = await _db.Products
                .CountAsync(p => p.IsActive && p.ProductQuantity <= LOW_STOCK_THRESHOLD);

            // ── Top 5 clientes por total gastado (mes actual) ──
            var topClients = await activeSales
                .Include(p => p.Client)
                .Where(p => p.PurchaseDate >= monthStart)
                .GroupBy(p => new { p.ClientId, p.Client.ClientName })
                .Select(g => new TopClientDto
                {
                    ClientId = g.Key.ClientId,
                    ClientName = g.Key.ClientName,
                    TotalPurchases = g.Count(),
                    TotalSpent = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(5)
                .ToListAsync();

            // ── Productos bajo stock (detalle) ──
            var lowStockList = await _db.Products
                .Include(p => p.Distributor)
                .Where(p => p.IsActive && p.ProductQuantity <= LOW_STOCK_THRESHOLD)
                .OrderBy(p => p.ProductQuantity)
                .Take(10)
                .Select(p => new LowStockProductDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Quantity = p.ProductQuantity,
                    DistributorName = p.Distributor.Name
                })
                .ToListAsync();

            // ── Ventas últimos 7 días ──
            var salesLast7Raw = await activeSales
                .Where(p => p.PurchaseDate >= sevenDaysAgo)
                .Select(p => new { p.PurchaseDate, p.Total })
                .ToListAsync();

            var salesLast7 = salesLast7Raw
                .GroupBy(p => p.PurchaseDate.Date)
                .Select(g => new SalesByDayDto
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Count = g.Count(),
                    Total = g.Sum(x => x.Total)
                })
                .OrderBy(x => x.Date)
                .ToList();

            // ── Top 10 productos más vendidos (mes actual) ──
            var topProducts = await _db.SaleDetails
                .Where(sd => sd.Purchase.IsActive && sd.Purchase.PurchaseDate >= monthStart)
                .GroupBy(sd => new { sd.ProductId, sd.ProductName })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    UnitsSold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.LineTotal)
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(10)
                .ToListAsync();

            // ── Actividad reciente (audit_log, últimos 20) ──
            var recentActivity = await _db.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Take(20)
                .Select(a => new RecentAuditDto
                {
                    LogId = a.LogId,
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    Details = a.Details,
                    UserName = a.User.UserName + " " + (a.User.UserLastname ?? ""),
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(new AdminDashboardDto
            {
                ActiveUsers = activeUsers,
                SalesToday = salesToday,
                SalesThisWeek = salesThisWeek,
                SalesThisMonth = salesThisMonth,
                RevenueToday = revenueToday,
                RevenueThisWeek = revenueThisWeek,
                RevenueThisMonth = revenueThisMonth,
                LowStockProducts = lowStockCount,
                TopClients = topClients,
                LowStockList = lowStockList,
                SalesLast7Days = salesLast7,
                TopProducts = topProducts,
                RecentActivity = recentActivity
            });
        }

        // ══════════════════════════════════════════
        // GET /api/dashboard/seller
        // Solo Vendedor (y también SPU/Admin pueden ver)
        // ══════════════════════════════════════════
        [HttpGet("seller")]
        [Authorize(Roles = "SuperUsuario,Administrador,Vendedor")]
        public async Task<IActionResult> GetSellerDashboard()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("No se pudo determinar el usuario");

            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var weekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var sevenDaysAgo = todayStart.AddDays(-6);

            var mySales = _db.Purchases
                .Where(p => p.IsActive && p.UserId == userId);

            // ── KPIs del vendedor ──
            var salesToday = await mySales.CountAsync(p => p.PurchaseDate >= todayStart);
            var salesWeek = await mySales.CountAsync(p => p.PurchaseDate >= weekStart);
            var salesMonth = await mySales.CountAsync(p => p.PurchaseDate >= monthStart);

            var revToday = await mySales
                .Where(p => p.PurchaseDate >= todayStart)
                .SumAsync(p => (decimal?)p.Total) ?? 0;
            var revWeek = await mySales
                .Where(p => p.PurchaseDate >= weekStart)
                .SumAsync(p => (decimal?)p.Total) ?? 0;
            var revMonth = await mySales
                .Where(p => p.PurchaseDate >= monthStart)
                .SumAsync(p => (decimal?)p.Total) ?? 0;

            // ── Top 5 clientes del vendedor ──
            var myTopClients = await mySales
                .Include(p => p.Client)
                .Where(p => p.PurchaseDate >= monthStart)
                .GroupBy(p => new { p.ClientId, p.Client.ClientName })
                .Select(g => new TopClientDto
                {
                    ClientId = g.Key.ClientId,
                    ClientName = g.Key.ClientName,
                    TotalPurchases = g.Count(),
                    TotalSpent = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(5)
                .ToListAsync();

            // ── Top 10 productos del vendedor ──
            var myTopProducts = await _db.SaleDetails
                .Where(sd => sd.Purchase.IsActive
                    && sd.Purchase.UserId == userId
                    && sd.Purchase.PurchaseDate >= monthStart)
                .GroupBy(sd => new { sd.ProductId, sd.ProductName })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    UnitsSold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.LineTotal)
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(10)
                .ToListAsync();

            // ── Ventas últimos 7 días del vendedor ──
            var mySalesLast7Raw = await mySales
                .Where(p => p.PurchaseDate >= sevenDaysAgo)
                .Select(p => new { p.PurchaseDate, p.Total })
                .ToListAsync();

            var mySalesLast7 = mySalesLast7Raw
                .GroupBy(p => p.PurchaseDate.Date)
                .Select(g => new SalesByDayDto
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Count = g.Count(),
                    Total = g.Sum(x => x.Total)
                })
                .OrderBy(x => x.Date)
                .ToList();

            return Ok(new SellerDashboardDto
            {
                MySalesToday = salesToday,
                MySalesThisWeek = salesWeek,
                MySalesThisMonth = salesMonth,
                MyRevenueToday = revToday,
                MyRevenueThisWeek = revWeek,
                MyRevenueThisMonth = revMonth,
                MyTopClients = myTopClients,
                MyTopProducts = myTopProducts,
                MySalesLast7Days = mySalesLast7
            });
        }

        // ══════════════════════════════════════════
        // GET /api/dashboard/audit-logs
        // Paginado para la US de logs del sistema
        // ══════════════════════════════════════════
        [HttpGet("audit-logs")]
        [Authorize(Roles = "SuperUsuario,Administrador")]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? action = null,
            [FromQuery] int? userId = null,
            [FromQuery] string? entityType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? ipAddress = null)
        {
            var query = _db.AuditLogs.Include(a => a.User).AsQueryable();

            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action == action);

            if (!string.IsNullOrEmpty(ipAddress))
                query = query.Where(a => a.IpAddress != null && a.IpAddress.Contains(ipAddress));

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityType == entityType);

            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.CreatedAt <= endDate.Value);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new RecentAuditDto
                {
                    LogId = a.LogId,
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    Details = a.Details,
                    UserName = a.User.UserName + " " + (a.User.UserLastname ?? ""),
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                data = logs,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        // ══════════════════════════════════════════
        // GET /api/dashboard/audit-logs/export
        // Exportar logs a CSV (hasta 100k filas)
        // ══════════════════════════════════════════
        [HttpGet("audit-logs/export")]
        [Authorize(Roles = "SuperUsuario,Administrador")]
        public async Task<IActionResult> ExportAuditLogsCsv(
            [FromQuery] string? action = null,
            [FromQuery] int? userId = null,
            [FromQuery] string? entityType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? ipAddress = null)
        {
            var query = _db.AuditLogs.Include(a => a.User).AsQueryable();

            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action == action);

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityType == entityType);

            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.CreatedAt <= endDate.Value);

            if (!string.IsNullOrEmpty(ipAddress))
                query = query.Where(a => a.IpAddress != null && a.IpAddress.Contains(ipAddress));

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Take(100_000)
                .Select(a => new
                {
                    a.LogId,
                    a.Action,
                    a.EntityType,
                    a.EntityId,
                    UserName = a.User.UserName + " " + (a.User.UserLastname ?? ""),
                    a.UserId,
                    a.Details,
                    a.IpAddress,
                    a.CreatedAt
                })
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("log_id,fecha_utc,usuario,user_id,accion,entidad,entidad_id,ip,detalles");

            foreach (var l in logs)
            {
                var details = (l.Details ?? "").Replace("\"", "\"\"");
                sb.AppendLine($"{l.LogId},{l.CreatedAt:yyyy-MM-dd HH:mm:ss},{Escape(l.UserName)},{l.UserId},{l.Action},{l.EntityType},{l.EntityId},{l.IpAddress ?? ""},\"{details}\"");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"audit_logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }

        // ══════════════════════════════════════════
        // GET /api/dashboard/audit-logs/filters
        // Devuelve listas únicas para poblar dropdowns
        // ══════════════════════════════════════════
        [HttpGet("audit-logs/filters")]
        [Authorize(Roles = "SuperUsuario,Administrador")]
        public async Task<IActionResult> GetAuditLogFilters()
        {
            var actions = await _db.AuditLogs
                .Select(a => a.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

            var entityTypes = await _db.AuditLogs
                .Select(a => a.EntityType)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

            var users = await _db.Users
                .Where(u => u.IsActive)
                .Select(u => new { u.UserId, Name = u.UserName + " " + (u.UserLastname ?? "") })
                .OrderBy(u => u.Name)
                .ToListAsync();

            return Ok(new { actions, entityTypes, users });
        }

        private static string Escape(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }
}
