namespace SyncroBE.Application.DTOs.Reports
{
    // ── Reporte de Ventas ──
    public class SalesReportDto
    {
        public List<SalesReportRow> Rows { get; set; } = new();
        public SalesReportSummary Summary { get; set; } = new();
        public int TotalRows { get; set; }
    }

    public class SalesReportRow
    {
        public int PurchaseId { get; set; }
        public string PurchaseOrderNumber { get; set; } = null!;
        public DateTime PurchaseDate { get; set; }
        public string ClientId { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public int UserId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public bool IsPaid { get; set; }
        public bool IsActive { get; set; }
    }

    public class SalesReportSummary
    {
        public int TotalSales { get; set; }
        public decimal TotalSubtotal { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PaidCount { get; set; }
        public int UnpaidCount { get; set; }
    }

    // ── Reporte de Inventario ──
    public class InventoryReportDto
    {
        public List<InventoryReportRow> Rows { get; set; } = new();
        public InventoryReportSummary Summary { get; set; } = new();
    }

    public class InventoryReportRow
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductType { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal InventoryValue { get; set; }
        public string DistributorName { get; set; } = null!;
        public bool IsActive { get; set; }
    }

    public class InventoryReportSummary
    {
        public int TotalProducts { get; set; }
        public int TotalUnits { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
    }

    // ── Reporte de Facturación ──
    public class InvoiceReportDto
    {
        public List<InvoiceReportRow> Rows { get; set; } = new();
        public InvoiceReportSummary Summary { get; set; } = new();
        public int TotalRows { get; set; }
    }

    public class InvoiceReportRow
    {
        public int InvoiceId { get; set; }
        public string? Clave { get; set; }
        public string? ConsecutiveNumber { get; set; }
        public string? DocumentType { get; set; }
        public decimal InvoiceTotal { get; set; }
        public DateTime? EmissionDate { get; set; }
        public string? HaciendaStatus { get; set; }
        public string ClientName { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public int PurchaseId { get; set; }
        public string PurchaseOrderNumber { get; set; } = null!;
    }

    public class InvoiceReportSummary
    {
        public int TotalInvoices { get; set; }
        public decimal TotalAmount { get; set; }
        public int AcceptedCount { get; set; }
        public int RejectedCount { get; set; }
        public int PendingCount { get; set; }
    }
}
