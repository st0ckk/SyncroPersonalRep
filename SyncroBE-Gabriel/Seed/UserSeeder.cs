using SyncroBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SyncroBE.API.Seed
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SyncroDbContext>();

            const string email = "super@syncro.local";
            const string password = "Admin123*";

            var user = await context.Users.FirstOrDefaultAsync(u => u.UserEmail == email);

            var hash = BCrypt.Net.BCrypt.HashPassword(password);

            if (user == null)
            {
                context.Users.Add(new User
                {
                    UserName = "Super",
                    UserLastname = "Usuario",
                    UserEmail = email,
                    UserRole = "SuperUsuario",
                    PasswordHash = hash,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                // 🔥 fuerza actualización del hash inválido
                user.PasswordHash = hash;
                user.UserRole = "SuperUsuario";
                user.IsActive = true;
            }

            await context.SaveChangesAsync();
        }
    }
}
