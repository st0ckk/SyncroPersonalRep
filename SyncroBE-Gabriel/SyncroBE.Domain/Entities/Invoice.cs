namespace SyncroBE.Domain.Entities
{
    /// <summary>
    /// Electronic invoice entity aligned with Costa Rica Hacienda requirements.
    /// Maps to the existing [invoice] table + new columns from 001_ElectronicInvoice_Schema.sql.
    /// </summary>
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public int PurchaseId { get; set; }

        // ── Legacy columns ──
        public string? ElectronicInvoice { get; set; }
        public decimal InvoiceTotal { get; set; }
        public DateTime? InvoiceDate { get; set; }

        // ── Hacienda electronic invoice fields ──
        public string? Clave { get; set; }                      // 50-digit unique key
        public string? ConsecutiveNumber { get; set; }           // 20-digit consecutive
        public string? DocumentType { get; set; }                // 01=FE, 02=ND, 03=NC, 04=TE
        public string? HaciendaStatus { get; set; }              // pending, sent, accepted, rejected, error
        public string? XmlSigned { get; set; }                   // Signed XML sent to Hacienda
        public string? XmlResponse { get; set; }                 // Hacienda response XML
        public string? HaciendaMessage { get; set; }             // Response summary
        public DateTime? EmissionDate { get; set; }              // FechaEmision in XML
        public DateTime? SentAt { get; set; }                    // When sent to Hacienda API
        public DateTime? ResponseAt { get; set; }                // When Hacienda responded

        // ── Currency ──
        public string? CurrencyCode { get; set; }                // CRC, USD, etc.
        public decimal? ExchangeRate { get; set; }               // Tipo de cambio

        // ── Sale/Payment conditions ──
        public string? SaleCondition { get; set; }               // 01=Contado, 02=Crédito, etc.
        public string? PaymentMethodCode { get; set; }           // 01=Efectivo, 02=Tarjeta, etc.

        // ── Reference (for NC/ND) ──
        public string? ReferenceDocumentClave { get; set; }      // Clave of original document
        public string? ReferenceCode { get; set; }               // 01=Anula, 02=Corrige, 05=Sustituye
        public string? ReferenceReason { get; set; }             // Reason for NC/ND

        // ── Activity code ──
        public string? ActivityCode { get; set; }                // 6-digit code

        // ── Timestamps ──
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ── Navigation ──
        public Purchase Purchase { get; set; } = null!;
    }
}
