using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Domain.Entities
{
    public class District
    {
        public int DistrictCode { get; set; }
        public string DistrictName { get; set; } = null!;
        public int CantonCode { get; set; }

        public Canton Canton { get; set; } = null!;
        public ICollection<Client>? Clients { get; set; }
    }
}

