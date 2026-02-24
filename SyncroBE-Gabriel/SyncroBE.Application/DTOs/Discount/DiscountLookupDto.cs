using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.Discount
{
    public class DiscountLookupDto
    {
        public int DiscountId { get; set; }
        public string DiscountName { get; set; }
        public int DiscountPercentage { get; set; }
    }
}
