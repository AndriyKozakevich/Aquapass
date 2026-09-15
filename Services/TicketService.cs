using AquaPass.ModelsDto;
using AquaPass.Enums;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace AquaPass.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _context;

    public TicketService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderValidationDto?> GetOrderByTicketCodeAsync(string ticketCode)
    {
        var cleanCode = ticketCode.Trim();

        // Робимо запит від Order -> Tickets, уникаючи циклічного Include
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Tickets)
                .ThenInclude(t => t.EntranceTariff)
            .Include(o => o.Tickets)
                .ThenInclude(t => t.Sunbed)
            .FirstOrDefaultAsync(o => o.Tickets.Any(t => t.TicketCode == cleanCode));

        if (order == null)
        {
            return null;
        }

        var guestName = $"{order.CustomerLastName} {order.CustomerFirstName}".Trim();
        if (string.IsNullOrWhiteSpace(guestName))
        {
            guestName = order.CustomerEmail;
        }

        var ticketsList = order.Tickets.Select(t => new TicketScanItemDto(
            t.Id,
            t.TicketCode,
            t.EntranceTariff?.Name ?? "Вхідний квиток",
            t.Sunbed != null ? $"Ряд {t.Sunbed.Row}, №{t.Sunbed.Number}" : null,
            t.EntrancePrice + (t.SunbedPrice ?? 0m),
            t.Status
        )).ToList();

        return new OrderValidationDto(
            order.Id,
            order.OrderNumber,
            order.VisitDate,
            guestName,
            order.CustomerPhone ?? "—",
            order.CustomerEmail,
            Enum.TryParse<OrderStatus>(order.Status, out var parsedStatus) ? parsedStatus : OrderStatus.Pending,
            ticketsList
        );
    }

    public async Task<BulkValidationResultDto> ValidateAllTicketsInOrderAsync(Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Tickets)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return new BulkValidationResultDto(false, "Замовлення не знайдено", 0);
        }

        if (order.Status == "Cancelled")
        {
            return new BulkValidationResultDto(false, "Замовлення скасовано!", 0);
        }

        var todayUtc = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var visitDateUtc = DateTime.SpecifyKind(order.VisitDate.Date, DateTimeKind.Utc);

        if (visitDateUtc != todayUtc)
        {
            return new BulkValidationResultDto(false, $"Квитки не на сьогодні! Дата: {order.VisitDate:dd.MM.yyyy}", 0);
        }

        var pendingTickets = order.Tickets.Where(t => t.Status != "Used").ToList();

        if (pendingTickets.Count == 0)
        {
            return new BulkValidationResultDto(false, "Усі квитки цього замовлення вже були погашені раніше.", 0);
        }

        foreach (var t in pendingTickets)
        {
            t.Status = "Used";
        }

        await _context.SaveChangesAsync();

        return new BulkValidationResultDto(true, $"Успішно валідовано {pendingTickets.Count} квитків!", pendingTickets.Count);
    }

    public async Task<TicketValidationResultDto> ValidateTicketAsync(string ticketCode)
    {
        var cleanCode = ticketCode.Trim();

        var ticket = await _context.Tickets
            .Include(t => t.Order)
            .Include(t => t.EntranceTariff)
            .Include(t => t.Sunbed)
            .FirstOrDefaultAsync(t => t.TicketCode == cleanCode);

        if (ticket == null)
        {
            return new TicketValidationResultDto(
                Success: false,
                Message: "Квиток із таким кодом не знайдено в системі."
            );
        }

        if (ticket.Order.Status == "Cancelled")
        {
            return new TicketValidationResultDto(
                Success: false,
                Message: "Замовлення скасовано. Вхід заборонено!",
                TicketCode: cleanCode,
                OrderNumber: ticket.Order.OrderNumber
            );
        }

        if (ticket.Status == "Used")
        {
            return new TicketValidationResultDto(
                Success: false,
                Message: "Увага! Цей квиток уже був використаний раніше!",
                TicketCode: cleanCode,
                OrderNumber: ticket.Order.OrderNumber
            );
        }

        // Перевірка дати візиту (UTC для PostgreSQL)
        var todayUtc = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var visitDateUtc = DateTime.SpecifyKind(ticket.Order.VisitDate.Date, DateTimeKind.Utc);

        if (visitDateUtc != todayUtc)
        {
            return new TicketValidationResultDto(
                Success: false,
                Message: $"Квиток недійсний на сьогодні! Дата візиту: {ticket.Order.VisitDate:dd.MM.yyyy}",
                TicketCode: cleanCode,
                OrderNumber: ticket.Order.OrderNumber
            );
        }

        // Погашення квитка
        ticket.Status = "Used";
        await _context.SaveChangesAsync();

        var guestName = $"{ticket.Order.CustomerLastName} {ticket.Order.CustomerFirstName}".Trim();
        if (string.IsNullOrWhiteSpace(guestName))
        {
            guestName = ticket.Order.CustomerEmail;
        }

        string? sunbedInfo = ticket.Sunbed != null
            ? $"Ряд {ticket.Sunbed.Row}, №{ticket.Sunbed.Number}"
            : null;

        return new TicketValidationResultDto(
            Success: true,
            Message: "Прохід дозволено!",
            TicketCode: ticket.TicketCode,
            GuestName: guestName,
            TariffName: ticket.EntranceTariff?.Name ?? "Вхідний квиток",
            SunbedInfo: sunbedInfo,
            OrderNumber: ticket.Order.OrderNumber
        );
    }

    public async Task<byte[]?> GetTicketQrCodeAsync(string ticketCode)
    {
        var cleanCode = ticketCode.Trim();

        var exists = await _context.Tickets.AnyAsync(t => t.TicketCode == cleanCode);
        if (!exists)
        {
            return null;
        }

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(cleanCode, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        return qrCode.GetGraphic(20);
    }

    public async Task<TicketResponseDto?> GetTicketByCodeAsync(string ticketCode)
    {
        var cleanCode = ticketCode.Trim();

        var ticket = await _context.Tickets
            .AsNoTracking()
            .Include(t => t.EntranceTariff)
            .Include(t => t.Sunbed)
            .FirstOrDefaultAsync(t => t.TicketCode == cleanCode);

        if (ticket == null)
        {
            return null;
        }

        return new TicketResponseDto(
            ticket.Id,
            ticket.TicketCode,
            ticket.EntranceTariff?.Name ?? "Вхідний квиток",
            ticket.EntrancePrice + (ticket.SunbedPrice ?? 0m),
            ticket.Status,
            ticket.SunbedId.HasValue,
            ticket.Sunbed != null ? $"Ряд {ticket.Sunbed.Row}, №{ticket.Sunbed.Number}" : null,
            ticket.SunbedPrice,
            ticket.EntrancePrice + (ticket.SunbedPrice ?? 0m)
        );
    }
}