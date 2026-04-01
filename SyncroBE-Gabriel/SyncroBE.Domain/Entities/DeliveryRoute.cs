namespace SyncroBE.Domain.Entities
{
    public class DeliveryRoute
    {
        public int RouteId { get; set; }

        public string RouteName { get; set; } = null!;
        public DateTime RouteDate { get; set; }

        public int DriverUserId { get; set; }
        public User DriverUser { get; set; } = null!;

        public string Status { get; set; } = "Draft";

        public DateTime? StartAtPlanned { get; set; }
        public DateTime? EndAtEstimated { get; set; }

        public int? EstimatedDurationMinutes { get; set; }
        public decimal? EstimatedDistanceKm { get; set; }

        public string? Polyline { get; set; }
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<DeliveryRouteStop> Stops { get; set; } = new List<DeliveryRouteStop>();
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}