using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Domain.Entities
{
    public class ClientAccountMovement
    {
        public int ClientAccountMovementId { get; set; }
        public int ClientAccountId { get; set; }
        public DateTime ClientAccountMovementDate { get; set; }
        public string ClientAccountMovementDescription { get; set; }
        public decimal ClientAccountMovementAmount { get; set; }
        public decimal ClientAccountMovementOldBalance { get; set; }
        public decimal ClientAccountMovementNewBalance { get; set; }
        public string ClientAccountMovementType { get; set; }

        // Navegacion
        public ClientAccount ClientAccount { get; set; }
    }
}
