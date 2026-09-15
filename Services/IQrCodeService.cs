namespace AquaPass.Services
{
    public interface IQrCodeService
    {
        byte[] GeneratePngQrCode(string payload);
    }
}
