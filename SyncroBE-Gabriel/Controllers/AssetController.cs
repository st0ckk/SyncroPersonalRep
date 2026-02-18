using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroBE.Application.DTOs.Asset;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;

namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/assets")]
    [Authorize]
    public class AssetController : ControllerBase
    {
        private readonly IAssetRepository _repository;

        public AssetController(IAssetRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var assets = await _repository.GetAllAsync();
            return Ok(Map(assets));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var asset = await _repository.GetByIdAsync(id);
            if (asset == null) return NotFound();

            return Ok(new AssetDto
            {
                AssetId = asset.AssetId,
                AssetName = asset.AssetName,
                Description = asset.Description,
                SerialNumber = asset.SerialNumber,
                Observations = asset.Observations,
                UserId = asset.UserId,
                UserName = $"{asset.User.UserName} {asset.User.UserLastname}",
                AssignmentDate = asset.AssignmentDate,
                IsActive = asset.IsActive
            });
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var assets = await _repository.GetByUserIdAsync(userId);
            return Ok(Map(assets));
        }

        [HttpGet("inactive")]
        public async Task<IActionResult> GetInactive()
        {
            var assets = await _repository.GetInactiveAsync();
            return Ok(Map(assets));
        }

        [HttpPost]
        public async Task<IActionResult> Create(AssetCreateDto dto)
        {
            var asset = new Asset
            {
                AssetName = dto.AssetName,
                Description = dto.Description,
                SerialNumber = dto.SerialNumber,
                Observations = dto.Observations,
                UserId = dto.UserId,
                AssignmentDate = dto.AssignmentDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(asset);

            return CreatedAtAction(nameof(GetById), new { id = asset.AssetId }, asset.AssetId);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, AssetUpdateDto dto)
        {
            if (id != dto.AssetId) return BadRequest("ID mismatch");

            var asset = await _repository.GetByIdAsync(id);
            if (asset == null) return NotFound();

            asset.AssetName = dto.AssetName;
            asset.Description = dto.Description;
            asset.SerialNumber = dto.SerialNumber;
            asset.Observations = dto.Observations;
            asset.UserId = dto.UserId;
            asset.AssignmentDate = dto.AssignmentDate;
            asset.IsActive = dto.IsActive;
            asset.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(asset);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _repository.DeactivateAsync(id);
            return NoContent();
        }

        [HttpPut("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            await _repository.ActivateAsync(id);
            return NoContent();
        }

        private static IEnumerable<AssetDto> Map(IEnumerable<Asset> assets)
        {
            return assets.Select(a => new AssetDto
            {
                AssetId = a.AssetId,
                AssetName = a.AssetName,
                Description = a.Description,
                SerialNumber = a.SerialNumber,
                Observations = a.Observations,
                UserId = a.UserId,
                UserName = $"{a.User.UserName} {a.User.UserLastname}",
                AssignmentDate = a.AssignmentDate,
                IsActive = a.IsActive
            });
        }
    }
}