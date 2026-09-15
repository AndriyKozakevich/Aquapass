using AquaPass.Data;
using AquaPass.ModelsDto.Monobank;
using AquaPass.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AquaPass.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMonobankPaymentService _monobankService;
    private readonly IEmailService _emailService;
    private readonly ITicketPdfGenerator _pdfGenerator;
    private readonly AppDbContext _context;

    public PaymentsController(IMonobankPaymentService monobankService, IEmailService emailService, ITicketPdfGenerator pdfGenerator, AppDbContext context)
    {
        _monobankService = monobankService;
        _emailService = emailService;
        _pdfGenerator = pdfGenerator;
        _context = context;
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
        if (payload.Status == "success" && Guid.TryParse(payload.Reference, out var orderId))
        {
            var order = await _context.Orders
                .Include(o => o.Tickets)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null && order.Status != "Paid")
            {
                order.Status = "Paid";
                // активуємо або оновлюємо статуси квитків
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
            }
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