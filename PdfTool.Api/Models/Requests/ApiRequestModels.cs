using Microsoft.AspNetCore.Http;
using PdfTool.Domain.Enums;

namespace PdfTool.Api.Models.Requests;

public sealed class Base64File
{
    public required string Base64 { get; init; }
    public string FileName { get; init; } = "input";
    public string ContentType { get; init; } = "application/octet-stream";
}

public class HtmlToPdfFormRequest
{
    public required string Html { get; init; }
    public string? BaseUrl { get; init; }
    public string PageSize { get; init; } = "A4";
    public int Dpi { get; init; } = 300;
    public int MarginTop { get; init; } = 10;
    public int MarginRight { get; init; } = 10;
    public int MarginBottom { get; init; } = 10;
    public int MarginLeft { get; init; } = 10;
    public bool Landscape { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class HtmlToPdfJsonRequest : HtmlToPdfFormRequest { }

public sealed class WatermarkFormRequest
{
    public required IFormFile PdfFile { get; init; }
    public required string Text { get; init; }
    public float Opacity { get; init; } = 30;
    public int Rotation { get; init; } = 0;
    public string Position { get; init; } = "Center";
    public int FontSize { get; init; } = 36;
    public bool ReturnBase64 { get; init; }
}

public sealed class WatermarkJsonRequest
{
    public required Base64File PdfFile { get; init; }
    public required string Text { get; init; }
    public float Opacity { get; init; } = 30;
    public int Rotation { get; init; } = 0;
    public string Position { get; init; } = "Center";
    public int FontSize { get; init; } = 36;
    public bool ReturnBase64 { get; init; }
}

public sealed class CompressFormRequest
{
    public required IFormFile PdfFile { get; init; }
    public CompressionProfile CompressionProfile { get; init; } = CompressionProfile.Balanced;
    public int? ImageQuality { get; init; }
    public int? Dpi { get; init; }
    public bool RemoveUnusedObjects { get; init; } = true;
    public bool ReturnBase64 { get; init; }
}

public sealed class CompressJsonRequest
{
    public required Base64File PdfFile { get; init; }
    public CompressionProfile CompressionProfile { get; init; } = CompressionProfile.Balanced;
    public int? ImageQuality { get; init; }
    public int? Dpi { get; init; }
    public bool RemoveUnusedObjects { get; init; } = true;
    public bool ReturnBase64 { get; init; }
}

public sealed class EncryptFormRequest
{
    public required IFormFile PdfFile { get; init; }
    public required string OwnerPassword { get; init; }
    public string? UserPassword { get; init; }
    public PermissionFlags Permissions { get; init; } = PermissionFlags.All;
    public EncryptionStrength EncryptionStrength { get; init; } = EncryptionStrength.Aes256;
    public bool ReturnBase64 { get; init; }
}

public sealed class EncryptJsonRequest
{
    public required Base64File PdfFile { get; init; }
    public required string OwnerPassword { get; init; }
    public string? UserPassword { get; init; }
    public PermissionFlags Permissions { get; init; } = PermissionFlags.All;
    public EncryptionStrength EncryptionStrength { get; init; } = EncryptionStrength.Aes256;
    public bool ReturnBase64 { get; init; }
}

public sealed class ConvertPdfAFormRequest
{
    public required IFormFile PdfFile { get; init; }
    public PdfAType PdfAType { get; init; } = PdfAType.PDFA3b;
    public bool ReturnBase64 { get; init; }
}

public sealed class ConvertPdfAJsonRequest
{
    public required Base64File PdfFile { get; init; }
    public PdfAType PdfAType { get; init; } = PdfAType.PDFA3b;
    public bool ReturnBase64 { get; init; }
}

public sealed class PdfToPngFormRequest
{
    public required IFormFile PdfFile { get; init; }
    public int Dpi { get; init; } = 200;
    public bool ZipOutput { get; init; } = true;
    public string? PageRange { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class PdfToPngJsonRequest
{
    public required Base64File PdfFile { get; init; }
    public int Dpi { get; init; } = 200;
    public bool ZipOutput { get; init; } = true;
    public string? PageRange { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class RemovePageFormRequest
{
    public required IFormFile PdfFile { get; init; }
    public int PageNumber { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class RemovePageJsonRequest
{
    public required Base64File PdfFile { get; init; }
    public int PageNumber { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class MergeFormRequest
{
    public required IFormFile Main { get; init; }
    public required IFormFile Append { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class MergeJsonRequest
{
    public required Base64File Main { get; init; }
    public required Base64File Append { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class ImageToPdfFormRequest
{
    public required IFormFile ImageFile { get; init; }
    public string PageSize { get; init; } = "A4";
    public bool CompressAfter { get; init; } = true;
    public CompressionProfile CompressionProfile { get; init; } = CompressionProfile.ImageFocused;
    public int? ImageQuality { get; init; }
    public int? Dpi { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class ImageToPdfJsonRequest
{
    public required Base64File ImageFile { get; init; }
    public string PageSize { get; init; } = "A4";
    public bool CompressAfter { get; init; } = true;
    public CompressionProfile CompressionProfile { get; init; } = CompressionProfile.ImageFocused;
    public int? ImageQuality { get; init; }
    public int? Dpi { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class SignPfxTextFormRequest
{
    public required IFormFile PdfFile { get; init; }
    public required IFormFile PfxFile { get; init; }
    public required string PfxPassword { get; init; }
    public string Reason { get; init; } = "";
    public string Location { get; init; } = "";
    public string VisibleText { get; init; } = "Signed";
    public int PageNumber { get; init; } = 1;
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class SignPfxTextJsonRequest
{
    public required Base64File PdfFile { get; init; }
    public required Base64File PfxFile { get; init; }
    public required string PfxPassword { get; init; }
    public string Reason { get; init; } = "";
    public string Location { get; init; } = "";
    public string VisibleText { get; init; } = "Signed";
    public int PageNumber { get; init; } = 1;
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class SignPfxImageFormRequest
{
    public required IFormFile PdfFile { get; init; }
    public required IFormFile PfxFile { get; init; }
    public required string PfxPassword { get; init; }
    public required IFormFile ImageAppearanceFile { get; init; }
    public int PageNumber { get; init; } = 1;
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
    public string Reason { get; init; } = "";
    public string Location { get; init; } = "";
    public bool ReturnBase64 { get; init; }
}

public sealed class SignPfxImageJsonRequest
{
    public required Base64File PdfFile { get; init; }
    public required Base64File PfxFile { get; init; }
    public required string PfxPassword { get; init; }
    public required Base64File ImageAppearanceFile { get; init; }
    public int PageNumber { get; init; } = 1;
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
    public string Reason { get; init; } = "";
    public string Location { get; init; } = "";
    public bool ReturnBase64 { get; init; }
}

public sealed class StampSignatureFormRequest
{
    public required IFormFile PdfFile { get; init; }
    public required IFormFile SignatureImageFile { get; init; }
    public int PageNumber { get; init; } = 1;
    public float Scale { get; init; } = 0.3f;
    public double? X { get; init; }
    public double? Y { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class StampSignatureJsonRequest
{
    public required Base64File PdfFile { get; init; }
    public required Base64File SignatureImageFile { get; init; }
    public int PageNumber { get; init; } = -1;
    public string Corner { get; init; } = "BottomRight";
    public int MarginPx { get; init; } = 24;
    public float Scale { get; init; } = 0.3f;
    public double? X { get; init; }
    public double? Y { get; init; }
    public double? Width { get; init; }
    public double? Height { get; init; }
    public bool ReturnBase64 { get; init; }
}

public class GenerateJsTestPdfFormRequest
{
    public required string JsCode { get; init; }
    public bool ReturnBase64 { get; init; }
}

public sealed class GenerateJsTestPdfJsonRequest : GenerateJsTestPdfFormRequest { }

public sealed class ScanPdfFormRequest
{
    public required IFormFile PdfFile { get; init; }
}

public sealed class ScanPdfJsonRequest
{
    public required Base64File PdfFile { get; init; }
}

public sealed class SanitizePdfFormRequest
{
    public required IFormFile PdfFile { get; init; }
    public SanitizeMode SanitizeMode { get; init; } = SanitizeMode.RasterizeAllPages;
    public int Dpi { get; init; } = 200;
    public bool RemoveMetadata { get; init; } = true;
    public bool ReturnBase64 { get; init; }
}

public sealed class SanitizePdfJsonRequest
{
    public required Base64File PdfFile { get; init; }
    public SanitizeMode SanitizeMode { get; init; } = SanitizeMode.RasterizeAllPages;
    public int Dpi { get; init; } = 200;
    public bool RemoveMetadata { get; init; } = true;
    public bool ReturnBase64 { get; init; }
}

public sealed class OcrExtractFormRequest: OcrExtractTextRequest
{
    public string[]? Keys { get; init; }
    public string Language { get; init; } = "en";
    public string KeySeparators { get; init; } = ":";
}
public class OcrExtractTextRequest
{
    public required IFormFile File { get; init; }

}

public sealed class OcrExtractJsonRequest
{
    public required Base64File File { get; init; }
}
