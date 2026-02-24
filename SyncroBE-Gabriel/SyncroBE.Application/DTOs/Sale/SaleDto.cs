using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.Sale
{
    public class SaleDto
    {
        public int PurchaseId { get; set; }
        public string ClientId { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public DateTime PurchaseDate { get; set; }
        public bool PurchasePaid { get; set; }
        public string? TaxName { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public bool IsActive { get; set; }
        public List<SaleDetailDto> Details { get; set; } = new();
    }

    public class SaleDetailDto
    {
        public int SaleDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}