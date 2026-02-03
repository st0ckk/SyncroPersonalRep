using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.Distributor
{
    public class DistributorCreateDto
    {
        [Required, MaxLength(50)]
        public string DistributorCode { get; set; } = null!;

        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;

        [EmailAddress, MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }
    }
}

