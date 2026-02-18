using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.Asset
{
    public class AssetDto
    {
        public int AssetId { get; set; }
        public string AssetName { get; set; } = null!;
        public string? Description { get; set; }
        public string? SerialNumber { get; set; }
        public string? Observations { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public DateTime AssignmentDate { get; set; }
        public bool IsActive { get; set; }
    }
}

