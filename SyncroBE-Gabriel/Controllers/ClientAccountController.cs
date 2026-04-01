using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using SyncroBE.Application.DTOs.ClientAccount;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Services;
using System.Diagnostics;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SyncroBE.API.Controllers
{
    [Route("api/clientaccounts")]
    [ApiController]
    [Authorize(Roles = "SuperUsuario,Administrador,Vendedor")]
    public class ClientAccountController : ControllerBase
    {
        private readonly IClientAccountRepository _repository;
        private readonly IPdfService _pdfService;

        public ClientAccountController(IClientAccountRepository repository, IPdfService pdfService)
        {
            _repository = repository;
            _pdfService = pdfService;
        }

        //Consigue todas las cuentas de cliente
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _repository.GetAllAsync();
            var result = data.Select(ca => new ClientAccountDto
            {
                ClientAccountId= ca.ClientAccountId,
                ClientId = ca.ClientId,
                UserId = ca.UserId,
                CustomerName = ca.Client.ClientName,
                ClientAccountNumber = ca.ClientAccountNumber,
                ClientAccountOpeningDate = ca.ClientAccountOpeningDate,
                ClientAccountCreditLimit = ca.ClientAccountCreditLimit,
                ClientAccountInterestRate = ca.ClientAccountInterestRate,
                ClientAccountCurrentBalance = ca.ClientAccountCurrentBalance,
                ClientAccountStatus = ca.ClientAccountStatus,
                ClientAccountConditions = ca.ClientAccountConditions,
                Movements = ca.Movements?.Select(m => new ClientAccountMovementDto
                {
                    ClientAccountMovementId = m.ClientAccountMovementId,
                    ClientAccountMovementDate = m.ClientAccountMovementDate,
                    ClientAccountMovementDescription = m.ClientAccountMovementDescription,
                    ClientAccountMovementAmount = m.ClientAccountMovementAmount,
                    ClientAccountMovementOldBalance = m.ClientAccountMovementOldBalance,
                    ClientAccountMovementNewBalance = m.ClientAccountMovementNewBalance,
                    ClientAccountMovementType = m.ClientAccountMovementType,
                }).ToList()
            });
            return Ok(result);
            //return Ok(data);
        }

        //Consigue todas las cuentas de cliente que estan activas o suspendidas
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var data = await _repository.GetAllActiveAsync();
            var result = data.Select(ca => new ClientAccountDto
            {
                ClientAccountId = ca.ClientAccountId,
                ClientId = ca.ClientId,
                UserId = ca.UserId,
                CustomerName = ca.Client.ClientName,
                ClientAccountNumber = ca.ClientAccountNumber,
                ClientAccountOpeningDate = ca.ClientAccountOpeningDate,
                ClientAccountCreditLimit = ca.ClientAccountCreditLimit,
                ClientAccountInterestRate = ca.ClientAccountInterestRate,
                ClientAccountCurrentBalance = ca.ClientAccountCurrentBalance,
                ClientAccountStatus = ca.ClientAccountStatus,
                ClientAccountConditions = ca.ClientAccountConditions,
                Movements = ca.Movements?.Select(m => new ClientAccountMovementDto
                {
                    ClientAccountMovementId = m.ClientAccountMovementId,
                    ClientAccountMovementDate = m.ClientAccountMovementDate,
                    ClientAccountMovementDescription = m.ClientAccountMovementDescription,
                    ClientAccountMovementAmount = m.ClientAccountMovementAmount,
                    ClientAccountMovementOldBalance = m.ClientAccountMovementOldBalance,
                    ClientAccountMovementNewBalance = m.ClientAccountMovementNewBalance,
                    ClientAccountMovementType = m.ClientAccountMovementType,
                }).ToList()
            });
            return Ok(result);
            //return Ok(data);
        }

        //Consigue todas las cuentas de cliente que estan activas o suspendidas en base a un cliente
        [HttpGet("client")]
        public async Task<IActionResult> GetByClient(string client)
        {
            var data = await _repository.GetByClient(client);
            var result = data.Select(ca => new ClientAccountDto
            {
                ClientAccountId = ca.ClientAccountId,
                ClientId = ca.ClientId,
                UserId = ca.UserId,
                CustomerName = ca.Client.ClientName,
                ClientAccountNumber = ca.ClientAccountNumber,
                ClientAccountOpeningDate = ca.ClientAccountOpeningDate,
                ClientAccountCreditLimit = ca.ClientAccountCreditLimit,
                ClientAccountInterestRate = ca.ClientAccountInterestRate,
                ClientAccountCurrentBalance = ca.ClientAccountCurrentBalance,
                ClientAccountStatus = ca.ClientAccountStatus,
                ClientAccountConditions = ca.ClientAccountConditions,
                Movements = ca.Movements?.Select(m => new ClientAccountMovementDto
                {
                    ClientAccountMovementId = m.ClientAccountMovementId,
                    ClientAccountMovementDate = m.ClientAccountMovementDate,
                    ClientAccountMovementDescription = m.ClientAccountMovementDescription,
                    ClientAccountMovementAmount = m.ClientAccountMovementAmount,
                    ClientAccountMovementOldBalance = m.ClientAccountMovementOldBalance,
                    ClientAccountMovementNewBalance = m.ClientAccountMovementNewBalance,
                    ClientAccountMovementType = m.ClientAccountMovementType,
                }).ToList()
            });
            return Ok(result);
            //return Ok(data);
        }

        //Consigue una cuenta por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _repository.GetById(id);
            if (data == null)
                return NotFound();

            var dto = new ClientAccountDto
            {
                ClientAccountId = data.ClientAccountId,
                ClientId = data.ClientId,
                UserId = data.UserId,
                CustomerName = data.Client.ClientName,
                ClientAccountNumber = data.ClientAccountNumber,
                ClientAccountOpeningDate = data.ClientAccountOpeningDate,
                ClientAccountCreditLimit = data.ClientAccountCreditLimit,
                ClientAccountInterestRate = data.ClientAccountInterestRate,
                ClientAccountCurrentBalance = data.ClientAccountCurrentBalance,
                ClientAccountStatus = data.ClientAccountStatus,
                ClientAccountConditions = data.ClientAccountConditions,
                Movements = data.Movements?.Select(m => new ClientAccountMovementDto
                {
                    ClientAccountMovementId = m.ClientAccountMovementId,
                    ClientAccountMovementDate = m.ClientAccountMovementDate,
                    ClientAccountMovementDescription = m.ClientAccountMovementDescription,
                    ClientAccountMovementAmount = m.ClientAccountMovementAmount,
                    ClientAccountMovementOldBalance = m.ClientAccountMovementOldBalance,
                    ClientAccountMovementNewBalance = m.ClientAccountMovementNewBalance,
                    ClientAccountMovementType = m.ClientAccountMovementType,
                }).ToList()
            };

            return Ok(dto);
        }

        //Consigue todas las cuentas de credito
        [HttpGet("filter")]
        public async Task<IActionResult> Filter(DateTime? startDate, DateTime? endDate, string searchTerm = "", string status = "")
        {
            var data = await _repository.FilterAsync(startDate, endDate, searchTerm, status);
            var result = data.Select(ca => new ClientAccountDto
            {
                ClientAccountId = ca.ClientAccountId,
                ClientId = ca.ClientId,
                UserId = ca.UserId,
                CustomerName = ca.Client.ClientName,
                ClientAccountNumber = ca.ClientAccountNumber,
                ClientAccountOpeningDate = ca.ClientAccountOpeningDate,
                ClientAccountCreditLimit = ca.ClientAccountCreditLimit,
                ClientAccountInterestRate = ca.ClientAccountInterestRate,
                ClientAccountCurrentBalance = ca.ClientAccountCurrentBalance,
                ClientAccountStatus = ca.ClientAccountStatus,
                ClientAccountConditions = ca.ClientAccountConditions,
                Movements = ca.Movements?.Select(m => new ClientAccountMovementDto
                {
                    ClientAccountMovementId = m.ClientAccountMovementId,
                    ClientAccountMovementDate = m.ClientAccountMovementDate,
                    ClientAccountMovementDescription = m.ClientAccountMovementDescription,
                    ClientAccountMovementAmount = m.ClientAccountMovementAmount,
                    ClientAccountMovementOldBalance = m.ClientAccountMovementOldBalance,
                    ClientAccountMovementNewBalance = m.ClientAccountMovementNewBalance,
                    ClientAccountMovementType = m.ClientAccountMovementType,
                }).ToList()
            });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ClientAccountCreateUpdateDto dto)
        {
            // Validar usuario del token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("No se pudo determinar el usuario");

            var account = new ClientAccount
            {
                ClientId = dto.ClientId,
                UserId = userId,
                ClientAccountCreditLimit = dto.ClientAccountCreditLimit,
                ClientAccountInterestRate = dto.ClientAccountInterestRate,
                ClientAccountConditions = dto.ClientAccountConditions,
            };

            await _repository.AddAsync(account);
            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, ClientAccountCreateUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ValidationProblem(ModelState);

                var clientAccount = await _repository.GetById(id);
                if (clientAccount is null)
                    return NotFound();


                // mappeo
                clientAccount.ClientAccountCreditLimit = dto.ClientAccountCreditLimit;
                clientAccount.ClientAccountInterestRate = dto.ClientAccountInterestRate;
                clientAccount.ClientAccountConditions = dto.ClientAccountConditions;
                clientAccount.ClientAccountStatus = dto.ClientAccountStatus;

                await _repository.UpdateAsync(clientAccount);
                return Ok();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return BadRequest();
            }
        }

        [HttpPut("closeaccount")]
        public async Task<IActionResult> CloseAccount(int id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ValidationProblem(ModelState);

                var clientAccount = await _repository.GetById(id);
                if (clientAccount is null)
                    return NotFound();

                await _repository.CloseAccountAsync(clientAccount);
                return Ok();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return BadRequest();
            }
        }

        /*----------------------------------------------------------------------Movimientos------------------------------------------------------------------------------*/

        //Consigue todas las cuentas de credito
        [HttpGet("filtermovements")]
        public async Task<IActionResult> FilterMovements(int account, DateTime? startDate, DateTime? endDate, string searchTerm = "", string type = "")
        {
            var data = await _repository.FilterMovementsAsync(account, startDate, endDate, searchTerm, type);
            var result = data.Select(m => new ClientAccountMovementDto
            {
                ClientAccountMovementId = m.ClientAccountMovementId,
                ClientAccountMovementDate = m.ClientAccountMovementDate,
                ClientAccountMovementDescription = m.ClientAccountMovementDescription,
                ClientAccountMovementAmount = m.ClientAccountMovementAmount,
                ClientAccountMovementOldBalance = m.ClientAccountMovementOldBalance,
                ClientAccountMovementNewBalance = m.ClientAccountMovementNewBalance,
                ClientAccountMovementType = m.ClientAccountMovementType,
            });
            return Ok(result);
        }


        
        //Descargar copia de la cotizacion
        [HttpPost("report")]
        public async Task<IActionResult> GenerateMovementReport(int id, DateTime? startDate = null, DateTime? endDate = null, string searchTerm = "", string type = "")
        {
            var data = await _repository.GetById(id);
            if (data == null)
                return NotFound();

            var dto = new ClientAccountDto
            {
                ClientAccountId = data.ClientAccountId,
                ClientId = data.ClientId,
                UserId = data.UserId,
                CustomerName = data.Client.ClientName,
                ClientAccountNumber = data.ClientAccountNumber,
                ClientAccountOpeningDate = data.ClientAccountOpeningDate,
                ClientAccountCreditLimit = data.ClientAccountCreditLimit,
                ClientAccountInterestRate = data.ClientAccountInterestRate,
                ClientAccountCurrentBalance = data.ClientAccountCurrentBalance,
                ClientAccountStatus = data.ClientAccountStatus,
                ClientAccountConditions = data.ClientAccountConditions,
            };

            var movements = await _repository.FilterMovementsAsync(dto.ClientAccountId, startDate, endDate, searchTerm, type);

            dto.Movements = movements.Select(m => new ClientAccountMovementDto
            {
                ClientAccountMovementId = m.ClientAccountMovementId,
                ClientAccountMovementDate = m.ClientAccountMovementDate,
                ClientAccountMovementDescription = m.ClientAccountMovementDescription,
                ClientAccountMovementAmount = m.ClientAccountMovementAmount,
                ClientAccountMovementOldBalance = m.ClientAccountMovementOldBalance,
                ClientAccountMovementNewBalance = m.ClientAccountMovementNewBalance,
                ClientAccountMovementType = m.ClientAccountMovementType,
            }).ToList();

            //Generacion de PDF
            try
            {
                var browserFetch = new BrowserFetcher();
                await browserFetch.DownloadAsync();

                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
                using var page = await browser.NewPageAsync();

                string templateFileContent = await _pdfService.GenerateAccountMovementReportPdfCopy(dto, startDate, endDate);

                await page.SetContentAsync(templateFileContent);
                var pdfStream = await page.PdfStreamAsync(new PdfOptions { Format = PaperFormat.A4 });
                return File(pdfStream, "application/pdf", $"Movimientos - Cuenta #{dto.ClientAccountNumber}.pdf");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return BadRequest();
            }
        }
        
    }
}
