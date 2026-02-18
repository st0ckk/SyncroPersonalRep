using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using SyncroBE.Application.DTOs.Quote;
using SyncroBE.Application.DTOs.QuoteDetails;
using SyncroBE.Application.Interfaces;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace SyncroBE.API.Controllers
{
    [Route("api/quotes")]
    [ApiController]
    //[Authorize(Roles = "SuperUsuario,Administrador")]
    public class QuoteController : ControllerBase
    {
        private readonly IQuoteRepository _repository;
        private readonly IPdfService _pdfService;

        public QuoteController(IQuoteRepository repository, IPdfService pdfService)
        {
            _repository = repository;
            _pdfService = pdfService;
        }

        //Consigue todas las cotizaciones
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _repository.GetAllAsync();
            var result = data.Select(q => new QuoteDto
            {
                QuoteId = q.QuoteId,
                ClientId = q.ClientId,
                UserId = q.UserId,
                QuoteNumber = q.QuoteNumber,
                ClientName = q.QuoteCustomer,
                QuoteTotal = q.QuoteDetails?.Sum(qd => qd.LineTotal) ?? 0,
                QuoteIsValid = IsQuoteExpired(q.QuoteValidDate),
                UserName = q.User != null ? $"{q.User.UserName} {q.User.UserLastname}" : "No encontrado",
                QuoteValidTil = q.QuoteValidDate,
                QuoteStatus = q.QuoteStatus,
                QuoteDate = q.QuoteDate,
                QuoteRemarks = q.QuoteRemarks,
                QuoteConditions = q.QuoteConditions,
                QuoteDetails = q.QuoteDetails?.Select(qd => new QuoteDetailDto
                {
                    QuoteDetailId = qd.QuoteDetailId,
                    ProductName = qd.ProductName,
                    UnitPrice = qd.UnitPrice,
                    Quantity = qd.Quantity,
                    LineTotal = qd.LineTotal
                }).ToList()
            });
            return Ok(result);
            //return Ok(data);
        }

        //Consigue todas las cotizaciones
        [HttpGet("filter")]
        public async Task<IActionResult> Filter(DateTime? startDate, DateTime? endDate, string searchTerm = "", string status = "")
        {
            var data = await _repository.FilterAsync(startDate, endDate, searchTerm, status);
            var result = data.Select(q => new QuoteDto
            {
                QuoteId = q.QuoteId,
                ClientId = q.ClientId,
                UserId = q.UserId,
                QuoteNumber = q.QuoteNumber,
                ClientName = q.QuoteCustomer,
                QuoteTotal = q.QuoteDetails?.Sum(qd => qd.LineTotal) ?? 0,
                QuoteIsValid = IsQuoteExpired(q.QuoteValidDate) ? false : true,
                UserName = q.User != null ? $"{q.User.UserName} {q.User.UserLastname}" : "No encontrado",
                QuoteValidTil = q.QuoteValidDate,
                QuoteStatus = q.QuoteStatus,
                QuoteDate = q.QuoteDate,
                QuoteRemarks = q.QuoteRemarks,
                QuoteConditions = q.QuoteConditions,
                QuoteDetails = q.QuoteDetails?.Select(qd => new QuoteDetailDto
                {
                    QuoteDetailId = qd.QuoteDetailId,
                    ProductName = qd.ProductName,
                    UnitPrice = qd.UnitPrice,
                    Quantity = qd.Quantity,
                    LineTotal = qd.LineTotal
                }).ToList()
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

            var dto = new QuoteDto
            {
                QuoteId = data.QuoteId,
                ClientId = data.ClientId,
                UserId = data.UserId,
                QuoteNumber = data.QuoteNumber,
                ClientName = data.QuoteCustomer,
                QuoteTotal = data.QuoteDetails?.Sum(qd => qd.LineTotal) ?? 0,
                QuoteIsValid = IsQuoteExpired(data.QuoteValidDate) ? false : true,
                UserName = data.User != null ? $"{data.User.UserName} {data.User.UserLastname}" : "No encontrado",
                QuoteValidTil = data.QuoteValidDate,
                QuoteStatus = data.QuoteStatus,
                QuoteDate = data.QuoteDate,
                QuoteRemarks = data.QuoteRemarks,
                QuoteConditions = data.QuoteConditions,
                QuoteDetails = data.QuoteDetails?.Select(qd => new QuoteDetailDto
                {
                    QuoteDetailId = qd.QuoteDetailId,
                    ProductName = qd.ProductName,
                    UnitPrice = qd.UnitPrice,
                    Quantity = qd.Quantity,
                    LineTotal = qd.LineTotal
                }).ToList()
            };

            return Ok(dto);
        }

        //Descargar copia de la cotizacion
        [HttpPost("copy/{id}")]
        public async Task<IActionResult> GeneratePdf(int id)
        {
            var data = await _repository.GetById(id);
            if (data == null)
                return NotFound();

            var dto = new QuoteDto
            {
                QuoteId = data.QuoteId,
                ClientId = data.ClientId,
                UserId = data.UserId,
                QuoteNumber = data.QuoteNumber,
                ClientName = data.QuoteCustomer,
                QuoteTotal = data.QuoteDetails?.Sum(qd => qd.LineTotal) ?? 0,
                QuoteIsValid = IsQuoteExpired(data.QuoteValidDate) ? false : true,
                UserName = data.User != null ? $"{data.User.UserName} {data.User.UserLastname}" : "No encontrado",
                QuoteValidTil = data.QuoteValidDate,
                QuoteStatus = data.QuoteStatus,
                QuoteDate = data.QuoteDate,
                QuoteRemarks = data.QuoteRemarks,
                QuoteConditions = data.QuoteConditions,
                QuoteDetails = data.QuoteDetails?.Select(qd => new QuoteDetailDto
                {
                    QuoteDetailId = qd.QuoteDetailId,
                    ProductName = qd.ProductName,
                    UnitPrice = qd.UnitPrice,
                    Quantity = qd.Quantity,
                    LineTotal = qd.LineTotal
                }).ToList()
            };

            //Generacion de PDF
            try
            {
                var browserFetch = new BrowserFetcher();
                await browserFetch.DownloadAsync();

                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
                using var page = await browser.NewPageAsync();

                string templateFileContent = await _pdfService.GenerateQuotePdfCopy(dto);

                await page.SetContentAsync(templateFileContent);
                var pdfStream = await page.PdfStreamAsync(new PdfOptions { Format = PaperFormat.A4 });
                return File(pdfStream, "application/pdf", $"Cotizacion {dto.QuoteNumber}.pdf");
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return BadRequest();
            }
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
