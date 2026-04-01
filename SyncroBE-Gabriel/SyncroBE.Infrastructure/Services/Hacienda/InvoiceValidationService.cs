using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;

namespace SyncroBE.Infrastructure.Services.Hacienda
{
    public class InvoiceValidationService : IInvoiceValidationService
    {
        private readonly SyncroDbContext _db;

        public InvoiceValidationService(SyncroDbContext db)
        {
            _db = db;
        }

        public async Task<List<InvoiceValidationError>> ValidateForInvoiceAsync(
            int purchaseId, string documentType = "01")
        {
            var errors = new List<InvoiceValidationError>();

            // ── Validate document type ──
            var validDocTypes = new[] { "01", "02", "03", "04", "08", "09" };
            if (!validDocTypes.Contains(documentType))
            {
                errors.Add(new InvoiceValidationError("documentType",
                    $"Tipo de documento '{documentType}' no es válido. Valores aceptados: 01=FE, 02=ND, 03=NC, 04=TE, 08=FEC, 09=FEE"));
            }

            // ── Validate purchase exists ──
            var purchase = await _db.Purchases
                .Include(p => p.Client)
                .Include(p => p.Tax)
                .Include(p => p.SaleDetails)
                    .ThenInclude(sd => sd.Product)
                .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId);

            if (purchase == null)
            {
                errors.Add(new InvoiceValidationError("purchaseId",
                    $"Compra con ID {purchaseId} no encontrada"));
                return errors; // Can't validate further
            }

            // ── Check for existing accepted invoice ──
            var existingInvoice = await _db.Invoices
                .FirstOrDefaultAsync(i => i.PurchaseId == purchaseId
                    && i.DocumentType == documentType
                    && i.HaciendaStatus == "accepted");

            if (existingInvoice != null)
            {
                errors.Add(new InvoiceValidationError("purchaseId",
                    $"La compra {purchaseId} ya tiene una factura aceptada (clave: {existingInvoice.Clave})"));
            }

            // ── Validate purchase has line items ──
            if (purchase.SaleDetails == null || !purchase.SaleDetails.Any())
            {
                errors.Add(new InvoiceValidationError("saleDetails",
                    "La compra no tiene líneas de detalle"));
                return errors;
            }

            // ── Validate purchase total ──
            if (purchase.Total <= 0)
            {
                errors.Add(new InvoiceValidationError("total",
                    "El total de la compra debe ser mayor a 0"));
            }

            // ── Validate client (receptor) ──
            ValidateClient(purchase.Client, documentType, errors);

            // ── Validate line items ──
            ValidateLineItems(purchase.SaleDetails, errors);

            // ── Validate tax configuration ──
            ValidateTax(purchase, errors);

            // ── Validate company config (emisor) ──
            await ValidateEmisorAsync(errors);

            return errors;
        }

        public async Task<List<InvoiceValidationError>> ValidateForCreditNoteAsync(int originalInvoiceId)
        {
            var errors = new List<InvoiceValidationError>();

            var originalInvoice = await _db.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == originalInvoiceId);

            if (originalInvoice == null)
            {
                errors.Add(new InvoiceValidationError("originalInvoiceId",
                    $"Factura con ID {originalInvoiceId} no encontrada"));
                return errors;
            }

            if (originalInvoice.HaciendaStatus != "accepted")
            {
                errors.Add(new InvoiceValidationError("haciendaStatus",
                    $"Solo se pueden crear notas de crédito para facturas aceptadas. Estado actual: {originalInvoice.HaciendaStatus}"));
            }

            if (string.IsNullOrEmpty(originalInvoice.Clave))
            {
                errors.Add(new InvoiceValidationError("clave",
                    "La factura original no tiene clave numérica"));
            }

            // Also validate the underlying purchase data
            var purchaseErrors = await ValidateForInvoiceAsync(originalInvoice.PurchaseId, "03");
            // Filter out the "already has accepted invoice" error since that's expected for NC
            errors.AddRange(purchaseErrors.Where(e => e.Field != "purchaseId" || !e.Message.Contains("ya tiene una factura aceptada")));

