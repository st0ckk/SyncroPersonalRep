using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Domain.Entities
{
    public class ClientLocation
    {
        public int LocationId { get; set; }
        public string ClientId { get; set; } = null!;

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Client Client { get; set; } = null!;
    }
}


