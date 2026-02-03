using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.Client
{
    public class ClientCreateUpdateDto
    {
        public string ClientId { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public string? ClientEmail { get; set; }
        public string? ClientPhone { get; set; }
        public string ClientType { get; set; } = null!;

        public int ProvinceCode { get; set; }
        public int CantonCode { get; set; }
        public int DistrictCode { get; set; }
        public string ExactAddress { get; set; } = null!;

        public ClientLocationDto? Location { get; set; }
    }
}


