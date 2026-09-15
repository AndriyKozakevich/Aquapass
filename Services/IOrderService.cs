using AquaPass.ModelsDto;

namespace AquaPass.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto);
        Task<OrderResponseDto?> GetOrderByIdAsync(Guid id);
        Task<List<OrderResponseDto>> GetOrdersByPeriodAsync(DateTime fromDate, DateTime toDate);
    }
}
