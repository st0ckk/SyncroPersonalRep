using System.ComponentModel.DataAnnotations;

namespace SyncroBE.Application.DTOs.RouteTemplate
{
    public class RouteTemplateCreateDto
    {
        [Required]
        public string TemplateName { get; set; } = null!;

        public string? Description { get; set; }
        public int? DefaultDriverUserId { get; set; }

        [Required]
        [MinLength(1)]
        public List<RouteTemplateStopCreateUpdateDto> Stops { get; set; } = new();
    }
}