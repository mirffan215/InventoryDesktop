using QRCoder;

namespace AMGH.ITInventory.QRCode;

public interface IQrService
{
    byte[] GenerateQrPng(string payload, int pixelsPerModule = 10);
    string BuildAssetPayload(string assetTag);
    bool TryParseAssetPayload(string scanned, out string assetTag);
}

public class QrService : IQrService
{
    public const string Prefix = "AMGH-ASSET:";

    public byte[] GenerateQrPng(string payload, int pixelsPerModule = 10)
    {
        if (string.IsNullOrWhiteSpace(payload)) throw new ArgumentException("Payload is required.", nameof(payload));
        using var gen = new QRCodeGenerator();
        using var data = gen.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        return new PngByteQRCode(data).GetGraphic(pixelsPerModule);
    }

    public string BuildAssetPayload(string assetTag) => Prefix + assetTag.Trim().ToUpperInvariant();

    /// <summary>Accepts our QR payload or a bare asset-tag barcode (Code128).</summary>
    public bool TryParseAssetPayload(string scanned, out string assetTag)
    {
        assetTag = "";
        if (string.IsNullOrWhiteSpace(scanned)) return false;
        var s = scanned.Trim();
        if (s.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)) s = s[Prefix.Length..];
        if (s.Length is < 3 or > 50 || !s.All(c => char.IsLetterOrDigit(c) || c is '-' or '_')) return false;
        assetTag = s.ToUpperInvariant();
        return true;
    }
}
