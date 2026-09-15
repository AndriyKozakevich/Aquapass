using AquaPass.Enums;

namespace AquaPass.ModelsDto
{
    public record TicketScanResultDto(
        Guid OrderId,
        string OrderNumber,
        DateTime VisitDate,
        string CustomerEmail,
        string CustomerPhone,
        OrderStatus OrderStatus,
        Guid ScannedTicketId,
        string ScannedTicketCode,
        List<TicketDetailDto> AllOrderTickets
    );
}
