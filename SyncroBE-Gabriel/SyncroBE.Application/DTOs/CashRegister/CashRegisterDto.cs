using SyncroBE.Application.DTOs.ClientAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.CashRegister
{
    public class CashRegisterDto
    {
        public int CashRegisterId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal CashRegisterOpeningAmount { get; set; }
        public string CashRegisterNumber { get; set; }
        public DateTime CashRegisterOpeningDate { get; set; }
        public DateTime? CashRegisterClosingDate { get; set; }
        public decimal? CashRegisterExpectedAmount { get; set; }
        public decimal? CashRegisterReportedAmount { get; set; }
        public decimal? CashRegisterAmountDifference { get; set; }
        public string? CashRegisterDifferenceReason { get; set; }
        public string CashRegisterStatus { get; set; }
        public List<CashRegisterMovementDto>? Movements { get; set; }
    }

    public class CashRegisterMovementDto
    {
        public int CashRegisterMovementId { get; set; }
        public int CashRegisterId { get; set; }
        public int? PurchaseId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string CashRegisterMovementType { get; set; }
        public string? CashRegisterMovementDescription { get; set; }
        public decimal CashRegisterMovementAmount { get; set; }
        public bool CashRegisterMovementManual { get; set; } = false;
        public DateTime CashRegisterMovementDate { get; set; }
    }
}
