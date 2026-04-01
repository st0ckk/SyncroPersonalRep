namespace SyncroBE.Domain.Entities
{
    public class RouteTemplate
    {
        public int TemplateId { get; set; }

        public string TemplateName { get; set; } = null!;
        public string? Description { get; set; }

        public int? DefaultDriverUserId { get; set; }
        public User? DefaultDriverUser { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<RouteTemplateStop> Stops { get; set; } = new List<RouteTemplateStop>();
    }
}