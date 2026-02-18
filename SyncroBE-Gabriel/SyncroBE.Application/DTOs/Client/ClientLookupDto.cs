using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.Client
{
    public class ClientLookupDto
    {
        public string ClientId { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public string? ClientType { get; set; }

        public string? ProvinceName { get; set; }
    }

}


