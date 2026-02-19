using PdfTool.Application.DTOs.Requests;
using PdfTool.Domain.Common;
using PdfTool.Domain.Models;
using System.Threading;
using System.Threading.Tasks;

namespace PdfTool.Application.Services;

public interface IPdfAppService
{
    Task<OperationResult<BinaryContent>> HtmlToPdfAsync(HtmlToPdfRequest request, CancellationToken ct);
    Task<OperationResult<BinaryContent>> WatermarkAsync(WatermarkRequest request, CancellationToken ct);
    Task<OperationResult<CompressResult>> CompressAsync(CompressRequest request, CancellationToken ct);
    Task<OperationResult<BinaryContent>> EncryptAsync(EncryptRequest request, CancellationToken ct);
    Task<OperationResult<BinaryContent>> ConvertToPdfAAsync(ConvertPdfARequest request, CancellationToken ct);
    Task<OperationResult<PageImagesResult>> PdfToPngAsync(PdfToPngRequest request, CancellationToken ct);
    Task<OperationResult<BinaryContent>> RemovePageAsync(RemovePageRequest request, CancellationToken ct);
    Task<OperationResult<BinaryContent>> MergeAsync(MergeRequest request, CancellationToken ct);
    Task<OperationResult<BinaryContent>> ImageToPdfAsync(ImageToPdfRequest request, CancellationToken ct);
    Task<OperationResult<BinaryContent>> SignPfxTextAsync(SignPfxTextRequest request, CancellationToken ct);
    Task<OperationResult<BinaryContent>> SignPfxImageAsync(SignPfxImageRequest request, CancellationToken ct);
    Task<OperationResult<BinaryContent>> StampSignatureAsync(StampSignatureRequest request, CancellationToken ct);
}

public interface IPdfScanAppService
{
    Task<OperationResult<PdfScanReport>> ScanAsync(ScanPdfRequest request, CancellationToken ct);
    Task<OperationResult<BinaryContent>> GenerateJsTestPdfAsync(GenerateJsTestPdfRequest request, CancellationToken ct);
    Task<OperationResult<(BinaryContent Pdf, PdfScanReport Before, PdfScanReport After)>> SanitizeAsync(SanitizePdfRequest request, CancellationToken ct);
}

public interface IOcrAppService
{
    Task<OperationResult<OcrResultModel>> ExtractAsync(OcrExtractRequest request, CancellationToken ct);
    Task<string> ExtractTextAsync(OcrExtractRequest request, CancellationToken ct);
}
