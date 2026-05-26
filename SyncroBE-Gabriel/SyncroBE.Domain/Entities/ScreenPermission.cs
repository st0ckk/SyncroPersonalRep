namespace SyncroBE.Domain.Entities
{
    public class ScreenPermission
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? Role { get; set; }
        public string ScreenKey { get; set; } = string.Empty;
        public int GrantedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }
        public User? GrantedByUser { get; set; }
    }
}
