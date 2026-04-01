using SyncroBE.Application.DTOs.Sale;
using SyncroBE.Domain.Entities;

namespace SyncroBE.Application.DTOs.Route
{
    public class RouteDto
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = null!;
        public DateTime RouteDate { get; set; }

        public int DriverUserId { get; set; }
        public string DriverName { get; set; } = null!;

        public string Status { get; set; } = "Draft";

        public DateTime? StartAtPlanned { get; set; }
        public DateTime? EndAtEstimated { get; set; }

        public int? EstimatedDurationMinutes { get; set; }
        public decimal? EstimatedDistanceKm { get; set; }

        public string? Polyline { get; set; }
        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public int StopCount { get; set; }
        public List<RouteStopDto> Stops { get; set; } = new();

        public List<SaleDto> Purchases { get; set; } = new();
    }
}