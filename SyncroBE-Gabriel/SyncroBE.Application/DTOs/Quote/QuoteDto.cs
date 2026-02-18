using SyncroBE.Application.DTOs.QuoteDetails;
using SyncroBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.Quote
{
    public class QuoteDto
    {
        public int QuoteId { get; set; }
        public string ClientId { get; set; }
        public int UserId { get; set; }
        public string QuoteNumber { get; set; }
        public string? ClientName { get; set; } = null;
        public decimal QuoteTotal { get; set; }
        public bool QuoteIsValid { get; set; }
        public string UserName { get; set; }
        public DateTime QuoteValidTil { get; set; }
        public string QuoteStatus { get; set; }
        public DateTime QuoteDate { get; set; }
        public string? QuoteRemarks { get; set; }
        public string? QuoteConditions { get; set; }
        public List<QuoteDetailDto>? QuoteDetails { get; set; }
    }
}
