using AquaPass.Services;
using Microsoft.AspNetCore.Mvc;
using AquaPass.Extensions;

namespace AquaPass.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet("by-code/{code}/order")]
    public async Task<IActionResult> GetOrderByTicketCode(string code)
    {
        var invalid = this.ValidateStringParam(code, nameof(code));
        if (invalid != null) return invalid;
        var order = await _ticketService.GetOrderByTicketCodeAsync(code);
        if (order == null)
        {
            return NotFound(new { message = "Замовлення за цим QR-кодом не знайдено." });
        }

        return Ok(order);
    }

    [HttpPost("orders/{orderId}/validate-all")]
    public async Task<IActionResult> ValidateAll(Guid orderId)
    {
        var invalid = this.ValidateId(orderId, nameof(orderId));
        if (invalid != null) return invalid;
        var result = await _ticketService.ValidateAllTicketsInOrderAsync(orderId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{code}/validate")]
    public async Task<IActionResult> Validate(string code)
    {
        var invalid = this.ValidateStringParam(code, nameof(code));
        if (invalid != null) return invalid;
        var result = await _ticketService.ValidateTicketAsync(code);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{code}/qr")]
    public async Task<IActionResult> GetQrCode(string code)
    {
        var invalid = this.ValidateStringParam(code, nameof(code));
        if (invalid != null) return invalid;
        var qrImage = await _ticketService.GetTicketQrCodeAsync(code);

        if (qrImage == null)
        {
            return NotFound("Квиток не знайдено.");
        }

        return File(qrImage, "image/png");
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var invalid = this.ValidateStringParam(code, nameof(code));
        if (invalid != null) return invalid;
        var ticket = await _ticketService.GetTicketByCodeAsync(code);

        if (ticket == null)
        {
            return NotFound("Квиток не знайдено.");
        }

        return Ok(ticket);
    }
}