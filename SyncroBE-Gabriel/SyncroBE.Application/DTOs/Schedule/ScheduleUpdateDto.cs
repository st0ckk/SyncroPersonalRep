using System.ComponentModel.DataAnnotations;

namespace SyncroBE.Application.DTOs.Schedule
{
    public class ScheduleUpdateDto
    {
        [Required] public int ScheduleId { get; set; }
        [Required] public int UserId { get; set; }
        [Required] public DateTime StartAt { get; set; }
        [Required] public DateTime EndAt { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
    }
}
