namespace SyncroBE.Domain.Entities
{
    public class Product
    {
        public int ProductId { get; set; }

        public int DistributorId { get; set; }
        public Distributor Distributor { get; set; } = null!;

        public string ProductName { get; set; } = null!;
        public string? ProductType { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal? PulperoPrice { get; set; }
        public decimal? ExtranjeroPrice { get; set; }
        public decimal? RuteroPrice { get; set; }
        public int ProductQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        // ── Electronic invoice fields ──
        public string? CabysCode { get; set; }             // CABYS code (13 digits)
        public bool IsService { get; set; }                 // true=Service, false=Merchandise

        public virtual ICollection<QuoteDetail> QuoteDetails { get; set; }

        public virtual ICollection<SaleDetail> SaleDetails { get; set; }
    }
}


