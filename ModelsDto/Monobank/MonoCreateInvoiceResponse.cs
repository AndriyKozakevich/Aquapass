namespace AquaPass.ModelsDto.Monobank
{
    public class MonoCreateInvoiceResponse
    {
        public string InvoiceId { get; set; } = string.Empty;
        public string PageUrl { get; set; } = string.Empty;
        public string? ErrCode { get; set; }
        public string? ErrText { get; set; }
    }
}
