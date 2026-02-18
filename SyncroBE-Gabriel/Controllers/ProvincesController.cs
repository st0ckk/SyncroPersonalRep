using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.DTOs.Address;
using SyncroBE.Infrastructure.Data;

//este controlador maneja las peticiones relacionadas con las provincias,
//permitiendo obtener la lista de provincias.
namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/provinces")]
    [Authorize(Roles = "SuperUsuario,Administrador,Vendedor,Chofer")]

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
                .Select(p => new AssetUpdateDto(
                    p.ProvinceCode,
                    p.ProvinceName
                ))
                .ToListAsync();

            return Ok(provinces);
        }
    }

}
