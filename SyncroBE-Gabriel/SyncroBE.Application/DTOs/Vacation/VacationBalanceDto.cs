namespace SyncroBE.Application.DTOs.Vacations
{
    public class VacationBalanceDto
    {
        public int UserId { get; set; }
        public decimal AvailableDays { get; set; }
        public string? LastAccrualDate { get; set; }
    }
}