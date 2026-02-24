using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.DTOs.Tax;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;

namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/taxes")]
    [Authorize(Roles = "SuperUsuario,Administrador")]
    public class TaxController : ControllerBase
    {
        private readonly SyncroDbContext _context;

        public TaxController(SyncroDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous] // Vendedores necesitan leer los impuestos para crear ventas
        public async Task<IActionResult> GetAll()
        {
            var taxes = await _context.Taxes
                .Where(t => t.IsActive)
                .Select(t => new TaxDto
                {
                    TaxId = t.TaxId,
                    TaxName = t.TaxName,
                    Percentage = t.Percentage,
                    IsActive = t.IsActive
                })
                .ToListAsync();

            return Ok(taxes);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllIncludingInactive()
        {
            var taxes = await _context.Taxes
                .Select(t => new TaxDto
                {
                    TaxId = t.TaxId,
                    TaxName = t.TaxName,
                    Percentage = t.Percentage,
                    IsActive = t.IsActive
                })
                .ToListAsync();

            return Ok(taxes);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaxDto dto)
        {
            if (dto.Percentage < 0 || dto.Percentage >= 100)
                return BadRequest("El porcentaje debe ser entre 0 y 99.99");

            var tax = new Tax
            {
                TaxName = dto.TaxName,
                Percentage = dto.Percentage,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Taxes.Add(tax);
            await _context.SaveChangesAsync();

            return Ok(new { tax.TaxId, message = "Impuesto creado" });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaxDto dto)
        {
            var tax = await _context.Taxes.FindAsync(id);
            if (tax == null) return NotFound();

            if (dto.Percentage < 0 || dto.Percentage >= 100)
                return BadRequest("El porcentaje debe ser entre 0 y 99.99");

            tax.TaxName = dto.TaxName;
            tax.Percentage = dto.Percentage;
            tax.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return Ok("Impuesto actualizado");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var tax = await _context.Taxes.FindAsync(id);
            if (tax == null) return NotFound();

            tax.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok("Impuesto desactivado");
        }
    }
}