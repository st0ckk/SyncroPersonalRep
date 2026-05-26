using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.CashRegister
{
    public class CashRegisterCreateUpdateDto
    {
        public decimal CashRegisterOpeningAmount { get; set; }
    }

    public class CashRegisterMovementCreateUpdateDto 
    {
        public int CashRegisterId { get; set; }
        public string CashRegisterMovementType { get; set; }
        public string? CashRegisterMovementDescription { get; set; }
        public decimal CashRegisterMovementAmount { get; set; }
        public bool CashRegisterMovementManual { get; set; }
    }
}
