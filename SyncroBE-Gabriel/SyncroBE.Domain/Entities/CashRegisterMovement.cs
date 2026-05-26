using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Domain.Entities
{
    public class CashRegisterMovement
    {
        public int CashRegisterMovementId { get; set; }
        public int CashRegisterId { get; set; }
        public int? PurchaseId { get; set; }
        public int UserId { get; set; }
        public string CashRegisterMovementType { get; set; }
        public string? CashRegisterMovementDescription { get; set; }
        public decimal CashRegisterMovementAmount { get; set; }
        public bool CashRegisterMovementManual { get; set; } = false;
        public DateTime CashRegisterMovementDate { get; set; } = DateTime.Now;
        
        //Navegacion
        public CashRegister CashRegister { get; set; }
        public User User { get; set; }
        public Purchase? Purchase { get; set; }
    }
}
