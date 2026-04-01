using SyncroBE.Application.DTOs.Quote;
using SyncroBE.Domain.Entities;
﻿using SyncroBE.Application.DTOs.ClientAccount;
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
        Task<string> GenerateInvoicePdfHtml(Invoice invoice, CompanyConfig emisor, Purchase purchase, Client client);

        Task<string> GenerateAccountMovementReportPdfCopy(ClientAccountDto account, DateTime? startDate, DateTime? endDate);
    }
}
