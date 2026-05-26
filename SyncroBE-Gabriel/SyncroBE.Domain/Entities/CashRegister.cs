using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Domain.Entities
{
    public class CashRegister
    {
        public int CashRegisterId { get; set; }
        public int UserId { get; set; }
        public decimal CashRegisterOpeningAmount { get; set; }
        public string CashRegisterNumber { get; set; }
        public DateTime CashRegisterOpeningDate { get; set; } = DateTime.Now;
        public DateTime? CashRegisterClosingDate { get; set; }
        public decimal? CashRegisterExpectedAmount { get; set; }
        public decimal? CashRegisterReportedAmount { get; set; }
        public decimal? CashRegisterAmountDifference { get; set; }
        public string? CashRegisterDifferenceReason{ get; set; }
        public string CashRegisterStatus { get; set; }

        //Navegacion
        public User User { get; set; }
        public ICollection<CashRegisterMovement> Movements { get; set; }

    }
}
