using AquaPass.Enums;
using AquaPass.Models;
using AquaPass.ModelsDto;
using Microsoft.EntityFrameworkCore;

namespace AquaPass.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto)
        {
            // 1. Нормалізація дати в UTC для PostgreSQL
            var visitDateUtc = DateTime.SpecifyKind(dto.VisitDate.Date, DateTimeKind.Utc);
            var nextDayUtc = visitDateUtc.AddDays(1);

            DayType dayType = visitDateUtc.DayOfWeek switch
            {
                DayOfWeek.Saturday => DayType.Weekend,
                DayOfWeek.Sunday => DayType.Weekend,
                _ => DayType.Weekday
            };

            var requestedTariffIds = dto.Items.Select(i => i.TariffId).Distinct().ToList();

            var tariffsFromDb = await _context.Tariffs
                .Where(t => requestedTariffIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id);

            // 2. Валідація тарифів за днем тижня
            foreach (var item in dto.Items)
            {
                if (!tariffsFromDb.TryGetValue(item.TariffId, out var tariff))
                {
                    throw new Exception($"Тариф з ID {item.TariffId} не знайдено.");
                }

                if (tariff.DayType != dayType)
                {
                    throw new Exception($"Тариф '{tariff.Name}' не діє для обраної дати ({dto.VisitDate:yyyy-MM-dd}).");
                }
            }

            // 3. Валідація та отримання шезлонгів
            var requestedSunbedIds = dto.Items
                .Where(i => i.SunbedId.HasValue)
                .Select(i => i.SunbedId!.Value)
                .ToList();

            Dictionary<Guid, Sunbed> sunbedsFromDb = new();

            if (requestedSunbedIds.Count > 0)
            {
                if (requestedSunbedIds.Count != requestedSunbedIds.Distinct().Count())
                {
                    throw new Exception("Неможливо обрати один і той самий шезлонг двічі в одному замовленні.");
                }

                // Перевірка зайнятості в базі через UTC-діапазон
                var occupiedSunbedIds = await _context.Tickets
                    .Where(t => t.Order.VisitDate >= visitDateUtc
                             && t.Order.VisitDate < nextDayUtc
                             && t.Order.Status != "Cancelled"
                             && t.SunbedId.HasValue
                             && requestedSunbedIds.Contains(t.SunbedId.Value))
                    .Select(t => t.SunbedId!.Value)
                    .ToListAsync();

                if (occupiedSunbedIds.Count > 0)
                {
                    throw new Exception("Один або декілька обраних шезлонгів уже зайняті на цю дату.");
                }

                // Отримуємо сутності шезлонгів, щоб знати їх Row та Number
                sunbedsFromDb = await _context.Sunbeds
                    .Where(s => requestedSunbedIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id);
            }

            // Знаходимо окремий тариф шезлонга (якщо потрібен)
            var sunbedTariff = await _context.Tariffs
                .FirstOrDefaultAsync(t => t.ServiceType == ServiceType.Sunbed && t.DayType == dayType);

            // 4. Формування замовлення
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"ORD-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                VisitDate = visitDateUtc,
                CustomerFirstName = dto.CustomerFirstName,
                CustomerLastName = dto.CustomerLastName,
                CustomerEmail = dto.CustomerEmail,
                CustomerPhone = dto.CustomerPhone,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            decimal totalAmount = 0;

            foreach (var item in dto.Items)
            {
                var tariff = tariffsFromDb[item.TariffId];

                for (int i = 0; i < item.Quantity; i++)
                {
                    decimal? sunbedPrice = null;

                    if (item.SunbedId.HasValue)
                    {
                        if (sunbedTariff == null)
                        {
                            throw new Exception("Не знайдено тарифу для оренди шезлонгів на цей день.");
                        }
                        sunbedPrice = sunbedTariff.Price;
                    }

                    var ticket = new Ticket
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        EntranceTariffId = tariff.Id,
                        EntrancePrice = tariff.Price,
                        TicketCode = Guid.NewGuid().ToString("N"),
                        Status = "Pending",
                        SunbedId = item.SunbedId,
                        SunbedPrice = sunbedPrice
                    };

                    order.Tickets.Add(ticket);
                    totalAmount += ticket.EntrancePrice + (ticket.SunbedPrice ?? 0m);
                }
            }

            order.TotalAmount = totalAmount;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Email confirmation will be sent after payment webhook confirms the order.

            // 5. Формування відповіді з назвами та підсумковими цінами
            var response = new OrderResponseDto(
                order.Id,
                order.OrderNumber,
                order.VisitDate,
                order.CustomerFirstName,
                order.CustomerLastName,
                order.CustomerEmail,
                order.CustomerPhone,
                order.TotalAmount,
                order.Status,
                order.CreatedAt,
                order.Tickets.Select(t =>
                {
                    var tariffName = tariffsFromDb.TryGetValue(t.EntranceTariffId, out var et)
                        ? et.Name
                        : "Вхідний квиток";

                    string? sunbedInfo = null;
                    if (t.SunbedId.HasValue && sunbedsFromDb.TryGetValue(t.SunbedId.Value, out var sb))
                    {
                        sunbedInfo = $"Ряд {sb.Row}, №{sb.Number}";
                    }

                    return new TicketResponseDto(
                        t.Id,
                        t.TicketCode,
                        tariffName,                             // Назва тарифу
                        t.EntrancePrice + (t.SunbedPrice ?? 0), // Підсумкова ціна (Price)
                        t.Status,
                        t.SunbedId.HasValue,
                        sunbedInfo,
                        t.SunbedPrice,
                        t.EntrancePrice + (t.SunbedPrice ?? 0)
                    );
                }).ToList()
            );

            return response;
        }

        public async Task<List<OrderResponseDto>> GetOrdersByPeriodAsync(DateTime fromDate, DateTime toDate)
        {
            var startUtc = DateTime.SpecifyKind(fromDate.Date, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(toDate.Date.AddDays(1), DateTimeKind.Utc);

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.VisitDate >= startUtc && o.VisitDate < endUtc)
                .Include(o => o.Tickets)
                    .ThenInclude(t => t.EntranceTariff)
                .Include(o => o.Tickets)
                    .ThenInclude(t => t.Sunbed)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(order => new OrderResponseDto(
                order.Id,
                order.OrderNumber,
                order.VisitDate,
                order.CustomerFirstName,
                order.CustomerLastName, 
                order.CustomerEmail,
                order.CustomerPhone,
                order.TotalAmount,
                order.Status,
                order.CreatedAt,
                order.Tickets.Select(t => new TicketResponseDto(
                    t.Id,
                    t.TicketCode,
                    t.EntranceTariff?.Name ?? "Вхідний квиток",
                    t.EntrancePrice + (t.SunbedPrice ?? 0m),
                    t.Status,
                    t.SunbedId.HasValue,
                    t.Sunbed != null ? $"Ряд {t.Sunbed.Row}, №{t.Sunbed.Number}" : null,
                    t.SunbedPrice,
                    t.EntrancePrice + (t.SunbedPrice ?? 0m)
                )).ToList()
            )).ToList();
        }

        public async Task<OrderResponseDto?> GetOrderByIdAsync(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.Tickets)
                    .ThenInclude(t => t.EntranceTariff)
                .Include(o => o.Tickets)
                    .ThenInclude(t => t.Sunbed)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;

            var response = new OrderResponseDto(
                order.Id,
                order.OrderNumber,
                order.VisitDate,
                order.CustomerFirstName,
                order.CustomerLastName,
                order.CustomerEmail,
                order.CustomerPhone,
                order.TotalAmount,
                order.Status,
                order.CreatedAt,
                order.Tickets.Select(t => new TicketResponseDto(
                    t.Id,
                    t.TicketCode,
                    t.EntranceTariff?.Name ?? "Вхідний квиток",
                    t.EntrancePrice + (t.SunbedPrice ?? 0),
                    t.Status,
                    t.SunbedId.HasValue,
                    t.Sunbed != null ? $"Ряд {t.Sunbed.Row}, №{t.Sunbed.Number}" : null,
                    t.SunbedPrice,
                    t.EntrancePrice + (t.SunbedPrice ?? 0)
                )).ToList()
            );

            return response;
        }
    }
}