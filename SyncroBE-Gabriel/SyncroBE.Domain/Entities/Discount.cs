using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Domain.Entities
{
    public class Discount
    {
        public int DiscountId { get; set; }
        public string DiscountName { get; set; }
        public int DiscountPercentage { get; set; }
        public bool IsActive { get; set; }
        public IEnumerable<Quote> Quotes { get; set; }

        public IEnumerable<Purchase> Purchases { get; set; }
    }
}