            return errors;
        }

        // ── Private validation methods ──

        private void ValidateClient(Client? client, string documentType, List<InvoiceValidationError> errors)
        {
            // Tiquete Electrónico (04) doesn't require receptor
            if (documentType == "04") return;

            if (client == null)
            {
                errors.Add(new InvoiceValidationError("client",
                    "La compra no tiene un cliente asociado"));
                return;
            }

            if (string.IsNullOrWhiteSpace(client.ClientName))
            {
                errors.Add(new InvoiceValidationError("client.clientName",
                    "El cliente no tiene nombre"));
            }

            // For FE (01), client identification is required
            if (documentType == "01")
            {
                if (string.IsNullOrWhiteSpace(client.ClientId))
                {
                    errors.Add(new InvoiceValidationError("client.clientId",
                        "El cliente no tiene número de identificación (requerido para Factura Electrónica)"));
                }

                if (string.IsNullOrWhiteSpace(client.HaciendaIdType))
                {
                    errors.Add(new InvoiceValidationError("client.haciendaIdType",
                        "El cliente no tiene tipo de identificación de Hacienda asignado (01=Física, 02=Jurídica, 03=DIMEX, 04=NITE)"));
                }
                else
                {
                    var validTypes = new[] { "01", "02", "03", "04" };
                    if (!validTypes.Contains(client.HaciendaIdType))
                    {
                        errors.Add(new InvoiceValidationError("client.haciendaIdType",
                            $"Tipo de identificación de Hacienda '{client.HaciendaIdType}' no válido. Use: 01=Física, 02=Jurídica, 03=DIMEX, 04=NITE"));
                    }
                }
            }

            // Email is a warning (not blocking)
            if (string.IsNullOrWhiteSpace(client.ClientEmail))
            {
                errors.Add(new InvoiceValidationError("client.clientEmail",
                    "El cliente no tiene correo electrónico configurado (no se podrá enviar la factura por correo)", "warning"));
            }
        }

        private void ValidateLineItems(ICollection<SaleDetail> saleDetails, List<InvoiceValidationError> errors)
        {
            int lineNumber = 1;
            foreach (var item in saleDetails)
            {
                var prefix = $"lineaDetalle[{lineNumber}]";

                // CABYS code validation
                if (string.IsNullOrWhiteSpace(item.Product?.CabysCode))
                {
                    errors.Add(new InvoiceValidationError($"{prefix}.cabysCode",
                        $"El producto '{item.ProductName}' (ID: {item.ProductId}) no tiene código CABYS asignado"));
                }
                else if (item.Product.CabysCode.Length != 13 || !item.Product.CabysCode.All(char.IsDigit))
                {
                    errors.Add(new InvoiceValidationError($"{prefix}.cabysCode",
                        $"El código CABYS '{item.Product.CabysCode}' del producto '{item.ProductName}' debe tener exactamente 13 dígitos numéricos"));
                }

                // Quantity validation
                if (item.Quantity <= 0)
                {
                    errors.Add(new InvoiceValidationError($"{prefix}.quantity",
                        $"La cantidad de '{item.ProductName}' debe ser mayor a 0"));
                }

                // Price validation
                if (item.UnitPrice < 0)
                {
                    errors.Add(new InvoiceValidationError($"{prefix}.unitPrice",
                        $"El precio unitario de '{item.ProductName}' no puede ser negativo"));
                }

                // Product name validation (Hacienda max 200 chars)
                if (string.IsNullOrWhiteSpace(item.ProductName))
                {
                    errors.Add(new InvoiceValidationError($"{prefix}.productName",
                        "El nombre del producto no puede estar vacío"));
                }

                lineNumber++;
            }
        }

        private void ValidateTax(Purchase purchase, List<InvoiceValidationError> errors)
        {
            if (purchase.TaxPercentage > 0)
            {
                // If no Tax record but percentage is set, defaults will be used (IVA 13% = code 01, tarifa 08)
                if (purchase.Tax != null)
                {
                    if (string.IsNullOrWhiteSpace(purchase.Tax.HaciendaTaxCode))
                    {
                        errors.Add(new InvoiceValidationError("tax.haciendaTaxCode",
                            $"El impuesto '{purchase.Tax.TaxName}' no tiene código de Hacienda asignado (01=IVA, 02=ISC, 99=Otro)"));
                    }

                    if (string.IsNullOrWhiteSpace(purchase.Tax.HaciendaIvaRateCode))
                    {
                        errors.Add(new InvoiceValidationError("tax.haciendaIvaRateCode",
                            $"El impuesto '{purchase.Tax.TaxName}' no tiene código de tarifa IVA asignado (01-08)"));
                    }
                }
            }
        }

        private async Task ValidateEmisorAsync(List<InvoiceValidationError> errors)
        {
            var emisor = await _db.CompanyConfigs.FirstOrDefaultAsync(c => c.IsActive);

            if (emisor == null)
            {
                errors.Add(new InvoiceValidationError("companyConfig",
                    "No hay configuración de empresa (emisor) activa en la base de datos"));
                return;
            }

            if (string.IsNullOrWhiteSpace(emisor.CompanyName))
                errors.Add(new InvoiceValidationError("emisor.companyName", "El emisor no tiene razón social"));

            if (string.IsNullOrWhiteSpace(emisor.IdNumber))
                errors.Add(new InvoiceValidationError("emisor.idNumber", "El emisor no tiene número de identificación"));

            if (string.IsNullOrWhiteSpace(emisor.IdType))
                errors.Add(new InvoiceValidationError("emisor.idType", "El emisor no tiene tipo de identificación"));

            if (string.IsNullOrWhiteSpace(emisor.ActivityCode))
                errors.Add(new InvoiceValidationError("emisor.activityCode", "El emisor no tiene código de actividad económica"));

            if (string.IsNullOrWhiteSpace(emisor.Email))
                errors.Add(new InvoiceValidationError("emisor.email", "El emisor no tiene correo electrónico"));

            if (emisor.ProvinceCode <= 0 || emisor.ProvinceCode > 7)
                errors.Add(new InvoiceValidationError("emisor.provinceCode", "Código de provincia del emisor inválido (1-7)"));

            if (string.IsNullOrWhiteSpace(emisor.OtherAddress))
                errors.Add(new InvoiceValidationError("emisor.otherAddress", "El emisor no tiene dirección (OtrasSenas)"));
        }
    }
}
