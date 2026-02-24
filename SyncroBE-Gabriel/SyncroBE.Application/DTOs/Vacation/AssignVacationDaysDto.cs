namespace SyncroBE.Application.DTOs.Vacations
{
    public class AssignVacationDaysDto
    {
        public decimal Days { get; set; }              // cantidad a sumar o fijar
        public bool IsSetOperation { get; set; }       // true = set, false = sumar
        public string? Reason { get; set; }
        public int? CreatedBy { get; set; }            // id del admin que ejecuta
    }
}