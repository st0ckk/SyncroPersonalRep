using Microsoft.AspNetCore.Mvc;
using SyncroBE.Application.DTOs.Address;
using SyncroBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

// este controlador maneja las peticiones relacionadas con los cantones,
// permitiendo obtener la lista de cantones filtrados por código de provincia.
namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/cantons")]
    public class CantonsController : ControllerBase
    {
        private readonly SyncroDbContext _context;

        public CantonsController(SyncroDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCantons([FromQuery] int province_code)
        {
            var cantons = await _context.Cantons
                .Where(c => c.ProvinceCode == province_code)
                .OrderBy(c => c.CantonName)
                .Select(c => new CantonDto(
                    c.CantonCode,
                    c.CantonName
                ))
                .ToListAsync();

            return Ok(cantons);
        }

    }

}
