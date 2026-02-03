using SyncroBE.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

public class User
{
    public int UserId { get; set; }
    public string UserRole { get; set; }
    public string UserName { get; set; }
    public string? UserLastname { get; set; }
    public string UserEmail { get; set; }
    public string PasswordHash { get; set; }
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastLogin { get; set; }

    public ICollection<Quote> Quotes { get; set; }

}
