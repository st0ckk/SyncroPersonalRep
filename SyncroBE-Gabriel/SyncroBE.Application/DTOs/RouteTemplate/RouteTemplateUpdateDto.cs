using System.ComponentModel.DataAnnotations;

namespace SyncroBE.Application.DTOs.RouteTemplate
{
    public class RouteTemplateUpdateDto
    {
        [Required]
        public int TemplateId { get; set; }

        [Required]
        public string TemplateName { get; set; } = null!;

        public string? Description { get; set; }
        public int? DefaultDriverUserId { get; set; }
        public bool IsActive { get; set; } = true;

        [Required]
        [MinLength(1)]
        public List<RouteTemplateStopCreateUpdateDto> Stops { get; set; } = new();
    }
}