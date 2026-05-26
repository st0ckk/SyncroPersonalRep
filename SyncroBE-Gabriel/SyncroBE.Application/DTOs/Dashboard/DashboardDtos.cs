namespace SyncroBE.Application.DTOs.Dashboard
{
    // ── Admin Dashboard ──

    public class AdminDashboardDto
    {
        // KPI cards
        public int ActiveUsers { get; set; }
        public int SalesToday { get; set; }
        public int SalesThisWeek { get; set; }
        public int SalesThisMonth { get; set; }
        public decimal RevenueToday { get; set; }
        public decimal RevenueThisWeek { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int LowStockProducts { get; set; }

        // Detalle
        public List<TopClientDto> TopClients { get; set; } = new();
        public List<LowStockProductDto> LowStockList { get; set; } = new();
        public List<SalesByDayDto> SalesLast7Days { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
        public List<RecentAuditDto> RecentActivity { get; set; } = new();
    }

    public class TopClientDto
    {
        public string ClientId { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public int TotalPurchases { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class LowStockProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public string DistributorName { get; set; } = null!;
    }

    public class SalesByDayDto
    {
        public string Date { get; set; } = null!;   // yyyy-MM-dd
        public int Count { get; set; }
        public decimal Total { get; set; }
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RecentAuditDto
    {
        public long LogId { get; set; }
        public string Action { get; set; } = null!;
        public string EntityType { get; set; } = null!;
        public string EntityId { get; set; } = null!;
        public string? Details { get; set; }
        public string UserName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    // ── Seller Dashboard ──

    public class SellerDashboardDto
    {
        public int MySalesToday { get; set; }
        public int MySalesThisWeek { get; set; }
        public int MySalesThisMonth { get; set; }
        public decimal MyRevenueToday { get; set; }
        public decimal MyRevenueThisWeek { get; set; }
        public decimal MyRevenueThisMonth { get; set; }

        public List<TopClientDto> MyTopClients { get; set; } = new();
        public List<TopProductDto> MyTopProducts { get; set; } = new();
        public List<SalesByDayDto> MySalesLast7Days { get; set; } = new();
    }
}
