using Microsoft.AspNetCore.Mvc;
using SyncroBE.Application.Interfaces;
using SyncroBE.Application.DTOs.Quote;

namespace SyncroBE.API.Controllers
{
    [Route("api/quotes")]
    [ApiController]
    public class QuoteController : ControllerBase
    {
        private readonly IQuoteRepository _repository;

        public QuoteController(IQuoteRepository repository)
        {
            _repository = repository;
        }

        //Consigue todas las cotizaciones
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _repository.GetAllAsync();
            return Ok(data.Select(q => new QuoteDto
            { 
                QuoteId = q.QuoteId,
                ClientId = q.ClientId,
                UserId = q.UserId,
                ClientName  = q.QuoteCustomer,
                QuoteTotal = q.QuoteDetails.Sum(qd => qd.LineTotal),
                QuoteIsValid = IsQuoteExpired(q.QuoteValidDate),
                UserName = q.User.UserName + q.User.UserLastname,
                QuoteValidTil = q.QuoteValidDate,
            }));
            //return Ok(data);
        }

        //Verifica si expiro la cotizacion
        private bool IsQuoteExpired(DateTime validDate)
        {
            bool result;
            result = (DateTime.Now.CompareTo(validDate) > 0) ? true : false;
            return result;
        }
    }
}
