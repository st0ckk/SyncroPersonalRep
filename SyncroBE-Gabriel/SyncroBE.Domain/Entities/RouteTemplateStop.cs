namespace SyncroBE.Domain.Entities
{
    public class RouteTemplateStop
    {
        public int TemplateStopId { get; set; }

        public int TemplateId { get; set; }
        public RouteTemplate Template { get; set; } = null!;

        public string ClientId { get; set; } = null!;
        public Client Client { get; set; } = null!;

        public string ClientNameSnapshot { get; set; } = null!;
        public string? AddressSnapshot { get; set; }

        public int StopOrder { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}