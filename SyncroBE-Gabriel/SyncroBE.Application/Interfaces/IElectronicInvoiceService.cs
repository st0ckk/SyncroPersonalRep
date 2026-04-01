using SyncroBE.Domain.Entities;

namespace SyncroBE.Application.Interfaces
{
    /// <summary>
    /// Orchestrates the full electronic invoice lifecycle:
    /// generate clave → build XML → sign → send to Hacienda → poll status.
    /// </summary>
    public interface IElectronicInvoiceService
    {
        /// <summary>
        /// Generates and sends an electronic invoice (FE) for a given purchase.
        /// </summary>
        Task<Invoice> GenerateAndSendAsync(int purchaseId, string documentType = "01");

        /// <summary>
        /// Generates a Credit Note (NC) referencing an existing invoice.
        /// </summary>
        Task<Invoice> GenerateCreditNoteAsync(int originalInvoiceId, string reason);

        /// <summary>
        /// Queries Hacienda for the status of a previously sent invoice.
        /// </summary>
        Task<Invoice> CheckStatusAsync(int invoiceId);

        /// <summary>
        /// Re-sends a failed/pending invoice to Hacienda.
        /// </summary>
        Task<Invoice> ResendAsync(int invoiceId);
    }
}
