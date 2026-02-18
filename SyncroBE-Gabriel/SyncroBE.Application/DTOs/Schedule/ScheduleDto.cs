namespace SyncroBE.Application.DTOs.Schedule
{
    public class ScheduleDto
    {
        public int ScheduleId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
    }
}
