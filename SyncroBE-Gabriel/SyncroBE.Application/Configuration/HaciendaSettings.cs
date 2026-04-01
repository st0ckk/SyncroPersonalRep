namespace SyncroBE.Application.Configuration
{
    /// <summary>
    /// Strongly-typed settings bound from appsettings.json → "Hacienda" section.
    /// </summary>
    public class HaciendaSettings
    {
        public const string SectionName = "Hacienda";

        public string Environment { get; set; } = "sandbox";

        // ── API endpoints ──
        public string ApiRecepcionUrl { get; set; } = null!;
        public string ApiRecepcionUrlSandbox { get; set; } = null!;

        // ── Token endpoints ──
        public string TokenUrl { get; set; } = null!;
        public string TokenUrlSandbox { get; set; } = null!;

        // ── OAuth client ──
        public string ClientId { get; set; } = "api-prod";
        public string ClientIdSandbox { get; set; } = "api-stag";
        public string ClientSecret { get; set; } = "";

        // ── ATV credentials ──
        public string AtvUsername { get; set; } = null!;
        public string AtvPassword { get; set; } = null!;

        // ── Certificate ──
        public string CertificatePath { get; set; } = null!;
        public string CertificatePin { get; set; } = null!;

        // ── Helpers ──
        public bool IsProduction => Environment?.ToLowerInvariant() == "production";
        public string EffectiveApiUrl => IsProduction ? ApiRecepcionUrl : ApiRecepcionUrlSandbox;
        public string EffectiveTokenUrl => IsProduction ? TokenUrl : TokenUrlSandbox;
        public string EffectiveClientId => IsProduction ? ClientId : ClientIdSandbox;
    }
}
