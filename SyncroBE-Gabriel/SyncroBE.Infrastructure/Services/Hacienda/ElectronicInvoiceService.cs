using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;

namespace SyncroBE.Infrastructure.Services.Hacienda
{
    /// <summary>
    /// Orchestrates the full electronic invoice lifecycle:
    /// 1. Load purchase data
    /// 2. Get next consecutive number
    /// 3. Generate clave numérica
    /// 4. Build XML
    /// 5. Sign XML
    /// 6. Send to Hacienda
    /// 7. Save invoice record
    /// </summary>
    public class ElectronicInvoiceService : IElectronicInvoiceService
    {
        private readonly SyncroDbContext _db;
        private readonly IConsecutiveService _consecutiveService;
        private readonly IClaveGeneratorService _claveGenerator;
        private readonly IXmlGeneratorService _xmlGenerator;
        private readonly IXmlSignerService _xmlSigner;
        private readonly IHaciendaApiService _haciendaApi;
        private readonly ILogger<ElectronicInvoiceService> _logger;

        public ElectronicInvoiceService(
            SyncroDbContext db,
            IConsecutiveService consecutiveService,
            IClaveGeneratorService claveGenerator,
            IXmlGeneratorService xmlGenerator,
            IXmlSignerService xmlSigner,
            IHaciendaApiService haciendaApi,
            ILogger<ElectronicInvoiceService> logger)
        {
            _db = db;
            _consecutiveService = consecutiveService;
            _claveGenerator = claveGenerator;
            _xmlGenerator = xmlGenerator;
            _xmlSigner = xmlSigner;
            _haciendaApi = haciendaApi;
            _logger = logger;
        }

        public async Task<Invoice> GenerateAndSendAsync(int purchaseId, string documentType = "01")
        {
            _logger.LogInformation("Generating electronic invoice for purchase {PurchaseId}", purchaseId);

            // 1. Load purchase with all related data
            var purchase = await LoadPurchaseWithDetails(purchaseId);

            // Check if invoice already exists
            var existingInvoice = await _db.Invoices
                .FirstOrDefaultAsync(i => i.PurchaseId == purchaseId && i.DocumentType == documentType);

            if (existingInvoice != null && existingInvoice.HaciendaStatus == "accepted")
                throw new InvalidOperationException(
                    $"Purchase {purchaseId} already has an accepted {documentType} invoice (clave: {existingInvoice.Clave})");

            // 2. Load company config (emisor)
            var emisor = await _db.CompanyConfigs
                .FirstOrDefaultAsync(c => c.IsActive)
                ?? throw new InvalidOperationException("No active company configuration found");

            // 3. Get next consecutive
            var consecutive = await _consecutiveService.GetNextConsecutiveAsync(
                documentType, emisor.BranchNumber, emisor.TerminalNumber);

            // 4. Generate clave
            var clave = _claveGenerator.Generate(
                documentType, emisor.IdType, emisor.IdNumber, consecutive);

            // 5. Create or update invoice record
            var invoice = existingInvoice ?? new Invoice();
            invoice.PurchaseId = purchaseId;
            invoice.InvoiceTotal = purchase.Total;
            invoice.InvoiceDate = DateTime.UtcNow;
            invoice.Clave = clave;
            invoice.ConsecutiveNumber = consecutive;
            invoice.DocumentType = documentType;
            invoice.HaciendaStatus = "pending";
            invoice.EmissionDate = DateTime.Now;  // Local time for CR
            invoice.CurrencyCode = "CRC";
            invoice.ExchangeRate = 1;
            invoice.ActivityCode = emisor.ActivityCode;
            invoice.CreatedAt = DateTime.UtcNow;

            // Map sale condition from payment method
            invoice.SaleCondition = purchase.ClientAccountId.HasValue ? "02" : "01"; // Crédito vs Contado
            invoice.PaymentMethodCode = MapPaymentMethodCode(purchase.PurchasePaymentMethod);

            // 6. Generate XML
            var xml = _xmlGenerator.GenerateInvoiceXml(emisor, purchase.Client, purchase, invoice);

            // 7. Sign XML
            var signedXml = _xmlSigner.SignXml(xml);
            invoice.XmlSigned = signedXml;

            // 8. Save to DB before sending (to preserve the record even if send fails)
            if (existingInvoice == null)
                _db.Invoices.Add(invoice);

            await _db.SaveChangesAsync();

            // 9. Send to Hacienda
            try
            {
                var signedXmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(signedXml));
                var fecha = invoice.EmissionDate?.ToString("yyyy-MM-ddTHH:mm:ss-06:00")
                    ?? DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss-06:00");

                var (statusCode, responseBody) = await _haciendaApi.SendDocumentAsync(
                    clave,
                    fecha,
                    emisor.IdType,
                    emisor.IdNumber,
                    purchase.Client?.HaciendaIdType,
                    purchase.Client?.ClientId,
                    signedXmlBase64);

                invoice.SentAt = DateTime.UtcNow;

                // Hacienda returns 202 Accepted for successful submission
                if (statusCode == 202)
                {
                    invoice.HaciendaStatus = "sent";
                    invoice.HaciendaMessage = "Documento recibido por Hacienda, pendiente de procesamiento";
                }
                else
                {
                    invoice.HaciendaStatus = "error";
                    invoice.HaciendaMessage = $"HTTP {statusCode}: {TruncateString(responseBody, 500)}";
                }

                invoice.XmlResponse = responseBody;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send invoice {Clave} to Hacienda", clave);
                invoice.HaciendaStatus = "error";
                invoice.HaciendaMessage = $"Error de envío: {TruncateString(ex.Message, 500)}";
            }

