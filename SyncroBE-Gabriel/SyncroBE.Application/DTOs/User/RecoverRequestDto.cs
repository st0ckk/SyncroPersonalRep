namespace SyncroBE.Application.DTOs.User
{
    public class RecoverRequestDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class RecoverVerifyTotpDto
    {
        public string TempToken { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class RecoverSetPasswordDto
    {
        public string ResetToken { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string TotpCode { get; set; } = string.Empty;
    }
}
