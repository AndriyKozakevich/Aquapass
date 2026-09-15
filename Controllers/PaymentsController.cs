using AquaPass.Data;
using AquaPass.ModelsDto.Monobank;
using AquaPass.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AquaPass.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMonobankPaymentService _monobankService;
    private readonly IEmailService _emailService;
    private readonly ITicketPdfGenerator _pdfGenerator;
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IMonobankPaymentService monobankService, IEmailService emailService, ITicketPdfGenerator pdfGenerator, AppDbContext context, ILogger<PaymentsController> logger)
    {
        _monobankService = monobankService;
        _emailService = emailService;
        _pdfGenerator = pdfGenerator;
        _context = context;
        _logger = logger;
    }

    // Endpoint for frontend to call after redirect (e.g., when Monobank redirect includes paid=true)
    // Marks order as paid and sends confirmation email. Use POST from frontend: POST /api/payments/confirm/{orderId}
    [AllowAnonymous]
    [HttpPost("confirm/{orderId:guid}")]
    public async Task<IActionResult> ConfirmPayment(Guid orderId)
    {
        try
        {
            _logger.LogInformation("ConfirmPayment called for order {OrderId}", orderId);

            var order = await _context.Orders
                .Include(o => o.Tickets)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound(new { message = "Order not found" });

            if (order.Status == "Paid") return Ok(new { message = "Already paid" });

            order.Status = "Paid";
            foreach (var ticket in order.Tickets)
            {
                ticket.Status = "Active";
            }

            await _context.SaveChangesAsync();

            var pdf = _pdfGenerator.GenerateOrderTicketsPdf(order);
            await _emailService.SendOrderConfirmationAsync(
                order.CustomerEmail,
                $"{order.CustomerFirstName} {order.CustomerLastName}",
                order.OrderNumber,
                pdf
            );

            _logger.LogInformation("Order {OrderId} confirmed and email sent", orderId);

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming payment for order {OrderId}", orderId);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    // 1. Клієнт викликає для отримання посилання на оплату
    [AllowAnonymous]
    [HttpPost("create-checkout/{orderId:guid}")]
    public async Task<IActionResult> CreateCheckout(Guid orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return NotFound(new { message = "Замовлення не знайдено" });

        var invoice = await _monobankService.CreateInvoiceAsync(
            order.Id,
            order.TotalAmount,
            $"Оплата замовлення №{order.OrderNumber} в AquaPass"
        );

        if (invoice == null || string.IsNullOrEmpty(invoice.PageUrl))
        {
            return BadRequest(new { message = "Не вдалося згенерувати платіж" });
        }

        return Ok(new { paymentUrl = invoice.PageUrl });
    }

    // 2. Monobank стукає сюди, коли клієнт оплатив карткою
    [AllowAnonymous]
    [HttpPost("mono-webhook")]
    public async Task<IActionResult> MonoWebhook([FromBody] MonoWebhookPayload payload)
    {
        try
        {
            if (payload.Status == "success" && Guid.TryParse(payload.Reference, out var orderId))
            {
                _logger.LogInformation("MonoWebhook received success for order {OrderId}", orderId);

                var order = await _context.Orders
                    .Include(o => o.Tickets)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order != null && order.Status != "Paid")
                {
                    order.Status = "Paid";
                    foreach (var ticket in order.Tickets)
                    {
                        ticket.Status = "Active";
                    }

                    await _context.SaveChangesAsync();

                    var pdf = _pdfGenerator.GenerateOrderTicketsPdf(order);
                    await _emailService.SendOrderConfirmationAsync(
                        order.CustomerEmail,
                        $"{order.CustomerFirstName} {order.CustomerLastName}",
                        order.OrderNumber,
                        pdf
                    );

                    _logger.LogInformation("Payment processed and email sent for order {OrderId}", orderId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MonoWebhook payload");
            // swallow exception to ensure 200 response to the gateway, but log for investigation
        }

        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail([FromServices] IEmailService emailService)
    {
        try
        {
            // Простий порожній або текстовий масив байтів для перевірки вкладення
            byte[] dummyPdf = System.Text.Encoding.UTF8.GetBytes("%PDF-1.4 тестовий файл");

            await emailService.SendOrderConfirmationAsync(
                toEmail: "andriy7work@gmail.com", // відправляємо тестовий лист самі собі
                customerName: "Андрій",
                orderNumber: "TEST-001",
                pdfBytes: dummyPdf
            );

            return Ok(new { success = true, message = "Лист успішно надіслано! Перевірте пошту (і спам)." });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                error = ex.Message,
                inner = ex.InnerException?.Message
            });
        }
    }
}