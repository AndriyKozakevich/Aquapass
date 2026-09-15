namespace AquaPass.ModelsDto
{
    public record TicketValidationResultDto(
        bool Success,
        string Message,
        string? TicketCode = null,
        string? GuestName = null,
        string? TariffName = null,
        string? SunbedInfo = null,
        string? OrderNumber = null
    );
}