using AquaPass.ModelsDto;
using AquaPass.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AquaPass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SunbedController : ControllerBase
    {
        private readonly SunbedService _sunbedService;

        public SunbedController(SunbedService sunbedService)
        {
            _sunbedService = sunbedService;
        }

        #region GET Operations

        // GET: api/Sunbed
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sunbeds = await _sunbedService.GetAllAsync();

            return Ok(sunbeds);
        }

        // GET: api/Sunbed/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var sunbed = await _sunbedService.GetByIdAsync(id);

                return Ok(sunbed);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/Sunbed/available
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable(DateTime visitDate)
        {
            var sunbeds = await _sunbedService.GetAvailableSeubedsAsync(visitDate);

            return Ok(sunbeds);
        }

        // GET: api/Sunbed/row/{row}
        [HttpGet("row/{row}")]
        public async Task<IActionResult> GetByRow(string row)
        {
            var sunbeds = await _sunbedService.GetByRowAsync(row);
            return Ok(sunbeds);
        }

        // GET: api/Sunbed/search?row=A&number=5
        [HttpGet("search")]
        public async Task<IActionResult> GetByRowAndNumber([FromQuery] string row, [FromQuery] int number)
        {
            var sunbeds = await _sunbedService.GetByRowAndNumberAsync(row, number);
            return Ok(sunbeds);
        }

        #endregion

        #region POST Operations

        // POST: api/Sunbed
        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SunbedCreateDto dto)
        {
            var createdSunbed = await _sunbedService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = createdSunbed.Id }, createdSunbed);
        }

        // POST: api/Sunbed/range?row=A&count=10
        [HttpPost("range")]
        public async Task<IActionResult> CreateRange([FromQuery] string row, [FromQuery] int count)
        {
            await _sunbedService.CreateRangeAsync(row, count);

            return Ok(new { message = $"Успішно створено {count} шезлонгів у ряду {row}." });
        }

        #endregion

        #region PUT / PATCH Operations

        // PUT: api/Sunbed/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SunbedUpdateDto dto)
        {
            try
            {
                await _sunbedService.UpdateAsync(id, dto);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // PATCH: api/Sunbed/{id}/status?isAvailable=true
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> ToggleAvailability(Guid id, [FromQuery] bool isAvailable)
        {
            try
            {
                await _sunbedService.ToggleAvailabilityAsync(id, isAvailable);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        #endregion

        #region DELETE Operations

        // DELETE: api/Sunbed/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _sunbedService.DeleteAsync(id);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        #endregion
    }
}