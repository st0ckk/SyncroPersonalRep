using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroBE.Application.DTOs.Distributor;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;

namespace Syncro.API.Controllers
{
    [ApiController]
    [Route("api/distributors")]
    [Authorize(Roles = "SuperUsuario,Administrador,Vendedor,Chofer")]
    public class DistributorController : ControllerBase
    {
        private readonly IDistributorRepository _repository;

        public DistributorController(IDistributorRepository repository)
        {
            _repository = repository;
        }

        // get para mostrar todos los distribuidores activos
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _repository.GetAllAsync();

            return Ok(data.Select(d => new DistributorDto
            {
                DistributorId = d.DistributorId,
                DistributorCode = d.DistributorCode,
                Name = d.Name,
                Email = d.Email,
                Phone = d.Phone,
                IsActive = d.IsActive 
            }));
        }

        // get para mostrar un distribuidor por id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var distributor = await _repository.GetByIdAsync(id);
            if (distributor == null)
                return NotFound();

            return Ok(new DistributorDto
            {
                DistributorId = distributor.DistributorId,
                DistributorCode = distributor.DistributorCode,
                Name = distributor.Name,
                Email = distributor.Email,
                Phone = distributor.Phone,
                IsActive = distributor.IsActive 
            });
        }

        // post para crear un nuevo distribuidor
        [HttpPost]
        public async Task<IActionResult> Create(DistributorCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _repository.CodeExistsAsync(dto.DistributorCode))
                return BadRequest("distributor code already exists");

            var distributor = new Distributor
            {
                DistributorCode = dto.DistributorCode,
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(distributor);

            return CreatedAtAction(
                nameof(GetById),
                new { id = distributor.DistributorId },
                distributor.DistributorId
            );
        }

        // put para actualizar un distribuidor existente
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, DistributorUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var distributor = await _repository.GetByIdAsync(id);
            if (distributor == null)
                return NotFound();

            distributor.DistributorCode = dto.DistributorCode;
            distributor.Name = dto.Name;
            distributor.Email = dto.Email;
            distributor.Phone = dto.Phone;

            await _repository.UpdateAsync(distributor);

            return NoContent();
        }

        // delete para desactivar un distribuidor por id, esto no elimina de la base de datos
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var distributor = await _repository.GetByIdAsync(id);
            if (distributor == null)
                return NotFound();

            await _repository.DeactivateAsync(distributor);

            return NoContent();
        }

        // get para mostrar todos los distribuidores inactivos
        [HttpGet("inactive")]
        public async Task<IActionResult> GetInactive()
        {
            var data = await _repository.GetInactiveAsync();

            return Ok(data.Select(d => new DistributorDto
            {
                DistributorId = d.DistributorId,
                DistributorCode = d.DistributorCode,
                Name = d.Name,
                Email = d.Email,
                Phone = d.Phone,
                IsActive = d.IsActive 
            }));
        }

        // aca se busca por dinamicamente los distribuidores,
        // conforme se escribe van apareciendo los resultados api/distributors/lookup
        [HttpGet("lookup")]
        public async Task<IActionResult> Lookup()
        {
            var data = await _repository.GetLookupAsync();

            return Ok(data.Select(d => new DistributorLookupDto
            {
                DistributorId = d.DistributorId,
                DistributorCode = d.DistributorCode,
                Name = d.Name
            }));
        }

        // esta api se encargar de activar el distribuidor por id
        // api/distributors/{id}/activate
        [HttpPut("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            var distributor = await _repository.GetByIdAsync(id);

            // GetByIdAsync solo trae activos
            if (distributor == null)
            {
                distributor = await _repository.GetByIdIncludingInactiveAsync(id);
                if (distributor == null)
                    return NotFound();
            }

            if (distributor.IsActive)
                return BadRequest("distributor already active");

            await _repository.ActivateAsync(distributor);

            return NoContent();
        }
    }
}
