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
        public int ProductQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<QuoteDetail> QuoteDetails { get; set; }
    }
}


