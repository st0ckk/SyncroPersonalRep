using Microsoft.AspNetCore.Mvc;
using SyncroBE.Application.DTOs.Discount;
using SyncroBE.Application.DTOs.Distributor;
using SyncroBE.Application.DTOs.Quote;
using SyncroBE.Application.DTOs.QuoteDetails;
using SyncroBE.Application.Interfaces;

namespace SyncroBE.API.Controllers
{
    [Route("api/discounts")]
    [ApiController]
    public class DiscountController : Controller
    {
        private readonly IDiscountRepository _repository;

        public DiscountController(IDiscountRepository repository)
        {
            _repository = repository;
        }

        //Consigue todos los descuentos
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _repository.GetAllAsync();
            var result = data.Select(q => new DiscountDto
            {
                DiscountId = q.DiscountId,
                DiscountName = q.DiscountName,
                DiscountPercentage = q.DiscountPercentage,
                DiscountStatus = q.IsActive,
            });
            return Ok(result);
        }

        //Consigue una cotizacion por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _repository.GetById(id);
            if (data == null)
                return NotFound();

            var dto = new DiscountDto
            {
                DiscountId = data.DiscountId,
                DiscountName = data.DiscountName,
                DiscountPercentage = data.DiscountPercentage,
                DiscountStatus = data.IsActive,
            };

            return Ok(dto);
        }

        // aca se busca por dinamicamente los descuentos,
        [HttpGet("lookup")]
        public async Task<IActionResult> Lookup()
        {
            var data = await _repository.GetLookupAsync();

            return Ok(data.Select(d => new DiscountLookupDto
            {
                DiscountId = d.DiscountId,
                DiscountName = d.DiscountName,
                DiscountPercentage = d.DiscountPercentage
            }));
        }
    }
}
