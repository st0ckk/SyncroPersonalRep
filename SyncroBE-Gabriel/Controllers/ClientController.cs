using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroBE.Application.DTOs.Client;
using SyncroBE.Application.DTOs.Distributor;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;

namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/clients")]
    [Authorize(Roles = "SuperUsuario,Administrador,Vendedor,Chofer")]
    public class ClientController : ControllerBase
    {
        private readonly IClientRepository _repository;

        public ClientController(IClientRepository repository)
        {
            _repository = repository;
        }

        // get para mostrar todos los clientes activos
        [HttpGet]
        public async Task<IActionResult> GetActive()
        {
            var clients = await _repository.GetActiveAsync();
            return Ok(Map(clients));
        }

        // get para mostrar todos los clientes inactivos
        [HttpGet("inactive")]
        public async Task<IActionResult> GetInactive()
        {
            var clients = await _repository.GetInactiveAsync();
            return Ok(Map(clients));
        }

        // post para crear un nuevo cliente
        [HttpPost]
        public async Task<IActionResult> Create(ClientCreateUpdateDto dto)
        {
            var client = new Client
            {
                ClientId = dto.ClientId,
                ClientName = dto.ClientName,
                ClientEmail = dto.ClientEmail,
                ClientPhone = dto.ClientPhone,
                ClientType = dto.ClientType,
                ProvinceCode = dto.ProvinceCode,
                CantonCode = dto.CantonCode,
                DistrictCode = dto.DistrictCode,
                ExactAddress = dto.ExactAddress,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Location = dto.Location == null ? null : new ClientLocation
                {
                    Latitude = dto.Location.Latitude,
                    Longitude = dto.Location.Longitude,
                    Address = dto.Location.Address
                }
            };

            await _repository.AddAsync(client);
            return Ok();
        }



        // delete para desactivar un cliente, esto no lo elimina de la base de datos
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(string id)
        {
            await _repository.DeactivateAsync(id);
            return Ok();
        }

        // put para activar un cliente
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> Activate(string id)
        {
            await _repository.ActivateAsync(id);
            return Ok();
        }

        // mapper interno, no expuesto en la API   
        // convierte una lista de Client a una lista de ClientDto
        // considerar implementar en los demas a futuro
        private static IEnumerable<ClientDto> Map(IEnumerable<Client> clients)
        {
            return clients.Select(c => new ClientDto
            {
                ClientId = c.ClientId,
                ClientName = c.ClientName,
                ClientEmail = c.ClientEmail,
                ClientPhone = c.ClientPhone,
                ClientType = c.ClientType,
                ClientElectronicInvoice = c.ClientElectronicInvoice,
                IsActive = c.IsActive,

                ProvinceCode = c.ProvinceCode,
                ProvinceName = c.Province != null
                    ? c.Province.ProvinceName
                    : null,

                CantonCode = c.CantonCode,
                CantonName = c.Canton != null
                    ? c.Canton.CantonName
                    : null,

                DistrictCode = c.DistrictCode,
                DistrictName = c.District != null
                    ? c.District.DistrictName
                    : null,

                ExactAddress = c.ExactAddress,

                Location = c.Location == null
                    ? null
                    : new ClientLocationDto
                    {
                        Latitude = c.Location.Latitude,
                        Longitude = c.Location.Longitude,
                        Address = c.Location.Address
                    }
            });
        }



        // put para editar/actualizar un cliente por id
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, ClientCreateUpdateDto dto)
        {
            var client = await _repository.GetByIdAsync(id);
            if (client == null) return NotFound();

            client.ClientName = dto.ClientName;
            client.ClientEmail = dto.ClientEmail;
            client.ClientPhone = dto.ClientPhone;
            client.ClientType = dto.ClientType;
            client.ProvinceCode = dto.ProvinceCode;
            client.CantonCode = dto.CantonCode;
            client.DistrictCode = dto.DistrictCode;
            client.ExactAddress = dto.ExactAddress;
            client.UpdatedAt = DateTime.UtcNow;

            if (dto.Location != null)
            {
                client.Location ??= new ClientLocation();
                client.Location.Latitude = dto.Location.Latitude;
                client.Location.Longitude = dto.Location.Longitude;
                client.Location.Address = dto.Location.Address;
            }

            await _repository.UpdateAsync(client);
            return NoContent();
        }

        // GET: api/clients/{id}
        // Obtener un cliente por id (para editar / detalle)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var client = await _repository.GetByIdAsync(id);
            if (client == null)
                return NotFound();

            var dto = new ClientDto
            {
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                ClientEmail = client.ClientEmail,
                ClientPhone = client.ClientPhone,
                ClientType = client.ClientType,
                ClientElectronicInvoice = client.ClientElectronicInvoice,
                IsActive = client.IsActive,

                ProvinceCode = client.ProvinceCode,
                ProvinceName = client.Province?.ProvinceName,

                CantonCode = client.CantonCode,
                CantonName = client.Canton?.CantonName,

                DistrictCode = client.DistrictCode,
                DistrictName = client.District?.DistrictName,

                ExactAddress = client.ExactAddress,

                Location = client.Location == null
                    ? null
                    : new ClientLocationDto
                    {
                        Latitude = client.Location.Latitude,
                        Longitude = client.Location.Longitude,
                        Address = client.Location.Address
                    }
            };

            return Ok(dto);
        }

        // aca se busca por dinamicamente los distribuidores,
        // búsqueda dinámica para filtros / autocomplete
        [HttpGet("lookup")]
        public async Task<IActionResult> Lookup()
        {
            var data = await _repository.GetLookupAsync();

            return Ok(data.Select(c => new ClientLookupDto
            {
                ClientId = c.ClientId,
                ClientName = c.ClientName,
                ClientType = c.ClientType,
                ProvinceName = c.Province?.ProvinceName
            }));
        }



    }
}
