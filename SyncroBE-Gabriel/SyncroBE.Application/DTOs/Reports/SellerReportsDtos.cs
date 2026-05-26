namespace SyncroBE.Application.DTOs.Reports
{
    // ── Mis Ventas (Vendedor) ──
    public class MySellerSalesReportDto
    {
        public List<SalesReportRow> Rows { get; set; } = new();
        public SalesReportSummary Summary { get; set; } = new();
        public int TotalRows { get; set; }
        // Meta para el vendedor
        public decimal SalesGoal { get; set; }
        public decimal GoalProgress { get; set; } // porcentaje
    }

    // ── Top productos del vendedor ──
    public class MyTopProductsReportDto
    {
        public List<SellerProductRow> Rows { get; set; } = new();
        public int TotalUnitsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class SellerProductRow
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductType { get; set; } = null!;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal AvgUnitPrice { get; set; }
    }

    // ── Top clientes del vendedor ──
    public class MyTopClientsReportDto
    {
        public List<SellerClientRow> Rows { get; set; } = new();
        public int TotalClients { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class SellerClientRow
    {
        public string ClientId { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public string ClientType { get; set; } = null!;
        public int TotalPurchases { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
    }
}
