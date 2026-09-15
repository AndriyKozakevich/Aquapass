namespace AquaPass.Services
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(string toEmail, string customerName, string orderNumber, byte[] pdfBytes);
    }
}
