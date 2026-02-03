using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Domain.Entities
{
    public class Distributor
    {
        public int DistributorId { get; set; }
        public string DistributorCode { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}


