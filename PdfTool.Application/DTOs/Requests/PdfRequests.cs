using PdfTool.Domain.Enums;

namespace PdfTool.Application.DTOs.Requests;

public sealed class FileInput
{
    public required byte[] Bytes { get; init; }
    public string FileName { get; init; } = "input";
    public string ContentType { get; init; } = "application/octet-stream";
}

public sealed class MarginOptions
{
    public int Top { get; init; } = 10;
    public int Right { get; init; } = 10;
    public int Bottom { get; init; } = 10;
    public int Left { get; init; } = 10;
}

public sealed class PdfPageOptions
{
    public string PageSize { get; init; } = "A4";
    public int Dpi { get; init; } = 300;
}

public sealed class Rect
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
}

public sealed class HtmlToPdfRequest
{
    public required string Html { get; init; }
    public string? BaseUrl { get; init; }
    public PdfPageOptions PageOptions { get; init; } = new();
    public MarginOptions Margins { get; init; } = new();
    public bool Landscape { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class WatermarkRequest
{
    public required FileInput PdfFile { get; init; }
    public string? PdfPassword { get; init; }
    public required string Text { get; init; }
    public float Opacity { get; init; } = 30;
    public int Rotation { get; init; } = 0;
    public string Position { get; init; } = "Center";
    public int FontSize { get; init; } = 36;
    public bool ReturnBase64 { get; init; }
}

public sealed class CompressRequest
{
    public required FileInput PdfFile { get; init; }
    public string? PdfPassword { get; init; }
    public CompressionProfile CompressionProfile { get; init; } = CompressionProfile.Balanced;
    public int? ImageQuality { get; init; }
    public int? Dpi { get; init; }
    public bool RemoveUnusedObjects { get; init; } = true;
    public bool ReturnBase64 { get; init; }
}

public sealed class EncryptRequest
{
    public required FileInput PdfFile { get; init; }
    public string? PdfPassword { get; init; }
    public required string OwnerPassword { get; init; }
    public string? UserPassword { get; init; }
    public PermissionFlags Permissions { get; init; } = PermissionFlags.All;
    public EncryptionStrength EncryptionStrength { get; init; } = EncryptionStrength.Aes256;
    public bool ReturnBase64 { get; init; }
}

public sealed class ConvertPdfARequest
{
    public required FileInput PdfFile { get; init; }
    public string? PdfPassword { get; init; }
    public PdfAType PdfAType { get; init; } = PdfAType.PDFA3b;
    public bool ReturnBase64 { get; init; }
}

public sealed class PdfToPngRequest
{
    public required FileInput PdfFile { get; init; }
    public string? PdfPassword { get; init; }
    public int Dpi { get; init; } = 200;
    public bool ZipOutput { get; init; } = true;
    public string? PageRange { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class RemovePageRequest
{
    public required FileInput PdfFile { get; init; }
    public string? PdfPassword { get; init; }
    public int PageNumber { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class MergeRequest
{
    public required FileInput Main { get; init; }
    public required FileInput Append { get; init; }
    public string? MainPassword { get; init; }
    public string? AppendPassword { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class ImageToPdfRequest
{
    public required FileInput ImageFile { get; init; }
    public string PageSize { get; init; } = "A4";
    public bool CompressAfter { get; init; } = true;
    public CompressionProfile CompressionProfile { get; init; } = CompressionProfile.ImageFocused;
    public int? ImageQuality { get; init; }
    public int? Dpi { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class SignPfxTextRequest
{
    public required FileInput PdfFile { get; init; }
    public string? PdfPassword { get; init; }
    public required FileInput PfxFile { get; init; }
    public required string PfxPassword { get; init; }
    public string Reason { get; init; } = "";
    public string Location { get; init; } = "";
    public string VisibleText { get; init; } = "Signed";
    public int PageNumber { get; init; } = 1;
    public Rect Rect { get; init; } = new();
    public bool ReturnBase64 { get; init; }
}

public sealed class SignPfxImageRequest
{
    public required FileInput PdfFile { get; init; }
    public string? PdfPassword { get; init; }
    public required FileInput PfxFile { get; init; }
    public required string PfxPassword { get; init; }
    public required FileInput ImageAppearanceFile { get; init; }
    public int PageNumber { get; init; } = 1;
    public Rect Rect { get; init; } = new();
    public string Reason { get; init; } = "";
    public string Location { get; init; } = "";
    public bool ReturnBase64 { get; init; }
}

public sealed class StampSignatureRequest
{
    public required FileInput PdfFile { get; init; }
    public string? PdfPassword { get; init; }
    public required FileInput SignatureImageFile { get; init; }
    public int PageNumber { get; init; } = 1;
    public float Scale { get; init; } = 0.3f;
    public double? X { get; init; }
    public double? Y { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class GenerateJsTestPdfRequest
{
    public required string JsCode { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class ScanPdfRequest
{
    public required FileInput PdfFile { get; init; }
    public string? PdfPassword { get; init; }
}

public sealed class SanitizePdfRequest
{
    public required FileInput PdfFile { get; init; }
    public string? PdfPassword { get; init; }
    public SanitizeMode SanitizeMode { get; init; } = SanitizeMode.RasterizeAllPages;
    public int Dpi { get; init; } = 200;
    public bool RemoveMetadata { get; init; } = true;
    public bool ReturnBase64 { get; init; }
}

public sealed class OcrExtractRequest
{
    public required FileInput File { get; init; }
    public string? PdfPassword { get; init; }
    public string[]? Keys { get; init; }
    public string Language { get; init; } = "en";
    public string KeySeparators { get; init; } = ":";
}
