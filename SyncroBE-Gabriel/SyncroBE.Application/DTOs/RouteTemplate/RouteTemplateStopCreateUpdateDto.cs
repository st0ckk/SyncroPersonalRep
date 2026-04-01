using System.ComponentModel.DataAnnotations;

namespace SyncroBE.Application.DTOs.RouteTemplate
{
    public class RouteTemplateStopCreateUpdateDto
    {
        [Required]
        public string ClientId { get; set; } = null!;

        [Required]
        public int StopOrder { get; set; }

        public string? Notes { get; set; }
    }
}