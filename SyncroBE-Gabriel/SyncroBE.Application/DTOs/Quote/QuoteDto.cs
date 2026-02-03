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
        public string? ClientName { get; set; } = null;
        public decimal QuoteTotal { get; set; }
        public bool QuoteIsValid { get; set; }
        public string UserName { get; set; }
        public DateTime QuoteValidTil { get; set; }
    }
}
