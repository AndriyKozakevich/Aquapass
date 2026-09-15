using MimeKit;
using MailKit.Net.Smtp;

namespace AquaPass.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOrderConfirmationAsync(string toEmail, string customerName, string orderNumber, byte[] pdfBytes)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["EmailSettings:SenderName"] ?? "AquaPass",
                _config["EmailSettings:SenderEmail"]
            ));
            message.To.Add(new MailboxAddress(customerName, toEmail));
            message.Subject = $"Ваші квитки в AquaPass — Замовлення #{orderNumber}";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; color: #1e293b; max-width: 600px;'>
                    <h2 style='color: #0284c7;'>Вітаємо, {customerName}!</h2>
                    <p>Ваше замовлення <b>#{orderNumber}</b> успішно підтверджено та оплачено.</p>
                    <p>Електронні квитки з QR-кодами для проходу через турнікет прикріплені до цього листа у форматі PDF.</p>
                    <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;'/>
                    <p style='font-size: 12px; color: #64748b;'>Збережіть цей файл або відкрийте його на вході до комплексу.</p>
                </div>"
            };

            // Додаємо згенерований PDF як вкладення
            bodyBuilder.Attachments.Add($"AquaPass_Order_{orderNumber}.pdf", pdfBytes, new ContentType("application", "pdf"));
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["EmailSettings:SmtpServer"],
                int.Parse(_config["EmailSettings:Port"] ?? "587"),
                MailKit.Security.SecureSocketOptions.StartTls
            );

            await client.AuthenticateAsync(
                _config["EmailSettings:Username"],
                _config["EmailSettings:Password"]
            );

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}