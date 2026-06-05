using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using SyncroBE.Infrastructure.Data;
using System.Security.Claims;

namespace SyncroBE.API.Authorization
{
    /// <summary>
    /// Allows access if the user's JWT role is in <paramref name="roles"/>
    /// OR if the user (or their role) has been granted <paramref name="screenKey"/>
    /// in the ScreenPermissions table.
    /// Requires [Authorize] at the controller level to ensure JWT is valid first.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ScreenOrRoleAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _screenKey;
        private readonly HashSet<string> _roles;

        public ScreenOrRoleAttribute(string screenKey, params string[] roles)
        {
            _screenKey = screenKey;
            _roles = new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase);
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "No autenticado." });
                return;
            }

            var role = user.FindFirst(ClaimTypes.Role)?.Value
                    ?? user.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value
                    ?? string.Empty;

            // Role is directly allowed
            if (_roles.Contains(role))
                return;

            // Check screen permission in DB for this user or their role
            var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
            {
                context.Result = new ObjectResult(new { message = "No tiene permisos para acceder a este recurso." }) { StatusCode = 403 };
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<SyncroDbContext>();
            var hasPermission = await db.ScreenPermissions
                .AnyAsync(p => p.ScreenKey == _screenKey &&
                               (p.UserId == userId || p.Role == role));

            if (!hasPermission)
                context.Result = new ObjectResult(new { message = "No tiene permisos para acceder a este recurso." }) { StatusCode = 403 };
        }
    }
}
