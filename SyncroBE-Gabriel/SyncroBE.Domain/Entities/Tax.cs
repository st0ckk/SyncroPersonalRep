using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Domain.Entities
{
    public class Tax
    {
        public int TaxId { get; set; }
        public string TaxName { get; set; } = null!;       // ej: "IVA 13%", "IVA 4%", "Exento"
        public decimal Percentage { get; set; }              // ej: 13.00, 4.00, 0.00
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Navegacion
        public IEnumerable<Purchase> Purchases { get; set; }



    }
}