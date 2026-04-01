namespace SyncroBE.Application.Configuration
{
    /// <summary>
    /// SMTP email settings bound from appsettings.json → "Email" section.
    /// </summary>
    public class EmailSettings
    {
        public const string SectionName = "Email";

        public string SmtpHost { get; set; } = null!;
        public int SmtpPort { get; set; } = 587;
        public bool UseSsl { get; set; } = true;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FromEmail { get; set; } = null!;
        public string FromName { get; set; } = "Distribuidora Sion";
    }
}