            invoice.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Invoice {Clave} status: {Status}", clave, invoice.HaciendaStatus);

            return invoice;
        }

        public async Task<Invoice> GenerateCreditNoteAsync(int originalInvoiceId, string reason)
        {
            _logger.LogInformation("Generating credit note for invoice {InvoiceId}", originalInvoiceId);

            var originalInvoice = await _db.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == originalInvoiceId)
                ?? throw new InvalidOperationException($"Invoice {originalInvoiceId} not found");

            if (originalInvoice.HaciendaStatus != "accepted")
                throw new InvalidOperationException(
                    "Can only create credit notes for accepted invoices");

            var purchase = await LoadPurchaseWithDetails(originalInvoice.PurchaseId);
            var emisor = await _db.CompanyConfigs
                .FirstOrDefaultAsync(c => c.IsActive)
                ?? throw new InvalidOperationException("No active company configuration found");

            // NC = document type 03
            var consecutive = await _consecutiveService.GetNextConsecutiveAsync(
                "03", emisor.BranchNumber, emisor.TerminalNumber);

            var clave = _claveGenerator.Generate(
                "03", emisor.IdType, emisor.IdNumber, consecutive);

            var creditNote = new Invoice
            {
                PurchaseId = originalInvoice.PurchaseId,
                InvoiceTotal = purchase.Total,
                InvoiceDate = DateTime.UtcNow,
                Clave = clave,
                ConsecutiveNumber = consecutive,
                DocumentType = "03",
                HaciendaStatus = "pending",
                EmissionDate = DateTime.Now,
                CurrencyCode = "CRC",
                ExchangeRate = 1,
                ActivityCode = emisor.ActivityCode,
                ReferenceDocumentClave = originalInvoice.Clave,
                ReferenceCode = "01",  // Anula documento de referencia
                ReferenceReason = reason,
                SaleCondition = "01",
                PaymentMethodCode = "01",
                CreatedAt = DateTime.UtcNow,
            };

            var xml = _xmlGenerator.GenerateCreditNoteXml(
                emisor, purchase.Client, purchase, creditNote, originalInvoice);

            var signedXml = _xmlSigner.SignXml(xml);
            creditNote.XmlSigned = signedXml;

            _db.Invoices.Add(creditNote);
            await _db.SaveChangesAsync();

            // Send to Hacienda
            try
            {
                var signedXmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(signedXml));
                var fecha = creditNote.EmissionDate?.ToString("yyyy-MM-ddTHH:mm:ss-06:00")
                    ?? DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss-06:00");

                var (statusCode, responseBody) = await _haciendaApi.SendDocumentAsync(
                    clave, fecha,
                    emisor.IdType, emisor.IdNumber,
                    purchase.Client?.ClientType, purchase.Client?.ClientId,
                    signedXmlBase64);

