using QRCoder;

namespace AquaPass.Services
{
    public class QrCodeService : IQrCodeService
    {
        public byte[] GeneratePngQrCode(string payload)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);

            return qrCode.GetGraphic(20);
        }
    }
}
