using Microsoft.AspNetCore.Identity;

namespace SyncroBE.Infrastructure.Auth
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
