using System.ComponentModel.DataAnnotations;

namespace SyncroBE.Application.DTOs.RouteTemplate
{
    public class InstantiateRouteFromTemplateDto
    {
        [Required]
        public DateTime RouteDate { get; set; }

        public int? DriverUserId { get; set; }

        public DateTime? StartAtPlanned { get; set; }
        public string? RouteName { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "Scheduled";
    }
}