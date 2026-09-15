namespace AquaPass.ModelsDto
{
    public record OrderResponseDto(
        Guid OrderId,
        string OrderNumber,
        DateTime VisitDate,
        string CustomerFirstName,
        string CustomerLastName,
        string CustomerEmail,
        string CustomerPhone,
        decimal TotalAmount,
        string Status,
        DateTime CreatedAt,
        List<TicketResponseDto> Tickets
    );
}