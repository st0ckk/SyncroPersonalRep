using System;
using System.ComponentModel.DataAnnotations;

namespace SyncroBE.Domain.Entities
{
    public class Vacation
    {
        public int VacationId { get; set; }
        public int UserId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal DaysRequested { get; set; }
        [MaxLength(255)]
        public string? Reason { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "APPROVED"; // APPROVED / CANCELLED

        public DateTime CreatedAt { get; set; }

        // Navegación
        public User? User { get; set; }
    }
}