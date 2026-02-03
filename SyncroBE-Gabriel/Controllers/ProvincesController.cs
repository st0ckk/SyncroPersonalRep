using Microsoft.AspNetCore.Mvc;
using SyncroBE.Application.DTOs.Address;
using SyncroBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

//este controlador maneja las peticiones relacionadas con las provincias,
//permitiendo obtener la lista de provincias.
namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/provinces")]
    public class ProvincesController : ControllerBase
    {
        private readonly SyncroDbContext _context;

        public ProvincesController(SyncroDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProvinces()
        {
            var provinces = await _context.Provinces
                .OrderBy(p => p.ProvinceName)
                .Select(p => new ProvinceDto(
                    p.ProvinceCode,
                    p.ProvinceName
                ))
                .ToListAsync();

            return Ok(provinces);
        }
    }

}
