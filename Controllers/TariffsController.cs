using AquaPass.Enums;
using Microsoft.AspNetCore.Mvc;
using AquaPass.ModelsDto;
using AquaPass.Services;
using AquaPass.Models;
using Microsoft.AspNetCore.Authorization;
using AquaPass.Extensions;

namespace AquaPass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class TariffsController : ControllerBase
    {
        private readonly TariffService _service;

        public TariffsController(TariffService service)
        {
            _service = service;
        }

        // GET: api/tariffs
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tariffs = await _service.GetAllAsync();
            return Ok(tariffs);
        }

        // GET: api/tariffs/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
        var invalid = this.ValidateId(id, nameof(id));
        if (invalid != null) return invalid;
            var tariff = await _service.GetByIdAsync(id);
            if (tariff == null) return NotFound();
            return Ok(tariff);
        }

        // GET: api/tariffs/by-date?date=2026-07-28
        [AllowAnonymous]
        [HttpGet("by-date")]
        public async Task<IActionResult> GetTariffsByDate([FromQuery] DateTime date)
        {
            var tariffs = await _service.GetByDateAsync(date);
            DayType targetDayType = (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                ? DayType.Weekend
                : DayType.Weekday;

            return Ok(new
            {
                SelectedDate = date.ToString("yyyy-MM-dd"),
                DayType = targetDayType.ToString(),
                Tariffs = tariffs
            });
        }

        // POST: api/tariffs
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TariffCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var tariff = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Price = dto.Price,
                ServiceType = dto.ServiceType,
                DayType = dto.DayType,
                ZoneId = dto.ZoneId
            };

            var created = await _service.CreateAsync(tariff);

            var response = new TariffResponseDto(
                created.Id,
                created.Name,
                created.ServiceType,
                created.ServiceType.ToString(),
                created.Price,
                created.ZoneId,
                created.DayType.ToString()
            );

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        }

        // PUT: api/tariffs/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TariffUpdateDto dto)
        {
        var invalid = this.ValidateId(id, nameof(id));
        if (invalid != null) return invalid;
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _service.UpdateAsync(id, dto);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // DELETE: api/tariffs/{id}
        //[Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
