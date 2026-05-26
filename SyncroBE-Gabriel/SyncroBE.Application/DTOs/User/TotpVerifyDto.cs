namespace SyncroBE.Application.DTOs.User
{
    public class TotpVerifyDto
    {
        public string TempToken { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
}
