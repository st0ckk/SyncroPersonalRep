using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.Product
{
    public class ProductReadDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? ProductType { get; set; }
        public decimal ProductPrice { get; set; }
        public int ProductQuantity { get; set; }
        public bool IsActive { get; set; }
    }
}

