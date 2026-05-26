using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Win32;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using SyncroBE.Application.DTOs.CashRegister;
using SyncroBE.Application.DTOs.ClientAccount;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using System.Diagnostics;
using System.Security.Claims;

namespace SyncroBE.API.Controllers
{
    [ApiController]
    [Route("api/registers")]
    [Authorize(Roles = "SuperUsuario,Administrador,Vendedor,Chofer")]
    public class CashRegisterController : ControllerBase
    {
        private readonly ICashRegisterRepository _registerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPdfService _pdfService;

        public CashRegisterController(ICashRegisterRepository registerRepository, IUserRepository userRepository, IPdfService pdfService)
        {
            _registerRepository = registerRepository;
            _userRepository = userRepository;
            _pdfService = pdfService;
        }

        //Consigue todas las cuentas de cliente
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Validar usuario del token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { message = "No se pudo determinar el usuario" });

            var user = await _userRepository.GetById(userId);
            var data = await _registerRepository.GetAllAsync( user );
            var result = data.Select(cr => new CashRegisterDto
            {
                CashRegisterId = cr.CashRegisterId,
                UserId = cr.UserId,
                UserName = $"{cr.User.UserName} {cr.User.UserLastname}",
                CashRegisterOpeningAmount = cr.CashRegisterOpeningAmount,
                CashRegisterNumber = cr.CashRegisterNumber,
                CashRegisterOpeningDate = cr.CashRegisterOpeningDate,
                CashRegisterClosingDate = cr.CashRegisterClosingDate,
                CashRegisterExpectedAmount = cr.CashRegisterExpectedAmount,
                CashRegisterReportedAmount = cr.CashRegisterReportedAmount,
                CashRegisterAmountDifference = cr.CashRegisterAmountDifference,
                CashRegisterDifferenceReason = cr.CashRegisterDifferenceReason,
                CashRegisterStatus = cr.CashRegisterStatus,
                Movements = cr.Movements?.Select(m => new CashRegisterMovementDto
                {
                    CashRegisterMovementId = m.CashRegisterMovementId,
                    UserName = $"{m.User.UserName} {m.User.UserLastname}",
                    CashRegisterMovementType = m.CashRegisterMovementType,
                    CashRegisterMovementDescription = m.CashRegisterMovementDescription,
                    CashRegisterMovementAmount = m.CashRegisterMovementAmount,
                    CashRegisterMovementManual = m.CashRegisterMovementManual,
                    CashRegisterMovementDate = m.CashRegisterMovementDate,
                }).ToList()
            });
            return Ok(result);
            //return Ok(data);
        }

        //Verifica si el un usuario tiene cajas abiertas
        [HttpGet("openregisters")]
        public async Task<IActionResult> GetOpenRegistersByUser()
        {
            // Validar usuario del token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { message = "No se pudo determinar el usuario" });

            IEnumerable<CashRegister> data = new List<CashRegister>();
            data = await _registerRepository.GetUserOpenRegisters(userId);

            return Ok(data.Any());
        }

        //Verifica si el un usuario tiene cajas abiertas
        [HttpGet("expectedamount")]
        public async Task<IActionResult> GetExpectedAmount(int id)
        {
            decimal data = await _registerRepository.GetRegisterExpectedAmount(id);
            return Ok(data);
        }

        //Consigue una cuenta por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _registerRepository.GetById(id);
            if (data == null)
                return NotFound();

            var dto = new CashRegisterDto
            {
                CashRegisterId = data.CashRegisterId,
                UserId = data.UserId,
                UserName = $"{data.User.UserName} {data.User.UserLastname}",
                CashRegisterOpeningAmount = data.CashRegisterOpeningAmount,
                CashRegisterNumber = data.CashRegisterNumber,
                CashRegisterOpeningDate = data.CashRegisterOpeningDate,
                CashRegisterClosingDate = data.CashRegisterClosingDate,
                CashRegisterExpectedAmount = data.CashRegisterExpectedAmount,
                CashRegisterReportedAmount = data.CashRegisterReportedAmount,
                CashRegisterAmountDifference = data.CashRegisterAmountDifference,
                CashRegisterDifferenceReason = data.CashRegisterDifferenceReason,
                CashRegisterStatus = data.CashRegisterStatus,
            };

            return Ok(dto);
        }

        //Consigue todas las cuentas de credito
        [HttpGet("filter")]
        public async Task<IActionResult> Filter(DateTime? startDate, DateTime? endDate, string searchTerm = "", string status = "")
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { message = "No se pudo determinar el usuario" });

            var user = await _userRepository.GetById(userId);

            var data = await _registerRepository.FilterAsync(startDate, endDate, searchTerm, status, user);
            var result = data.Select(cr => new CashRegisterDto
            {
                CashRegisterId = cr.CashRegisterId,
                UserId = cr.UserId,
                UserName = $"{cr.User.UserName} {cr.User.UserLastname}",
                CashRegisterOpeningAmount = cr.CashRegisterOpeningAmount,
                CashRegisterNumber = cr.CashRegisterNumber,
                CashRegisterOpeningDate = cr.CashRegisterOpeningDate,
                CashRegisterClosingDate = cr.CashRegisterClosingDate,
                CashRegisterExpectedAmount = cr.CashRegisterExpectedAmount,
                CashRegisterReportedAmount = cr.CashRegisterReportedAmount,
                CashRegisterAmountDifference = cr.CashRegisterAmountDifference,
                CashRegisterDifferenceReason = cr.CashRegisterDifferenceReason,
                CashRegisterStatus = cr.CashRegisterStatus,
                Movements = cr.Movements?.Select(m => new CashRegisterMovementDto
                {
                    CashRegisterMovementId = m.CashRegisterMovementId,
                    UserName = $"{m.User.UserName} {m.User.UserLastname}",
                    CashRegisterMovementType = m.CashRegisterMovementType,
                    CashRegisterMovementDescription = m.CashRegisterMovementDescription,
                    CashRegisterMovementAmount = m.CashRegisterMovementAmount,
                    CashRegisterMovementManual = m.CashRegisterMovementManual,
                    CashRegisterMovementDate = m.CashRegisterMovementDate,
                }).ToList()
            });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CashRegisterCreateUpdateDto dto)
        {
            // Validar usuario del token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { message = "No se pudo determinar el usuario" });

            var register = new CashRegister
            {
                CashRegisterOpeningAmount = dto.CashRegisterOpeningAmount,
                CashRegisterStatus = "open",
                UserId = userId,
            };

            try
            {
                await _registerRepository.AddAsync(register);
                return Ok();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return StatusCode(500, new { message = "Error al abrir la caja. Por favor intente de nuevo." });
            }
        }

        [HttpPut("closeregister")]
        public async Task<IActionResult> CloseRegister(int id, CashRegisterClosingDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return ValidationProblem(ModelState);

                var register = await _registerRepository.GetById(id);
                if (register is null)
                    return NotFound();

                // mappeo
                register.CashRegisterReportedAmount = dto.CashRegisterReportedAmount;
                register.CashRegisterExpectedAmount = await _registerRepository.GetRegisterExpectedAmount(register.CashRegisterId);
                register.CashRegisterAmountDifference = register.CashRegisterExpectedAmount - dto.CashRegisterReportedAmount;
                register.CashRegisterDifferenceReason = dto.CashRegisterDifferenceReason != "" ? dto.CashRegisterDifferenceReason : "Sin definir";
                register.CashRegisterClosingDate = DateTime.Now;
                register.CashRegisterStatus = "closed";

                await _registerRepository.CloseRegisterAsync(register);
                return Ok();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return BadRequest(new { message = "Error al cerrar la caja. Por favor intente de nuevo." });
            }
        }

        //Descargar copia de la cotizacion
        [HttpPost("report")]
        public async Task<IActionResult> GenerateRegisterSummary(int id, DateTime? startDate = null, DateTime? endDate = null, string searchTerm = "", string type = "")
        {
            var data = await _registerRepository.GetById(id);
            if (data == null)
                return NotFound();

            var dto = new CashRegisterDto
            {
                CashRegisterId = data.CashRegisterId,
                UserId = data.UserId,
                UserName = $"{data.User.UserName} {data.User.UserLastname}",
                CashRegisterOpeningAmount = data.CashRegisterOpeningAmount,
                CashRegisterNumber = data.CashRegisterNumber,
                CashRegisterOpeningDate = data.CashRegisterOpeningDate,
                CashRegisterClosingDate = data.CashRegisterClosingDate,
                CashRegisterExpectedAmount = data.CashRegisterExpectedAmount != null ? data.CashRegisterExpectedAmount  : await _registerRepository.GetRegisterExpectedAmount(id),
                CashRegisterReportedAmount = data.CashRegisterReportedAmount,
                CashRegisterAmountDifference = data.CashRegisterAmountDifference,
                CashRegisterDifferenceReason = data.CashRegisterDifferenceReason,
                CashRegisterStatus = data.CashRegisterStatus,
            };

            var movements = await _registerRepository.FilterMovementsAsync(dto.CashRegisterId, startDate, endDate, searchTerm, type);

            dto.Movements = movements.Select(m => new CashRegisterMovementDto
            {
                CashRegisterMovementId = m.CashRegisterMovementId,
                UserName = $"{m.User.UserName} {m.User.UserLastname}",
                CashRegisterMovementType = m.CashRegisterMovementType,
                CashRegisterMovementDescription = m.CashRegisterMovementDescription,
                CashRegisterMovementAmount = m.CashRegisterMovementAmount,
                CashRegisterMovementManual = m.CashRegisterMovementManual,
                CashRegisterMovementDate = m.CashRegisterMovementDate,
            }).ToList();

            //Generacion de PDF
            try
            {
                var browserFetch = new BrowserFetcher();
                await browserFetch.DownloadAsync();

                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
                using var page = await browser.NewPageAsync();

                string templateFileContent = await _pdfService.GenerateRegisterSummaryPdfCopy(dto, startDate, endDate);

                await page.SetContentAsync(templateFileContent);
                var pdfStream = await page.PdfStreamAsync(new PdfOptions { Format = PaperFormat.A4 });
                return File(pdfStream, "application/pdf", $"Resumen - Caja #{dto.CashRegisterNumber}.pdf");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return BadRequest(new { message = "Error al generar el reporte. Por favor intente de nuevo." });
            }
        }

        /*----------------------------------------------------------------------Movimientos------------------------------------------------------------------------------*/

        //Consigue todas las cuentas de credito
        [HttpGet("filtermovements")]
        public async Task<IActionResult> FilterMovements(int register, DateTime? startDate, DateTime? endDate, string searchTerm = "", string type = "")
        {
            var data = await _registerRepository.FilterMovementsAsync(register, startDate, endDate, searchTerm, type);
            var result = data.Select(m => new CashRegisterMovementDto
            {
                CashRegisterMovementId = m.CashRegisterMovementId,
                UserName = $"{m.User.UserName} {m.User.UserLastname}",
                CashRegisterMovementType = m.CashRegisterMovementType,
                CashRegisterMovementDescription = m.CashRegisterMovementDescription,
                CashRegisterMovementAmount = m.CashRegisterMovementAmount,
                CashRegisterMovementManual = m.CashRegisterMovementManual,
                CashRegisterMovementDate = m.CashRegisterMovementDate,
            });
            return Ok(result);
        }

        // Agregar movimiento manual
        [HttpPost("manualmovement")]
        public async Task<IActionResult> CreateManualMovement(CashRegisterMovementCreateUpdateDto dto)
        {
            // Validar usuario del token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { message = "No se pudo determinar el usuario" });

            var register = await _registerRepository.GetById(dto.CashRegisterId);
            if (register == null)
                return NotFound(new { message = "Caja no encontrada." });

            if (register.CashRegisterStatus != "open")
                return BadRequest(new { message = "No se pueden agregar movimientos a una caja cerrada." });

            //Mapeo
            var movement = new CashRegisterMovement
            {
                CashRegisterId = dto.CashRegisterId,
                CashRegisterMovementType = dto.CashRegisterMovementType,
                CashRegisterMovementDescription = dto.CashRegisterMovementDescription,
                CashRegisterMovementAmount = dto.CashRegisterMovementAmount,
                CashRegisterMovementManual = dto.CashRegisterMovementManual,
                UserId = userId,
            };

            try
            {
                await _registerRepository.AddManualMovementAsync(movement);
                return Ok();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return StatusCode(500, new { message = "Error al registrar el movimiento. Por favor intente de nuevo." });
            }
        }

        //Consigue todos los movimientos de un registro
        [HttpGet("movements")]
        public async Task<IActionResult> GetAllMovements(int id)
        {
            var data = await _registerRepository.GetRegisterMovements(id);
            var result = data.Select(m => new CashRegisterMovementDto
            {
                CashRegisterMovementId = m.CashRegisterMovementId,
                UserName = $"{m.User.UserName} {m.User.UserLastname}",
                CashRegisterMovementType = m.CashRegisterMovementType,
                CashRegisterMovementDescription = m.CashRegisterMovementDescription,
                CashRegisterMovementAmount = m.CashRegisterMovementAmount,
                CashRegisterMovementManual = m.CashRegisterMovementManual,
                CashRegisterMovementDate = m.CashRegisterMovementDate,
            });
            return Ok(result);
        }
    }
}
