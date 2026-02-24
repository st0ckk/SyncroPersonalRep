using System;
using System.ComponentModel.DataAnnotations;

namespace SyncroBE.Domain.Entities
{
    public class VacationMovement
    {
        public int MovementsId { get; set; }
        public int UserId { get; set; }

        [MaxLength(20)]
        public string MovementType { get; set; } = default!; // ASSIGN, ACCRUAL, USE, ADJUST, REFUND

        public decimal Days { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }

        public User? User { get; set; }
    }
}