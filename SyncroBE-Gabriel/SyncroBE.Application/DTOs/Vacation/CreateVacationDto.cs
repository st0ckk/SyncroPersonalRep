using System;

namespace SyncroBE.Application.DTOs.Vacations
{
    public class CreateVacationDto
    {
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
    }
}