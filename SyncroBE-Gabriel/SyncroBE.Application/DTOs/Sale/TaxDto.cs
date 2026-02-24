using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.Tax
{
    public class TaxDto
    {
        public int TaxId { get; set; }
        public string TaxName { get; set; } = null!;
        public decimal Percentage { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateTaxDto
    {
        public string TaxName { get; set; } = null!;
        public decimal Percentage { get; set; }
    }

    public class UpdateTaxDto
    {
        public string TaxName { get; set; } = null!;
        public decimal Percentage { get; set; }
        public bool IsActive { get; set; }
    }
}