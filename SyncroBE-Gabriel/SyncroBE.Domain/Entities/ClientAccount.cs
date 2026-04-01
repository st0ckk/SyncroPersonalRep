using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Domain.Entities
{
    public class ClientAccount
    {
        public int ClientAccountId { get; set; }
        public string ClientId { get; set; }
        public int UserId { get; set; }
        public string ClientAccountNumber { get; set; }
        public DateTime ClientAccountOpeningDate { get; set; } = DateTime.Now;
        public decimal ClientAccountCreditLimit { get; set; }
        public decimal ClientAccountInterestRate { get; set; }
        public decimal ClientAccountCurrentBalance { get; set; } = decimal.Zero;
        public string ClientAccountStatus { get; set; } = "active";
        public string  ClientAccountConditions { get; set; }

        // Navegacion
        public Client Client { get; set; }
        public User User { get; set; }
        public ICollection<ClientAccountMovement> Movements { get; set; }
        public ICollection<Purchase> Purchases { get; set; }

    }
}
