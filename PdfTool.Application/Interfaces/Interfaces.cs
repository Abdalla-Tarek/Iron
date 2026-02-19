using PdfTool.Domain.Models;
using PdfTool.Domain.Enums;
using PdfTool.Application.DTOs.Requests;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PdfTool.Application.Interfaces;

public interface IPdfCreator
{
    Task<BinaryContent> HtmlToPdfAsync(HtmlToPdfRequest request, CancellationToken ct);
}

public interface IPdfWatermarker
{
    Task<BinaryContent> AddWatermarkAsync(WatermarkRequest request, CancellationToken ct);
}

public interface IPdfCompressor
{
    Task<CompressResult> CompressAsync(CompressRequest request, CancellationToken ct);
}

public interface IPdfEncryptor
{
    Task<BinaryContent> EncryptAsync(EncryptRequest request, CancellationToken ct);
}

public interface IPdfConverter
{
    Task<BinaryContent> ConvertToPdfAAsync(ConvertPdfARequest request, CancellationToken ct);
    Task<PageImagesResult> PdfToPngAsync(PdfToPngRequest request, CancellationToken ct);
    Task<BinaryContent> ImageToPdfAsync(ImageToPdfRequest request, CancellationToken ct);
}

public interface IPdfEditor
{
    Task<BinaryContent> RemovePageAsync(RemovePageRequest request, CancellationToken ct);
}

public interface IPdfMerger
{
    Task<BinaryContent> MergeAsync(MergeRequest request, CancellationToken ct);
}

public interface IPdfSigner
{
    Task<BinaryContent> SignPfxTextAsync(SignPfxTextRequest request, CancellationToken ct);
    Task<BinaryContent> SignPfxImageAsync(SignPfxImageRequest request, CancellationToken ct);
    Task<BinaryContent> StampSignatureAsync(StampSignatureRequest request, CancellationToken ct);
}

public interface IPdfScanner
{
    Task<PdfScanReport> ScanAsync(ScanPdfRequest request, CancellationToken ct);
    Task<BinaryContent> GenerateJsTestPdfAsync(GenerateJsTestPdfRequest request, CancellationToken ct);
}

public interface IPdfSanitizer
{
    Task<(BinaryContent Pdf, PdfScanReport Before, PdfScanReport After)> SanitizeAsync(SanitizePdfRequest request, CancellationToken ct);
}

public interface IOcrService
{
    Task<OcrResultModel> ExtractAsync(OcrExtractRequest request, CancellationToken ct);
    Task<string> ExtractTextAsync(OcrExtractRequest request, CancellationToken ct);
}

public interface IFileStorage
{
    Task<string> SaveAsync(string fileName, byte[] bytes, CancellationToken ct);
    Task<byte[]?> GetAsync(string key, CancellationToken ct);
}

public interface IFileTypeValidator
{
    bool IsPdf(byte[] bytes);
    bool IsImage(byte[] bytes);
}

public interface IOcrFieldExtractor
{
    List<OcrField> ExtractFields(string rawText);
}
