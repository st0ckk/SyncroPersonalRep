using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using SyncroBE.Application.DTOs.Client;
using SyncroBE.Application.DTOs.Product;
using SyncroBE.Application.DTOs.Quote;
using SyncroBE.Application.DTOs.QuoteDetails;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SyncroBE.API.Controllers
{
    [Route("api/quotes")]
    [ApiController]
    [Authorize(Roles = "SuperUsuario,Administrador,Vendedor")]
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
                DiscountId = q.DiscountId,
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
                QuoteDiscountApplied = q.QuoteDiscountApplied,
                QuoteDiscountPercentage = q.QuoteDiscountPercentage,
                QuoteDiscountReason = q.QuoteDiscountReason,
                QuoteDetails = q.QuoteDetails?.Select(qd => new QuoteDetailDto
                {
                    QuoteDetailId = qd.QuoteDetailId,
                    ProductId = qd.ProductId,
                    ProductName = qd.ProductName,
                    UnitPrice = qd.UnitPrice,
                    Quantity = qd.Quantity,
                    LineTotal = qd.LineTotal
                }).ToList()
            });
            return Ok(result);
            //return Ok(data);
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
                DiscountId = data.DiscountId,
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
                QuoteDiscountApplied = data.QuoteDiscountApplied,
                QuoteDiscountPercentage = data.QuoteDiscountPercentage,
                QuoteDiscountReason = data.QuoteDiscountReason,
                QuoteDetails = data.QuoteDetails?.Select(qd => new QuoteDetailDto
                {
                    QuoteDetailId = qd.QuoteDetailId,
                    ProductId = qd.ProductId,
                    ProductName = qd.ProductName,
                    UnitPrice = qd.UnitPrice,
                    Quantity = qd.Quantity,
                    LineTotal = qd.LineTotal
                }).ToList()
            };

            return Ok(dto);
        }

        //Consigue la ultima cotizacion de un client
        [HttpGet("client")]
        public async Task<IActionResult> GetLatestByClient (string clientId)
        {
            var data = await _repository.GetLatestByClient(clientId);
            if (data == null)
                return Ok();

            var dto = new QuoteDto
            {
                QuoteId = data.QuoteId,
                ClientId = data.ClientId,
                UserId = data.UserId,
                DiscountId = data.DiscountId,
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
                QuoteDiscountApplied = data.QuoteDiscountApplied,
                QuoteDiscountPercentage = data.QuoteDiscountPercentage,
                QuoteDiscountReason = data.QuoteDiscountReason,
                QuoteDetails = data.QuoteDetails?.Select(qd => new QuoteDetailDto
                {
                    QuoteDetailId = qd.QuoteDetailId,
                    ProductId = qd.ProductId,
                    ProductName = qd.ProductName,
                    UnitPrice = qd.UnitPrice,
                    Quantity = qd.Quantity,
                    LineTotal = qd.LineTotal
                }).ToList()
            };

            return Ok(dto);
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
                DiscountId = q.DiscountId,
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
                QuoteDiscountApplied = q.QuoteDiscountApplied,
                QuoteDiscountPercentage = q.QuoteDiscountPercentage,
                QuoteDiscountReason = q.QuoteDiscountReason,
                QuoteDetails = q.QuoteDetails?.Select(qd => new QuoteDetailDto
                {
                    QuoteDetailId = qd.QuoteDetailId,
                    ProductId = qd.ProductId,
                    ProductName = qd.ProductName,
                    UnitPrice = qd.UnitPrice,
                    Quantity = qd.Quantity,
                    LineTotal = qd.LineTotal
                }).ToList()
            });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(QuoteCreateUpdateDto dto)
        {
            Debug.WriteLine(dto.QuoteDetails);
            // Validar usuario del token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("No se pudo determinar el usuario");
            var quoteItems = new List<QuoteDetail>();

            var quote = new Quote
            {
                UserId = userId,
                ClientId = dto.ClientId,
                DiscountId = dto.DiscountId,
                QuoteNumber = dto.QuoteNumber,
                QuoteCustomer = dto.QuoteCustomer,
                QuoteValidDate = dto.QuoteValidTil,
                QuoteRemarks = dto.QuoteRemarks,
                QuoteConditions = dto.QuoteConditions,
                QuoteDiscountApplied = dto.QuoteDiscountApplied,
                QuoteDiscountPercentage = dto.QuoteDiscountPercentage,
                QuoteDiscountReason = dto.QuoteDiscountReason,
                QuoteStatus = dto.QuoteStatus,
            };

            foreach(QuoteDetailCreateUpdateDto item in dto.QuoteDetails)
            {
                quoteItems.Add(new QuoteDetail
                {
                    ProductId = item.ProductId,
                    ProductName= item.ProductName,
                    Quantity= item.Quantity,
                    UnitPrice= item.UnitPrice,
                    LineTotal= item.LineTotal
                });
            }
            
            await _repository.AddAsync(quote,quoteItems);
            return Ok();
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
                QuoteDiscountApplied = data.QuoteDiscountApplied,
                QuoteDiscountPercentage = data.QuoteDiscountPercentage,
                QuoteDiscountReason = data.QuoteDiscountReason,
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

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, QuoteCreateUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ValidationProblem(ModelState);

                var quote = await _repository.GetById(id);
                if (quote is null)
                    return NotFound();

                var quoteItems = new List<QuoteDetail>();

                // mappeo
                quote.QuoteValidDate = dto.QuoteValidTil;
                quote.QuoteStatus = dto.QuoteStatus;
                quote.QuoteRemarks = dto.QuoteRemarks;
                quote.QuoteConditions = dto.QuoteConditions;
                quote.QuoteDiscountApplied = dto.QuoteDiscountApplied;
                quote.QuoteDiscountPercentage = dto.QuoteDiscountPercentage;
                quote.QuoteDiscountReason = dto.QuoteDiscountReason;

                foreach (QuoteDetailCreateUpdateDto item in dto.QuoteDetails)
                {
                    quoteItems.Add(new QuoteDetail
                    {
                        QuoteId = quote.QuoteId,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.LineTotal
                    });
                }



                await _repository.UpdateAsync(quote, quoteItems);
                return Ok();
            }
            catch (Exception ex)
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
