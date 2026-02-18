using SyncroBE.Application.DTOs.Quote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.Interfaces
{
    public interface IPdfService
    {
        Task<string> GenerateQuotePdfCopy(QuoteDto quote);
    }
}
