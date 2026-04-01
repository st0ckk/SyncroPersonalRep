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

        /// <summary>
        /// Hacienda identification type: 01=Física, 02=Jurídica, 03=DIMEX, 04=NITE
        /// </summary>
        public string? HaciendaIdType { get; set; }

        public int ClientPurchases { get; set; }
        public DateTime? ClientLastPurchase { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int? ProvinceCode { get; set; }
        public int? CantonCode { get; set; }
        public int? DistrictCode { get; set; }
        public string? ExactAddress { get; set; }

        // ── Exoneration (Exoneración) fields ──
        public string? ExonerationDocType { get; set; }         // Hacienda code: 01-07, 99
        public string? ExonerationDocNumber { get; set; }       // e.g. "AL-00023244-25"
        public string? ExonerationInstitutionCode { get; set; } // e.g. "01" = Min. Hacienda
        public string? ExonerationInstitutionName { get; set; } // e.g. "Ministerio de Hacienda"
        public DateTime? ExonerationDate { get; set; }          // Date exoneration was issued
        public int? ExonerationPercentage { get; set; }         // Percentage exempted (e.g. 13)

        // ── Receptor activity code for Hacienda ──
        public string? ActivityCode { get; set; }               // e.g. "702000" (6 digits)

        public Province? Province { get; set; }
        public Canton? Canton { get; set; }
        public District? District { get; set; }
        public ClientLocation? Location { get; set; }
        public ICollection<Quote> Quotes { get; set; }
        public ICollection<Purchase> Purchases { get; set; }
        public ICollection<ClientAccount> ClientAccounts { get; set; }
    }
}


