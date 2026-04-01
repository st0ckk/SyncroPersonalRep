namespace SyncroBE.Application.DTOs.RouteTemplate
{
    public class RouteTemplateStopDto
    {
        public int TemplateStopId { get; set; }
        public string ClientId { get; set; } = null!;
        public string ClientNameSnapshot { get; set; } = null!;
        public string? AddressSnapshot { get; set; }
        public int StopOrder { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Notes { get; set; }
    }
}