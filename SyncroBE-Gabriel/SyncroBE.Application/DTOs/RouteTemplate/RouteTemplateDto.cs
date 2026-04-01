namespace SyncroBE.Application.DTOs.RouteTemplate
{
    public class RouteTemplateDto
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = null!;
        public string? Description { get; set; }

        public int? DefaultDriverUserId { get; set; }
        public string? DefaultDriverName { get; set; }

        public bool IsActive { get; set; }
        public int StopCount { get; set; }

        public List<RouteTemplateStopDto> Stops { get; set; } = new();
    }
}