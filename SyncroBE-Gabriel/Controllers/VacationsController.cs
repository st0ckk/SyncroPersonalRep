using Microsoft.AspNetCore.Mvc;
using SyncroBE.Application.DTOs.Vacations;
using SyncroBE.Application.Interfaces;

namespace SyncroBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VacationsController : ControllerBase
    {
        private readonly IVacationService _vacationService;

        public VacationsController(IVacationService vacationService)
        {
            _vacationService = vacationService;
        }

        [HttpGet("balance/{userId:int}")]
        public async Task<IActionResult> GetBalance(int userId)
        {
            var result = await _vacationService.GetBalanceAsync(userId);
            return Ok(result);
        }

        [HttpPut("balance/{userId:int}/assign")]
        public async Task<IActionResult> AssignDays(int userId, [FromBody] AssignVacationDaysDto dto)
        {
            await _vacationService.AssignDaysAsync(userId, dto);
            return Ok(new { message = "Saldo de vacaciones actualizado correctamente." });
        }

        [HttpPost]
        public async Task<IActionResult> CreateVacation([FromBody] CreateVacationDto dto)
        {
            var vacation = await _vacationService.CreateVacationAsync(dto, dto.UserId);
            return Ok(vacation);
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var data = await _vacationService.GetUserVacationsAsync(userId);
            return Ok(data);
        }

        [HttpPut("{vacationId:int}/cancel")]
        public async Task<IActionResult> CancelVacation(int vacationId, [FromQuery] int? createdBy = null)
        {
            await _vacationService.CancelVacationAsync(vacationId, createdBy);
            return Ok(new { message = "Vacación cancelada y saldo reintegrado." });
        }

        [HttpPost("accrual/run")]
        public async Task<IActionResult> RunAccrual()
        {
            await _vacationService.RunMonthlyAccrualAsync();
            return Ok(new { message = "Acumulación mensual ejecutada correctamente." });
        }

        
        [HttpGet("calculate-days")]
        public async Task<IActionResult> CalculateDays([FromQuery] string start, [FromQuery] string end)
        {
            if (!DateTime.TryParse(start, out var startDate))
                return BadRequest("Fecha inicio inválida");

            if (!DateTime.TryParse(end, out var endDate))
                return BadRequest("Fecha final inválida");

            if (endDate.Date < startDate.Date)
                return BadRequest("La fecha final no puede ser menor a la inicial.");

            var days = await _vacationService.CalculateBusinessDaysAsync(startDate, endDate);

            return Ok(new { days });
        }
    }
}