using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.CashRegister
{
    public class CashRegisterClosingDto
    {
        public decimal CashRegisterReportedAmount { get; set; }
        public decimal CashRegisterExpectedAmount { get; set; }
        public string? CashRegisterDifferenceReason { get; set; }
    }
}
