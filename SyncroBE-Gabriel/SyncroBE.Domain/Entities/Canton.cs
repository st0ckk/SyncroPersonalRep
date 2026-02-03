using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Domain.Entities
{
    public class Canton
    {
        public int CantonCode { get; set; }
        public string CantonName { get; set; } = null!;
        public int ProvinceCode { get; set; }

        public Province Province { get; set; } = null!;
        public ICollection<District> Districts { get; set; } = new List<District>();
        public ICollection<Client>? Clients { get; set; }
    }
}

