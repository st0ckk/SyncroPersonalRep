using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Domain.Entities
{
    public class Client
    {
        public string ClientId { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public string? ClientEmail { get; set; }
        public string? ClientPhone { get; set; }
        public string? ClientElectronicInvoice { get; set; }
        public string? ClientType { get; set; }

        public int ClientPurchases { get; set; }
        public DateTime? ClientLastPurchase { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int? ProvinceCode { get; set; }
        public int? CantonCode { get; set; }
        public int? DistrictCode { get; set; }
        public string? ExactAddress { get; set; }

        public Province? Province { get; set; }
        public Canton? Canton { get; set; }
        public District? District { get; set; }

        public ClientLocation? Location { get; set; }

        public ICollection<Quote> Quotes { get; set; }
    }
}


