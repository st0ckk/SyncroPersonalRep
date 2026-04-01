using System.ComponentModel.DataAnnotations;

namespace SyncroBE.Application.DTOs.Route
{
    public class RouteUpdateDto
    {
        [Required]
        public int RouteId { get; set; }

        [Required]
        public string RouteName { get; set; } = null!;

        [Required]
        public DateTime RouteDate { get; set; }

        [Required]
        public int DriverUserId { get; set; }

        public string Status { get; set; } = "Draft";
        public DateTime? StartAtPlanned { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;

        [Required]
        [MinLength(1)]
        public List<RouteStopCreateUpdateDto> Stops { get; set; } = new();
    }
}