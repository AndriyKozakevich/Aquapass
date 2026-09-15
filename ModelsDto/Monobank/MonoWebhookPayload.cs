namespace AquaPass.ModelsDto.Monobank
{
    public class MonoWebhookPayload
    {
        public string InvoiceId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "success", "failure", "reversed", "expired"
        public int Amount { get; set; }
        public string Reference { get; set; } = string.Empty; // наш OrderId
        public string FailureReason { get; set; } = string.Empty;
    }
}