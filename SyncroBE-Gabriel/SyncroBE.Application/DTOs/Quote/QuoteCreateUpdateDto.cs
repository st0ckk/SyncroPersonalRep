using SyncroBE.Application.DTOs.QuoteDetails;
using SyncroBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.Quote
{
    public class QuoteCreateUpdateDto
    {
        public string ClientId { get; set; }
        public int? DiscountId { get; set; }
        public string QuoteNumber { get; set; }
        public string QuoteCustomer { get; set; }
        public DateTime QuoteValidTil { get; set; }
        public string QuoteStatus { get; set; }
        public string QuoteRemarks { get; set; }
        public string QuoteConditions { get; set; }
        public bool QuoteDiscountApplied { get; set; } = false;
        public int QuoteDiscountPercentage { get; set; }
        public string QuoteDiscountReason { get; set; }

        public List<QuoteDetailCreateUpdateDto> QuoteDetails { get; set; }
    }
}
