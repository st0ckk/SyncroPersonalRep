using Microsoft.AspNetCore.Mvc;
using SyncroBE.Application.DTOs.Address;
using SyncroBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

// este controlador maneja las peticiones relacionadas con los distritos,
// permitiendo obtener la lista de distritos filtrados por código de cantón.
namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/districts")]
    public class DistrictsController : ControllerBase
    {
        private readonly SyncroDbContext _context;

        public DistrictsController(SyncroDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDistricts([FromQuery] int canton_code)
        {
            var districts = await _context.Districts
                .Where(d => d.CantonCode == canton_code)
                .OrderBy(d => d.DistrictName)
                .Select(d => new DistrictDto(
                    d.DistrictCode,
                    d.DistrictName
                ))
                .ToListAsync();

            return Ok(districts);
        }

    }

}
