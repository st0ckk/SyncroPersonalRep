namespace SyncroBE.Application.DTOs.ElectronicInvoice
{
    public class ElectronicInvoiceDto
    {
        public int InvoiceId { get; set; }
        public int PurchaseId { get; set; }
        public string? Clave { get; set; }
        public string? ConsecutiveNumber { get; set; }
        public string? DocumentType { get; set; }
        public string? HaciendaStatus { get; set; }
        public string? HaciendaMessage { get; set; }
        public DateTime? EmissionDate { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? ResponseAt { get; set; }
        public decimal InvoiceTotal { get; set; }
        public string? CurrencyCode { get; set; }
        public string? SaleCondition { get; set; }
        public string? PaymentMethodCode { get; set; }
        public string? ActivityCode { get; set; }

        // Reference info (for NC/ND)
        public string? ReferenceDocumentClave { get; set; }
        public string? ReferenceCode { get; set; }
        public string? ReferenceReason { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class GenerateInvoiceRequestDto
    {
        public int PurchaseId { get; set; }
        public string DocumentType { get; set; } = "01";   // 01=FE, 04=TE
    }

    public class GenerateCreditNoteRequestDto
    {
        public int OriginalInvoiceId { get; set; }
        public string Reason { get; set; } = null!;
    }

    public class InvoiceStatusDto
    {
        public int InvoiceId { get; set; }
        public string? Clave { get; set; }
        public string? HaciendaStatus { get; set; }
        public string? HaciendaMessage { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? ResponseAt { get; set; }
    }
}
