using AquaPass.Enums;

namespace AquaPass.ModelsDto
{
    public record OrderValidationDto(
        Guid OrderId,
        string OrderNumber,
        DateTime VisitDate,
        string GuestName,
        string CustomerPhone,
        string CustomerEmail,
        OrderStatus OrderStatus,
        List<TicketScanItemDto> Tickets
    );
}
