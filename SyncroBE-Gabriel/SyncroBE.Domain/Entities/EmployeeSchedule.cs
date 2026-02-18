namespace SyncroBE.Domain.Entities
{
    public class EmployeeSchedule
    {
        public int ScheduleId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;

        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }



    }
}
