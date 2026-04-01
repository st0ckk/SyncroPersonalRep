namespace SyncroBE.Domain.Entities
{
    public class DeliveryRouteStop
    {
        public int RouteStopId { get; set; }

        public int RouteId { get; set; }
        public DeliveryRoute Route { get; set; } = null!;

        public string ClientId { get; set; } = null!;
        public Client Client { get; set; } = null!;

        public string ClientNameSnapshot { get; set; } = null!;
        public string? AddressSnapshot { get; set; }

        public int StopOrder { get; set; }

        public DateTime? PlannedArrival { get; set; }
        public int? EstimatedTravelMinutesFromPrevious { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; }

        public string? DeliveryPhotoPath { get; set; }
        public DateTime? DeliveryPhotoUploadedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}