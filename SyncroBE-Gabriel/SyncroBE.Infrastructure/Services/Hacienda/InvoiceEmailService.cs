using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SyncroBE.Application.Configuration;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;

namespace SyncroBE.Infrastructure.Services.Hacienda
{
    /// <summary>
    /// Sends electronic invoice emails via SMTP with the signed XML attached.
    /// </summary>
    public class InvoiceEmailService : IInvoiceEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<InvoiceEmailService> _logger;

        public InvoiceEmailService(
            IOptions<EmailSettings> settings,
            ILogger<InvoiceEmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendInvoiceEmailAsync(Invoice invoice, string recipientEmail)
        {
            _logger.LogInformation(
                "Sending invoice {Clave} to {Email}", invoice.Clave, recipientEmail);

            var docTypeName = invoice.DocumentType switch
            {
                "01" => "Factura Electrónica",
                "03" => "Nota de Crédito Electrónica",
                "04" => "Tiquete Electrónico",
                _ => "Comprobante Electrónico"
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = $"{docTypeName} - {invoice.Clave}",
                IsBodyHtml = true,
                Body = BuildEmailBody(invoice, docTypeName)
            };

            message.To.Add(new MailAddress(recipientEmail));

            // Attach signed XML
            if (!string.IsNullOrEmpty(invoice.XmlSigned))
            {
                var xmlBytes = Encoding.UTF8.GetBytes(invoice.XmlSigned);
                var xmlStream = new MemoryStream(xmlBytes);
                var fileName = $"{invoice.Clave}.xml";
                message.Attachments.Add(new Attachment(xmlStream, fileName, "application/xml"));
            }

            // Attach Hacienda response XML if available
            if (!string.IsNullOrEmpty(invoice.XmlResponse))
            {
                var respBytes = Encoding.UTF8.GetBytes(invoice.XmlResponse);
                var respStream = new MemoryStream(respBytes);
                var respFileName = $"Respuesta-{invoice.Clave}.xml";
                message.Attachments.Add(new Attachment(respStream, respFileName, "application/xml"));
            }

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.UseSsl
            };

            await client.SendMailAsync(message);

            _logger.LogInformation(
                "Invoice email sent successfully to {Email}", recipientEmail);
        }

        private static string BuildEmailBody(Invoice invoice, string docTypeName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .header {{ background-color: #2c3e50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; }}
        .detail {{ background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 10px 0; }}
        .detail table {{ width: 100%; border-collapse: collapse; }}
        .detail td {{ padding: 5px 10px; }}
        .detail td:first-child {{ font-weight: bold; color: #555; }}
        .footer {{ text-align: center; color: #888; font-size: 12px; padding: 20px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h2>{docTypeName}</h2>
    </div>
    <div class='content'>
        <p>Estimado cliente,</p>
        <p>Adjunto encontrará su comprobante electrónico con los siguientes detalles:</p>
        <div class='detail'>
            <table>
                <tr><td>Clave:</td><td>{invoice.Clave}</td></tr>
                <tr><td>Consecutivo:</td><td>{invoice.ConsecutiveNumber}</td></tr>
                <tr><td>Fecha de emisión:</td><td>{invoice.EmissionDate:dd/MM/yyyy HH:mm}</td></tr>
                <tr><td>Total:</td><td>₡{invoice.InvoiceTotal:N2}</td></tr>
                <tr><td>Estado Hacienda:</td><td>{invoice.HaciendaStatus}</td></tr>
            </table>
        </div>
        <p>Este comprobante ha sido enviado al Ministerio de Hacienda de Costa Rica.</p>
    </div>
    <div class='footer'>
        <p>Distribuidora Sion &bull; Comprobante generado electrónicamente</p>
    </div>
</body>
</html>";
        }
    }
}
