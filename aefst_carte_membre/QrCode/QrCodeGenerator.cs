using QRCoder;

namespace aefst_carte_membre.QrCode
{
    public static class QrCodeGenerator
    {
        public static byte[] Generate(string url)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(data);
            return qrCode.GetGraphic(10);
        }
    }
}
