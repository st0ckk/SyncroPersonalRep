using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.Discount
{
    public class DiscountDto
    {
        public int DiscountId { get; set; }
        public string DiscountName { get; set; }
        public int DiscountPercentage { get; set; }
        public bool DiscountStatus { get; set; }
    }
}
