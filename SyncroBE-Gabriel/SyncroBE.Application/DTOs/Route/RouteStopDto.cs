namespace SyncroBE.Application.DTOs.Route
{
    public class RouteStopDto
    {
        public int RouteStopId { get; set; }
        public string ClientId { get; set; } = null!;
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
    }
}