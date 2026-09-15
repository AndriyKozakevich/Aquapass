using AquaPass.ModelsDto;

public interface ITicketService
{
    Task<TicketValidationResultDto> ValidateTicketAsync(string ticketCode);
    Task<OrderValidationDto?> GetOrderByTicketCodeAsync(string ticketCode);
    Task<BulkValidationResultDto> ValidateAllTicketsInOrderAsync(Guid orderId);
    Task<byte[]?> GetTicketQrCodeAsync(string ticketCode);
    Task<TicketResponseDto?> GetTicketByCodeAsync(string ticketCode);
}