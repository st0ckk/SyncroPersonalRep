using System.ComponentModel.DataAnnotations;

namespace SyncroBE.Application.DTOs.Route
{
    public class RouteStopCreateUpdateDto
    {
        [Required]
        public string ClientId { get; set; } = null!;

        [Required]
        public int StopOrder { get; set; }

        public DateTime? PlannedArrival { get; set; }
        public string? Notes { get; set; }
    }
}