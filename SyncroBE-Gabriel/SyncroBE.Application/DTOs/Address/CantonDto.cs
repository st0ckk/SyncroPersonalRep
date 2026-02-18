using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.Address
{
    public record AssetDto(
        int CantonCode,
        string CantonName
    );
}

