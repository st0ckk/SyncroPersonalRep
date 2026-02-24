using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Domain.Entities
{
    public class Purchase
    {
        public int PurchaseId { get; set; }
        public int UserId { get; set; }
        public string ClientId { get; set; } = null!;       // ← cambiado de int a string
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
        public bool PurchasePaid { get; set; }

        // ── Impuesto a nivel de compra ──
        public int? TaxId { get; set; }
        public decimal TaxPercentage { get; set; }           // snapshot del % al momento de la venta
        public decimal Subtotal { get; set; }                // suma de LineTotals
        public decimal TaxAmount { get; set; }               // Subtotal * TaxPercentage / 100
        public decimal Total { get; set; }                   // Subtotal + TaxAmount

        public bool IsActive { get; set; } = true;           // para soft-delete (HU4)

        // ── Navegación ──
        public User User { get; set; } = null!;
        public Client Client { get; set; } = null!;
        public Tax? Tax { get; set; }
        public ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
    }
}
