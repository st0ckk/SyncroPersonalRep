using Microsoft.AspNetCore.Identity;
using SyncroBE.Infrastructure.Auth;

namespace SyncroBE.API.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "SuperUsuario", "Administrador", "Vendedor", "Chofer" };

            foreach (var r in roles)
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));

            var email = "super@syncro.local";
            var user = await userMgr.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = "Super Usuario"
                };

                await userMgr.CreateAsync(user, "Admin123*");
                await userMgr.AddToRoleAsync(user, "SuperUsuario");
            }
        }
    }
}
