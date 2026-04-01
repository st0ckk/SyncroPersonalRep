using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.Sale
{
    public class CreateUpdateSaleDto
    {
        public string ClientId { get; set; } = null!;
        public bool PurchasePaid { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public int? TaxId { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal Subtotal { get; set; }               
        public decimal TaxAmount { get; set; }              
        public decimal Total { get; set; }                   
        public int? DiscountId { get; set; }
        public int RouteId { get; set; }
        public int? ClientAccountId { get; set; }
        public bool IsActive { get; set; }
        public bool PurchaseDiscountApplied { get; set; }
        public int PurchaseDiscountPercentage { get; set; }
        public string PurchaseDiscountReason { get; set; }
        public string PurchasePaymentMethod { get; set; }

        // ── Electronic Invoice (optional) ──
        /// <summary>
        /// Set to true to auto-generate and send an electronic invoice to Hacienda after creating the sale.
        /// Defaults to false so existing flow is not affected.
        /// </summary>
        public bool GenerateElectronicInvoice { get; set; } = false;

        /// <summary>
        /// Document type for Hacienda: "01"=Factura Electrónica, "04"=Tiquete Electrónico.
        /// Only used when GenerateElectronicInvoice is true.
        /// </summary>
        public string ElectronicInvoiceDocumentType { get; set; } = "01";

        public List<CreateUpdateSaleDetailDto> saleDetails { get; set; } = new();
    }

    public class CreateUpdateSaleDetailDto
    {
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}