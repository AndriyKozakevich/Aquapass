using AquaPass.Models;

namespace AquaPass.Services
{
    public interface ITicketPdfGenerator
    {
        byte[] GenerateOrderTicketsPdf(Order order);
    }
}
