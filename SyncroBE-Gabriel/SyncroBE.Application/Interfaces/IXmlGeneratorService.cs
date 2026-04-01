using SyncroBE.Domain.Entities;

namespace SyncroBE.Application.Interfaces
{
    /// <summary>
    /// Generates Hacienda-compliant XML documents (FacturaElectronica v4.4).
    /// </summary>
    public interface IXmlGeneratorService
    {
        /// <summary>
        /// Generates unsigned FacturaElectronica XML for a purchase.
        /// </summary>
        string GenerateInvoiceXml(
            CompanyConfig emisor,
            Client receptor,
            Purchase purchase,
            Invoice invoice);

        /// <summary>
        /// Generates unsigned NotaCreditoElectronica XML.
        /// </summary>
        string GenerateCreditNoteXml(
            CompanyConfig emisor,
            Client receptor,
            Purchase purchase,
            Invoice creditNote,
            Invoice originalInvoice);
    }
}
