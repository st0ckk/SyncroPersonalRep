using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Domain.Entities
{
    public class Purchase
    { 
        public int PurchaseId { get; set; }
        public int UserId { get; set; }
        public int ClientId { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public bool PurchasePaid { get; set; }
    }
}