                creditNote.SentAt = DateTime.UtcNow;
                creditNote.HaciendaStatus = statusCode == 202 ? "sent" : "error";
                creditNote.HaciendaMessage = statusCode == 202
                    ? "NC recibida por Hacienda"
                    : $"HTTP {statusCode}: {TruncateString(responseBody, 500)}";
                creditNote.XmlResponse = responseBody;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send credit note {Clave}", clave);
                creditNote.HaciendaStatus = "error";
                creditNote.HaciendaMessage = $"Error de envío: {TruncateString(ex.Message, 500)}";
            }

            creditNote.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return creditNote;
        }

        public async Task<Invoice> CheckStatusAsync(int invoiceId)
        {
            var invoice = await _db.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId)
                ?? throw new InvalidOperationException($"Invoice {invoiceId} not found");

            if (string.IsNullOrEmpty(invoice.Clave))
                throw new InvalidOperationException("Invoice has no clave to query");

            if (invoice.HaciendaStatus == "accepted" || invoice.HaciendaStatus == "rejected")
            {
                // Already has a final status
                return invoice;
            }

            try
            {
                var (statusCode, responseBody) = await _haciendaApi.QueryDocumentStatusAsync(invoice.Clave);

                invoice.ResponseAt = DateTime.UtcNow;

                if (statusCode == 200)
                {
                    // Parse Hacienda response
                    using var doc = JsonDocument.Parse(responseBody);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("ind-estado", out var estado))
                    {
                        var status = estado.GetString()?.ToLowerInvariant();
                        invoice.HaciendaStatus = status switch
                        {
                            "aceptado" => "accepted",
                            "rechazado" => "rejected",
                            "procesando" => "sent",
                            _ => status ?? "sent"
                        };
                    }

                    if (root.TryGetProperty("respuesta-xml", out var respXml))
                    {
                        invoice.XmlResponse = respXml.GetString();
                    }

                    invoice.HaciendaMessage = $"Estado: {invoice.HaciendaStatus}";
                }
                else if (statusCode == 404)
                {
                    invoice.HaciendaMessage = "Documento no encontrado en Hacienda";
                }
                else
                {
                    invoice.HaciendaMessage = $"HTTP {statusCode}: {TruncateString(responseBody, 500)}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check status for invoice {InvoiceId}", invoiceId);
                invoice.HaciendaMessage = $"Error al consultar: {TruncateString(ex.Message, 500)}";
            }

            invoice.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return invoice;
        }

        public async Task<Invoice> ResendAsync(int invoiceId)
        {
            var invoice = await _db.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId)
                ?? throw new InvalidOperationException($"Invoice {invoiceId} not found");

            if (invoice.HaciendaStatus == "accepted")
                throw new InvalidOperationException("Cannot resend an accepted invoice");

            if (string.IsNullOrEmpty(invoice.XmlSigned))
                throw new InvalidOperationException("Invoice has no signed XML to resend");

            var emisor = await _db.CompanyConfigs
                .FirstOrDefaultAsync(c => c.IsActive)
                ?? throw new InvalidOperationException("No active company configuration found");

            var purchase = await LoadPurchaseWithDetails(invoice.PurchaseId);

            try
            {
                var signedXmlBase64 = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(invoice.XmlSigned));
                var fecha = invoice.EmissionDate?.ToString("yyyy-MM-ddTHH:mm:ss-06:00")
                    ?? DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss-06:00");

                var (statusCode, responseBody) = await _haciendaApi.SendDocumentAsync(
                    invoice.Clave!,
                    fecha,
                    emisor.IdType,
                    emisor.IdNumber,
                    purchase.Client?.HaciendaIdType,
                    purchase.Client?.ClientId,
                    signedXmlBase64);

                invoice.SentAt = DateTime.UtcNow;
                invoice.HaciendaStatus = statusCode == 202 ? "sent" : "error";
                invoice.HaciendaMessage = statusCode == 202
                    ? "Documento reenviado exitosamente"
                    : $"HTTP {statusCode}: {TruncateString(responseBody, 500)}";
                invoice.XmlResponse = responseBody;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend invoice {InvoiceId}", invoiceId);
                invoice.HaciendaStatus = "error";
                invoice.HaciendaMessage = $"Error al reenviar: {TruncateString(ex.Message, 500)}";
            }

            invoice.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return invoice;
        }

        // ── Private helpers ──

        private async Task<Purchase> LoadPurchaseWithDetails(int purchaseId)
        {
            return await _db.Purchases
                .Include(p => p.Client)
                .Include(p => p.Tax)
                .Include(p => p.SaleDetails)
                    .ThenInclude(sd => sd.Product)
                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId)
                ?? throw new InvalidOperationException($"Purchase {purchaseId} not found");
        }

        private static string MapPaymentMethodCode(string? method)
        {
            if (string.IsNullOrEmpty(method)) return "01";
            return method.ToLowerInvariant() switch
            {
                "efectivo" or "cash" => "01",
                "tarjeta" or "card" => "02",
                "cheque" => "03",
                "transferencia" or "transfer" or "sinpe" => "04",
                _ => "99"
            };
        }

        private static string TruncateString(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Length <= maxLength ? value : value[..maxLength];
        }
    }
}
