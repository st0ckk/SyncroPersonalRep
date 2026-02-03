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
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
