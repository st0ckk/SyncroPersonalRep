using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.DTOs.ElectronicInvoice;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;

namespace SyncroBE.Controllers
{
    [ApiController]
    [Route("api/electronic-invoice")]
    [Authorize]
    public class ElectronicInvoiceController : ControllerBase
    {
        private readonly IElectronicInvoiceService _invoiceService;
        private readonly IInvoiceValidationService _validationService;
        private readonly IInvoiceEmailService _emailService;
        private readonly IPdfService _pdfService;
        private readonly SyncroDbContext _db;

        public ElectronicInvoiceController(
            IElectronicInvoiceService invoiceService,
            IInvoiceValidationService validationService,
            IInvoiceEmailService emailService,
            IPdfService pdfService,
            SyncroDbContext db)
        {
            _invoiceService = invoiceService;
            _validationService = validationService;
            _emailService = emailService;
            _pdfService = pdfService;
            _db = db;
        }

        /// <summary>
        /// Validate invoice data before generating. Returns a list of errors/warnings.
        /// </summary>
        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] GenerateInvoiceRequestDto request)
        {
            var errors = await _validationService.ValidateForInvoiceAsync(
                request.PurchaseId, request.DocumentType);

            var validationErrors = errors.Where(e => e.Severity == "error").ToList();
            var validationWarnings = errors.Where(e => e.Severity == "warning").ToList();

            return Ok(new
            {
                isValid = !validationErrors.Any(),
                errors = validationErrors,
                warnings = validationWarnings
            });
        }

        /// <summary>
        /// Validate credit note data before generating.
        /// </summary>
        [HttpPost("validate-credit-note")]
        public async Task<IActionResult> ValidateCreditNote([FromBody] GenerateCreditNoteRequestDto request)
        {
            var errors = await _validationService.ValidateForCreditNoteAsync(request.OriginalInvoiceId);

            var validationErrors = errors.Where(e => e.Severity == "error").ToList();
            var validationWarnings = errors.Where(e => e.Severity == "warning").ToList();

            return Ok(new
            {
                isValid = !validationErrors.Any(),
                errors = validationErrors,
                warnings = validationWarnings
            });
        }

        /// <summary>
        /// Generate and send an electronic invoice for a purchase.
        /// Pre-validates data before sending to Hacienda.
        /// </summary>
        [HttpPost("generate")]
        public async Task<ActionResult<ElectronicInvoiceDto>> Generate(
            [FromBody] GenerateInvoiceRequestDto request)
        {
            try
            {
                // Pre-validate before generating
                var validationErrors = await _validationService.ValidateForInvoiceAsync(
                    request.PurchaseId, request.DocumentType);

                var errors = validationErrors.Where(e => e.Severity == "error").ToList();
                if (errors.Any())
                {
                    return BadRequest(new
                    {
                        error = "Validación fallida. Corrija los errores antes de generar la factura.",
                        validationErrors = errors
                    });
                }

                var invoice = await _invoiceService.GenerateAndSendAsync(
                    request.PurchaseId, request.DocumentType);

                return Ok(MapToDto(invoice));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al generar factura electrónica", detail = ex.Message });
            }
        }

        /// <summary>
        /// Generate a credit note (Nota de Crédito) for an existing invoice.
        /// </summary>
        [HttpPost("credit-note")]
        public async Task<ActionResult<ElectronicInvoiceDto>> GenerateCreditNote(
            [FromBody] GenerateCreditNoteRequestDto request)
        {
            try
            {
                // Pre-validate before generating
                var validationErrors = await _validationService.ValidateForCreditNoteAsync(
                    request.OriginalInvoiceId);

                var errors = validationErrors.Where(e => e.Severity == "error").ToList();
                if (errors.Any())
                {
                    return BadRequest(new
                    {
                        error = "Validación fallida. Corrija los errores antes de generar la nota de crédito.",
                        validationErrors = errors
                    });
                }

                var invoice = await _invoiceService.GenerateCreditNoteAsync(
                    request.OriginalInvoiceId, request.Reason);

                return Ok(MapToDto(invoice));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al generar nota de crédito", detail = ex.Message });
            }
        }

        /// <summary>
        /// Check the Hacienda status of an invoice.
        /// </summary>
        [HttpGet("{invoiceId}/status")]
        public async Task<ActionResult<InvoiceStatusDto>> CheckStatus(int invoiceId)
        {
            try
            {
                var invoice = await _invoiceService.CheckStatusAsync(invoiceId);

                return Ok(new InvoiceStatusDto
                {
                    InvoiceId = invoice.InvoiceId,
                    Clave = invoice.Clave,
                    HaciendaStatus = invoice.HaciendaStatus,
                    HaciendaMessage = invoice.HaciendaMessage,
                    SentAt = invoice.SentAt,
                    ResponseAt = invoice.ResponseAt
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Resend a failed or pending invoice to Hacienda.
        /// </summary>
        [HttpPost("{invoiceId}/resend")]
        public async Task<ActionResult<ElectronicInvoiceDto>> Resend(int invoiceId)
        {
            try
            {
                var invoice = await _invoiceService.ResendAsync(invoiceId);
                return Ok(MapToDto(invoice));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all electronic invoices with optional filtering.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ElectronicInvoiceDto>>> GetAll(
            [FromQuery] string? status = null,
            [FromQuery] string? documentType = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var query = _db.Invoices.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(i => i.HaciendaStatus == status);

            if (!string.IsNullOrEmpty(documentType))
                query = query.Where(i => i.DocumentType == documentType);

            if (fromDate.HasValue)
                query = query.Where(i => i.EmissionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => i.EmissionDate <= toDate.Value);

            var invoices = await query
                .OrderByDescending(i => i.CreatedAt)
                .Take(100)
                .ToListAsync();

            return Ok(invoices.Select(MapToDto).ToList());
        }

        /// <summary>
        /// Get a specific invoice by ID.
        /// </summary>
        [HttpGet("{invoiceId}")]
        public async Task<ActionResult<ElectronicInvoiceDto>> GetById(int invoiceId)
        {
            var invoice = await _db.Invoices.FindAsync(invoiceId);
            if (invoice == null) return NotFound();
            return Ok(MapToDto(invoice));
        }

        /// <summary>
        /// Get the invoice for a specific purchase.
        /// </summary>
        [HttpGet("by-purchase/{purchaseId}")]
        public async Task<ActionResult<ElectronicInvoiceDto>> GetByPurchase(int purchaseId)
        {
            var invoice = await _db.Invoices
                .Where(i => i.PurchaseId == purchaseId && i.DocumentType == "01")
                .FirstOrDefaultAsync();

            if (invoice == null) return NotFound();
            return Ok(MapToDto(invoice));
        }

        /// <summary>
        /// Download the signed XML for an invoice.
        /// </summary>
        [HttpGet("{invoiceId}/xml")]
        public async Task<IActionResult> DownloadXml(int invoiceId)
        {
            var invoice = await _db.Invoices.FindAsync(invoiceId);
            if (invoice == null) return NotFound();

            if (string.IsNullOrEmpty(invoice.XmlSigned))
                return BadRequest(new { error = "No hay XML firmado para esta factura" });

            var bytes = Encoding.UTF8.GetBytes(invoice.XmlSigned);
            var fileName = $"FE-{invoice.Clave ?? invoiceId.ToString()}.xml";

            return File(bytes, "application/xml", fileName);
        }

        /// <summary>
        /// Download the Hacienda response XML for an invoice.
        /// </summary>
        [HttpGet("{invoiceId}/xml-response")]
        public async Task<IActionResult> DownloadResponseXml(int invoiceId)
        {
            var invoice = await _db.Invoices.FindAsync(invoiceId);
            if (invoice == null) return NotFound();

            if (string.IsNullOrEmpty(invoice.XmlResponse))
                return BadRequest(new { error = "No hay respuesta XML de Hacienda" });

            var bytes = Encoding.UTF8.GetBytes(invoice.XmlResponse);
            var fileName = $"Respuesta-{invoice.Clave ?? invoiceId.ToString()}.xml";

            return File(bytes, "application/xml", fileName);
        }

        /// <summary>
        /// Send an existing invoice to the client's email.
        /// </summary>
        [HttpPost("{invoiceId}/send-email")]
        public async Task<IActionResult> SendEmail(int invoiceId, [FromQuery] string? overrideEmail = null)
        {
            try
            {
                var invoice = await _db.Invoices
                    .Include(i => i.Purchase)
                        .ThenInclude(p => p.Client)
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

                if (invoice == null) return NotFound();

                var clientEmail = overrideEmail ?? invoice.Purchase?.Client?.ClientEmail;
                if (string.IsNullOrEmpty(clientEmail))
                    return BadRequest(new { error = "El cliente no tiene correo electrónico configurado" });

                await _emailService.SendInvoiceEmailAsync(invoice, clientEmail);

                return Ok(new { message = $"Factura enviada a {clientEmail}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al enviar correo", detail = ex.Message });
            }
        }

        /// <summary>
        /// Generate, send to Hacienda, and email to client in one call.
        /// </summary>
        [HttpPost("generate-and-email")]
        public async Task<ActionResult<ElectronicInvoiceDto>> GenerateAndEmail(
            [FromBody] GenerateInvoiceRequestDto request)
        {
            try
            {
                var invoice = await _invoiceService.GenerateAndSendAsync(
                    request.PurchaseId, request.DocumentType);

                // Attempt to email — don't fail the whole request if email fails
                try
                {
                    var purchase = await _db.Purchases
                        .Include(p => p.Client)
                        .FirstAsync(p => p.PurchaseId == request.PurchaseId);

                    if (!string.IsNullOrEmpty(purchase.Client?.ClientEmail))
                    {
                        await _emailService.SendInvoiceEmailAsync(invoice, purchase.Client.ClientEmail);
                    }
                }
                catch (Exception emailEx)
                {
                    // Log but don't fail — invoice was already created and sent to Hacienda
                    return Ok(new
                    {
                        invoice = MapToDto(invoice),
                        emailWarning = $"Factura generada pero error al enviar correo: {emailEx.Message}"
                    });
                }

                return Ok(MapToDto(invoice));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al generar factura electrónica", detail = ex.Message });
            }
        }

        /// <summary>
        /// Get the invoice as an HTML document (for PDF rendering in frontend).
        /// </summary>
        [HttpGet("{invoiceId}/pdf")]
        public async Task<IActionResult> GetInvoicePdfHtml(int invoiceId)
        {
            try
            {
                var invoice = await _db.Invoices
                    .Include(i => i.Purchase)
                        .ThenInclude(p => p.Client)
                    .Include(i => i.Purchase)
                        .ThenInclude(p => p.SaleDetails)
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

                if (invoice == null) return NotFound();

                var emisor = await _db.CompanyConfigs.FirstOrDefaultAsync(c => c.IsActive);
                if (emisor == null) return BadRequest(new { error = "No hay configuración de empresa activa" });

                var html = await _pdfService.GenerateInvoicePdfHtml(
                    invoice, emisor, invoice.Purchase, invoice.Purchase.Client);

                return Content(html, "text/html", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al generar PDF", detail = ex.Message });
            }
        }

        // ── Mapping ──

        private static ElectronicInvoiceDto MapToDto(Invoice invoice)
        {
            return new ElectronicInvoiceDto
            {
                InvoiceId = invoice.InvoiceId,
                PurchaseId = invoice.PurchaseId,
                Clave = invoice.Clave,
                ConsecutiveNumber = invoice.ConsecutiveNumber,
                DocumentType = invoice.DocumentType,
                HaciendaStatus = invoice.HaciendaStatus,
                HaciendaMessage = invoice.HaciendaMessage,
                EmissionDate = invoice.EmissionDate,
                SentAt = invoice.SentAt,
                ResponseAt = invoice.ResponseAt,
                InvoiceTotal = invoice.InvoiceTotal,
                CurrencyCode = invoice.CurrencyCode,
                SaleCondition = invoice.SaleCondition,
                PaymentMethodCode = invoice.PaymentMethodCode,
                ActivityCode = invoice.ActivityCode,
                ReferenceDocumentClave = invoice.ReferenceDocumentClave,
                ReferenceCode = invoice.ReferenceCode,
                ReferenceReason = invoice.ReferenceReason,
                CreatedAt = invoice.CreatedAt
            };
        }
    }
}
