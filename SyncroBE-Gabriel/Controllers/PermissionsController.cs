using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;

namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/permissions")]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly SyncroDbContext _context;

        // All navigable screens with their labels and groups (mirrors the frontend MENU)
        private static readonly List<ScreenDef> AllScreens = new()
        {
            new("dashboard",       "Dashboard",             "General"),
            new("clientes",        "Clientes",              "Comercial"),
            new("cotizaciones",    "Cotizaciones",          "Comercial"),
            new("cuentas-credito", "Cuentas de crédito",    "Comercial"),
            new("facturacion",     "Facturación",           "Comercial"),
            new("mapa-clientes",   "Mapa de clientes",      "Comercial"),
            new("mis-reportes",    "Mis Reportes",          "Comercial"),
            new("ventas",          "Ventas",                "Comercial"),
            new("cajas",           "Cajas",                 "Comercial"),
            new("distributors",    "Distribuidores",        "Inventario"),
            new("stock",           "Stock",                 "Inventario"),
            new("rutas-monitoreo", "Monitoreo rutas",       "Logística"),
            new("plantillas-rutas","Plantillas de rutas",   "Logística"),
            new("rutas",           "Rutas",                 "Logística"),
            new("activos",         "Activos",               "Personal"),
            new("horarios",        "Horarios",              "Personal"),
            new("usuarios",        "Usuarios",              "Personal"),
            new("reportes",        "Reportes",              "General"),
            new("logs",            "Logs del sistema",      "General"),
        };

        public PermissionsController(SyncroDbContext context)
        {
            _context = context;
        }

        // Returns all extra screen keys granted to current user (beyond their role defaults)
        [HttpGet("me")]
        public async Task<IActionResult> GetMyPermissions()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value
                         ?? User.FindFirst("nameid")?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                    ?? User.FindFirst("role")?.Value
                    ?? User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value
                    ?? string.Empty;

            var screens = await _context.ScreenPermissions
                .Where(p => (p.UserId == userId) || (p.Role == role))
                .Select(p => p.ScreenKey)
                .Distinct()
                .ToListAsync();

            return Ok(screens);
        }

        // Returns the full screen catalog (for admin UI)
        [HttpGet("screens")]
        [Authorize(Roles = "SuperUsuario,Administrador")]
        public IActionResult GetScreens()
        {
            return Ok(AllScreens.Select(s => new { s.Key, s.Label, s.Group }));
        }

        // Get extra screens granted to a specific user
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "SuperUsuario,Administrador")]
        public async Task<IActionResult> GetUserPermissions(int userId)
        {
            var screens = await _context.ScreenPermissions
                .Where(p => p.UserId == userId)
                .Select(p => p.ScreenKey)
                .ToListAsync();

            return Ok(screens);
        }

        // Replace extra screens for a specific user
        [HttpPut("user/{userId}")]
        [Authorize(Roles = "SuperUsuario,Administrador")]
        public async Task<IActionResult> SetUserPermissions(int userId, [FromBody] List<string> screenKeys)
        {
            var granterIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value
                            ?? User.FindFirst("nameid")?.Value;
            int.TryParse(granterIdStr, out var granterId); // 0 si no se puede leer el claim

            var existing = await _context.ScreenPermissions
                .Where(p => p.UserId == userId)
                .ToListAsync();

            _context.ScreenPermissions.RemoveRange(existing);

            var now = DateTime.UtcNow;
            var newPerms = screenKeys
                .Where(k => AllScreens.Any(s => s.Key == k))
                .Select(k => new ScreenPermission
                {
                    UserId = userId,
                    ScreenKey = k,
                    GrantedBy = granterId,
                    CreatedAt = now
                });

            await _context.ScreenPermissions.AddRangeAsync(newPerms);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // Get extra screens granted to a role
        [HttpGet("role/{role}")]
        [Authorize(Roles = "SuperUsuario,Administrador")]
        public async Task<IActionResult> GetRolePermissions(string role)
        {
            var screens = await _context.ScreenPermissions
                .Where(p => p.Role == role)
                .Select(p => p.ScreenKey)
                .ToListAsync();

            return Ok(screens);
        }

        // Replace extra screens for a role
        [HttpPut("role/{role}")]
        [Authorize(Roles = "SuperUsuario,Administrador")]
        public async Task<IActionResult> SetRolePermissions(string role, [FromBody] List<string> screenKeys)
        {
            var granterIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value
                            ?? User.FindFirst("nameid")?.Value;
            int.TryParse(granterIdStr, out var granterId); // 0 si no se puede leer el claim

            var existing = await _context.ScreenPermissions
                .Where(p => p.Role == role)
                .ToListAsync();

            _context.ScreenPermissions.RemoveRange(existing);

            var now = DateTime.UtcNow;
            var newPerms = screenKeys
                .Where(k => AllScreens.Any(s => s.Key == k))
                .Select(k => new ScreenPermission
                {
                    Role = role,
                    ScreenKey = k,
                    GrantedBy = granterId,
                    CreatedAt = now
                });

            await _context.ScreenPermissions.AddRangeAsync(newPerms);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private record ScreenDef(string Key, string Label, string Group);
    }
}
