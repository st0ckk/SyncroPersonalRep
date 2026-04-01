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
        public string? HaciendaIdType { get; set; }
        public string? ClientElectronicInvoice { get; set; }
        public string? ActivityCode { get; set; }

        public int ProvinceCode { get; set; }
        public int CantonCode { get; set; }
        public int DistrictCode { get; set; }
        public string ExactAddress { get; set; } = null!;

        // Exoneration fields
        public string? ExonerationDocType { get; set; }
        public string? ExonerationDocNumber { get; set; }
        public string? ExonerationInstitutionCode { get; set; }
        public string? ExonerationInstitutionName { get; set; }
        public DateTime? ExonerationDate { get; set; }
        public int? ExonerationPercentage { get; set; }

        public ClientLocationDto? Location { get; set; }
    }
}


