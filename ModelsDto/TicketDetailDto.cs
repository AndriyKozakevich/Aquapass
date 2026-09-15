namespace AquaPass.ModelsDto
{
    public record TicketDetailDto(
        Guid TicketId,
        string TicketCode,
        string EntranceType,
        decimal EntrancePrice,
        string Status, // "Active", "Used", "Cancelled"
        string? SunbedInfo,
        decimal TotalPrice
    );
}