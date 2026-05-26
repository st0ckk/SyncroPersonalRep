namespace SyncroBE.Application.DTOs.User
{
    public class TotpSetupResponseDto
    {
        public string Secret { get; set; } = null!;
        public string OtpauthUri { get; set; } = null!;
    }
}
