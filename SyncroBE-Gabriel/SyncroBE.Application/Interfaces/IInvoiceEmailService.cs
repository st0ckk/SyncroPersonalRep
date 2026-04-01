using SyncroBE.Domain.Entities;

namespace SyncroBE.Application.Interfaces
{
    /// <summary>
    /// Sends electronic invoice emails to clients with the signed XML attached.
    /// </summary>
    public interface IInvoiceEmailService
    {
        /// <summary>
        /// Sends an invoice email to the specified address with the signed XML attached.
        /// </summary>
        Task SendInvoiceEmailAsync(Invoice invoice, string recipientEmail);
    }
}
