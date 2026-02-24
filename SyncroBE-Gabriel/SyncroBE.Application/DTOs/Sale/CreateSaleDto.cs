using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.Sale
{
    public class CreateSaleDto
    {
        public string ClientId { get; set; } = null!;
        public bool PurchasePaid { get; set; }
        public int? TaxId { get; set; }
        public List<CreateSaleDetailDto> Details { get; set; } = new();
    }

    public class CreateSaleDetailDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}