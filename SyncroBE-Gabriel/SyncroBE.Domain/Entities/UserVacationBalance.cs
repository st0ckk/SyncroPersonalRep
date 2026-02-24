using System;

namespace SyncroBE.Domain.Entities
{
    public class UserVacationBalance
    {
        public int VBalanceId { get; set; }
        public int UserId { get; set; }

        public decimal AvailableDays { get; set; }
        public DateTime? LastAccrualDate { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User? User { get; set; }
    }
}