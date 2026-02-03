using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.Product
{
    public class ProductCreateDto
    {
        [Required]
        public int DistributorId { get; set; }

        [Required]
        public string ProductName { get; set; } = string.Empty;

        public string? ProductType { get; set; }

        [Required]
        public decimal ProductPrice { get; set; }

        [Required]
        public int ProductQuantity { get; set; }
    }
}


