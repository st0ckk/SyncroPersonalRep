using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.DTOs;
using SyncroBE.Application.DTOs.Discount;
using SyncroBE.Application.DTOs.Product;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;


namespace SyncroBE.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperUsuario,Administrador,Vendedor,Chofer")]
public class StockController : ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly SyncroDbContext _context;
    private readonly IHaciendaLookupService _haciendaLookup;

    public StockController(
        IProductRepository repository,
        SyncroDbContext context,
        IHaciendaLookupService haciendaLookup)
    {
        _repository = repository;
        _context = context;
        _haciendaLookup = haciendaLookup;
    }

    // get para mostrar todos los productos activos
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _repository.GetAllAsync();

        var result = products.Select(p => MapToDto(p));

        return Ok(result);
    }

    // get para mostrar todos los productos inactivos
    [HttpGet("inactive")]
    public async Task<IActionResult> GetInactive()
    {
        var products = await _repository.GetInactiveAsync();

        var result = products.Select(p => MapToDto(p));

        return Ok(result);
    }

    // get para mostrar un producto por id
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _repository.GetByIdAsync(id);
        if (p is null) return NotFound();

        return Ok(MapToDto(p));
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductCreateDto dto)
    {
        // Model validation (automática por [ApiController])
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        // Validar que el distribuidor exista
        var distributorExists = await _context.Distributors
            .AnyAsync(d => d.DistributorId == dto.DistributorId);

        if (!distributorExists)
            return BadRequest("invalid distributorid");

        var product = new Product
        {
            DistributorId = dto.DistributorId,
            ProductName = dto.ProductName,
            ProductType = dto.ProductType,
            ProductPrice = dto.ProductPrice,
            PulperoPrice = dto.PulperoPrice,
            ExtranjeroPrice = dto.ExtranjeroPrice,
            RuteroPrice = dto.RuteroPrice,
            ProductQuantity = dto.ProductQuantity,
            CabysCode = dto.CabysCode,
            IsService = dto.IsService,
            IsActive = true
        };

        await _repository.AddAsync(product);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.ProductId },
            product.ProductId
        );
    }



    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        ProductUpdateDto dto)
    {
        if (id != dto.ProductId)
            return BadRequest("ID mismatch");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var product = await _repository.GetByIdAsync(id);
        if (product is null)
            return NotFound();

        var distributorExists = await _context.Distributors
            .AnyAsync(d => d.DistributorId == dto.DistributorId);

        if (!distributorExists)
            return BadRequest("invalid distributorid");

        // mappeo
        product.DistributorId = dto.DistributorId;
        product.ProductName = dto.ProductName;
        product.ProductType = dto.ProductType;
        product.ProductPrice = dto.ProductPrice;
        product.PulperoPrice = dto.PulperoPrice;
        product.ExtranjeroPrice = dto.ExtranjeroPrice;
        product.RuteroPrice = dto.RuteroPrice;
        product.ProductQuantity = dto.ProductQuantity;
        product.IsActive = dto.IsActive;
        product.CabysCode = dto.CabysCode;
        product.IsService = dto.IsService;

        await _repository.UpdateAsync(product);

        return NoContent();
    }


    // delete para desactivar un producto, esto no lo elimina de la base de datos
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _repository.DeactivateAsync(id);
        return NoContent();
    }

    // put para reactivar un producto por id
    [HttpPut("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        await _repository.ActivateAsync(id);
        return NoContent();
    }

    // get para filtrar productos por nombre y/o distribuidor
    [HttpGet("filter")]
    public async Task<IActionResult> Filter(
    [FromQuery] string? name,
    [FromQuery] int? distributorId)
    {
        var products = await _repository.FilterAsync(name, distributorId);

        var result = products.Select(p => MapToDto(p));

        return Ok(result);
    }
    // get para buscar productos por nombre
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(Array.Empty<ProductReadDto>());

        var products = await _repository.SearchAsync(q);

        var result = products.Select(p => new ProductReadDto
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            ProductType = p.ProductType,
            ProductPrice = p.ProductPrice,
            PulperoPrice = p.PulperoPrice,
            ExtranjeroPrice = p.ExtranjeroPrice,
            RuteroPrice = p.RuteroPrice,
            ProductQuantity = p.ProductQuantity,
            IsActive = p.IsActive,
            CabysCode = p.CabysCode,
            IsService = p.IsService
        });

        return Ok(result);
    }
    // post para agregar stock a un producto, esto no es un CRUD ya que es para actualizar
    // inventario sin necesidad de buscar manualmente el producto
    [HttpPost("entry")]
    public async Task<IActionResult> AddStock(StockEntryDto dto)
    {
        if (dto.Quantity <= 0)
            return BadRequest("quantity must be greater than 0");

        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null)
            return NotFound();

        product.ProductQuantity += dto.Quantity;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // CABYS lookup from Hacienda API
    [HttpGet("cabys")]
    public async Task<IActionResult> SearchCabys([FromQuery] string q, [FromQuery] int top = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(new { total = 0, cantidad = 0, cabys = Array.Empty<object>() });

        var result = await _haciendaLookup.SearchCabysAsync(q, top);
        return Ok(result);
    }

    // ── Helper ──
    private static ProductDto MapToDto(Product p) => new()
    {
        ProductId = p.ProductId,
        DistributorId = p.DistributorId,
        DistributorName = p.Distributor?.Name ?? "",
        ProductName = p.ProductName,
        ProductType = p.ProductType,
        ProductPrice = p.ProductPrice,
        PulperoPrice = p.PulperoPrice,
        ExtranjeroPrice = p.ExtranjeroPrice,
        RuteroPrice = p.RuteroPrice,
        ProductQuantity = p.ProductQuantity,
        IsActive = p.IsActive,
        CabysCode = p.CabysCode,
        IsService = p.IsService
    };
}
