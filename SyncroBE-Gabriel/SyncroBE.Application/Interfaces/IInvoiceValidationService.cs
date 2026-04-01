using SyncroBE.Domain.Entities;

namespace SyncroBE.Application.Interfaces
{
    /// <summary>
    /// Pre-validates invoice data before sending to Hacienda.
    /// Catches common issues (missing CABYS, missing client data, etc.) early.
    /// </summary>
    public interface IInvoiceValidationService
    {
        /// <summary>
        /// Validates all data needed to generate an electronic invoice for a purchase.
        /// Returns a list of validation errors. Empty list = valid.
        /// </summary>
        Task<List<InvoiceValidationError>> ValidateForInvoiceAsync(int purchaseId, string documentType = "01");

        /// <summary>
        /// Validates data needed to generate a credit note.
        /// </summary>
        Task<List<InvoiceValidationError>> ValidateForCreditNoteAsync(int originalInvoiceId);
    }

    public class InvoiceValidationError
    {
        public string Field { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Severity { get; set; } = "error"; // "error" or "warning"

        public InvoiceValidationError() { }

        public InvoiceValidationError(string field, string message, string severity = "error")
        {
            Field = field;
            Message = message;
            Severity = severity;
        }
    }
}
