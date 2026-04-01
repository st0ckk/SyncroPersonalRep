namespace SyncroBE.Domain.Entities
{
    /// <summary>
    /// Emisor (issuer) configuration for electronic invoicing.
    /// Stores all business info required by Hacienda XML.
    /// Credentials (ATV user/password, certificate PIN) are in appsettings.json.
    /// </summary>
    public class CompanyConfig
    {
        public int ConfigId { get; set; }
        public string CompanyName { get; set; } = null!;         // Nombre/Razón social
        public string? CommercialName { get; set; }               // Nombre comercial
        public string IdType { get; set; } = null!;               // 01=Física, 02=Jurídica, 03=DIMEX, 04=NITE
        public string IdNumber { get; set; } = null!;             // Cédula
        public string ActivityCode { get; set; } = null!;         // Código actividad económica (6 digits)

        // ── Location ──
        public int ProvinceCode { get; set; }
        public int CantonCode { get; set; }
        public int DistrictCode { get; set; }
        public int? NeighborhoodCode { get; set; }               // Barrio (optional)
        public string OtherAddress { get; set; } = null!;        // OtrasSenas

        // ── Contact ──
        public string PhoneCountryCode { get; set; } = "506";
        public string? PhoneNumber { get; set; }
        public string? FaxCountryCode { get; set; }
        public string? FaxNumber { get; set; }
        public string Email { get; set; } = null!;

        // ── Branch/Terminal ──
        public string BranchNumber { get; set; } = "001";        // Sucursal
        public string TerminalNumber { get; set; } = "00001";    // Terminal

        // ── Environment ──
        public string Environment { get; set; } = "sandbox";     // sandbox | production

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ── Navigation ──
        public Province? Province { get; set; }
        public Canton? Canton { get; set; }
        public District? District { get; set; }
    }
}
