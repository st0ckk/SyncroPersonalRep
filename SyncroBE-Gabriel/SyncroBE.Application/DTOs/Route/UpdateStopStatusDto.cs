using System.ComponentModel.DataAnnotations;

namespace SyncroBE.Application.DTOs.Route
{
    public class UpdateStopStatusDto
    {
        [Required]
        public string Status { get; set; } = null!; // "EnRoute" | "Delivered" | "Cancelled"

        public string? Note { get; set; } // requerido si Status == "Cancelled"
    }
}