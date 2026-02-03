using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Domain.Entities
{
    public class Province
    {
        public int ProvinceCode { get; set; }
        public string ProvinceName { get; set; } = null!;

        public ICollection<Canton> Cantons { get; set; } = new List<Canton>();
        public ICollection<Client>? Clients { get; set; }
    }
}
