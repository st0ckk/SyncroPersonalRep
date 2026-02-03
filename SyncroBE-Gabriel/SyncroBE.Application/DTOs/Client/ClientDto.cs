using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.Client
{
    public class ClientDto
    {
        public string ClientId { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public string? ClientEmail { get; set; }
        public string? ClientPhone { get; set; }
        public string? ClientType { get; set; }

        public bool IsActive { get; set; }

        public string? ClientElectronicInvoice { get; set; }

        public int? ProvinceCode { get; set; }
        public string? ProvinceName { get; set; }

        public int? CantonCode { get; set; }
        public string? CantonName { get; set; }

        public int? DistrictCode { get; set; }
        public string? DistrictName { get; set; }

        public string? ExactAddress { get; set; }
        public ClientLocationDto? Location { get; set; }
    }

}

