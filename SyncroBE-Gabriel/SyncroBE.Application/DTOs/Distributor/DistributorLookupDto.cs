using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.Distributor
{
    public class DistributorLookupDto
    {
        public int DistributorId { get; set; }
        public string DistributorCode { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
