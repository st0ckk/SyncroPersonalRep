using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Domain.Entities
{
    public class Quote
    {
        public int QuoteId { get; set; }
        public int UserId { get; set; }
        public string ClientId { get; set; }
        public int? DiscountId { get; set; }
        public string QuoteNumber { get; set; }
        public string QuoteCustomer { get; set; }
        public DateTime QuoteDate { get; set; } = DateTime.Now;
        public DateTime QuoteValidDate { get; set; }
        public string QuoteRemarks { get; set; }
        public string QuoteConditions { get; set; }
        public string QuoteStatus { get; set; }
        public bool QuoteDiscountApplied { get; set; } = false;
        public int QuoteDiscountPercentage { get; set; }
        public string QuoteDiscountReason { get; set; }
        public virtual User User { get; set; }
        public Client Client { get; set; }
        public ICollection<QuoteDetail> QuoteDetails { get; set; }
        public Discount? Discount { get; set; }
    }
}
