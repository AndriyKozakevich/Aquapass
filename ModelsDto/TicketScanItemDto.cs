namespace AquaPass.ModelsDto
{
    public record TicketScanItemDto(
        Guid Id,
        string TicketCode,
        string TariffName,
        string? SunbedInfo,
        decimal Price,
        string Status // "Created" | "Used"
    );
}