namespace SyncroBE.Application.DTOs.User
{
    public class UserDto
    {
        public int UserId { get; set; }
        public required string UserRole { get; set; }
        public required string UserName { get; set; }
        public string? UserLastname { get; set; }
        public required string UserEmail { get; set; }
        public bool IsActive { get; set; }
        public bool MustChangePassword { get; set; }
        public string? Telefono { get; set; }
        public string? TelefonoPersonal { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public bool IsLocked { get; set; }
    }
}
