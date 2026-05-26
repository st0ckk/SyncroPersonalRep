namespace SyncroBE.Domain.Entities
{
    public class RouteIncident
    {
        public int IncidentId { get; set; }
        public int? RouteId { get; set; }
        public int DriverUserId { get; set; }
        public string IncidentType { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime OccurredAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
