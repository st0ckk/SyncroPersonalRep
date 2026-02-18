using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SyncroBE.Application.DTOs.Asset
{
    public class AssetUpdateDto
    {
        [Required]
        public int AssetId { get; set; }

        [Required]
        public string AssetName { get; set; } = null!;

        public string? Description { get; set; }
        public string? SerialNumber { get; set; }
        public string? Observations { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime AssignmentDate { get; set; }

        public bool IsActive { get; set; }
    }
}