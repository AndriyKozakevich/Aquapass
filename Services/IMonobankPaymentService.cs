using AquaPass.ModelsDto.Monobank;

namespace AquaPass.Services
{
    public interface IMonobankPaymentService
    {
        Task<MonoCreateInvoiceResponse?> CreateInvoiceAsync(Guid orderId, decimal amount, string destination);
    }
}
