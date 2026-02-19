using System;
using System.Threading;
using System.Threading.Tasks;
using PdfTool.Application.DTOs.Requests;
using PdfTool.Application.Interfaces;
using PdfTool.Domain.Common;
using PdfTool.Domain.Models;

namespace PdfTool.Application.Services;

public sealed class PdfScanAppService : IPdfScanAppService
{
    private readonly IPdfScanner _scanner;
    private readonly IPdfSanitizer _sanitizer;

    public PdfScanAppService(IPdfScanner scanner, IPdfSanitizer sanitizer)
    {
        _scanner = scanner;
        _sanitizer = sanitizer;
    }

    public async Task<OperationResult<PdfScanReport>> ScanAsync(ScanPdfRequest request, CancellationToken ct)
        => await WrapAsync(() => _scanner.ScanAsync(request, ct));

    public async Task<OperationResult<BinaryContent>> GenerateJsTestPdfAsync(GenerateJsTestPdfRequest request, CancellationToken ct)
        => await WrapAsync(() => _scanner.GenerateJsTestPdfAsync(request, ct));

    public async Task<OperationResult<(BinaryContent Pdf, PdfScanReport Before, PdfScanReport After)>> SanitizeAsync(SanitizePdfRequest request, CancellationToken ct)
        => await WrapAsync(() => _sanitizer.SanitizeAsync(request, ct));

    private static async Task<OperationResult<T>> WrapAsync<T>(Func<Task<T>> action)
    {
        try
        {
            var result = await action();
            return OperationResult<T>.Ok(result);
        }
        catch (Exception ex)
        {
            return OperationResult<T>.Fail(new OperationError("Unhandled", ex.Message));
        }
    }
}

public sealed class OcrAppService : IOcrAppService
{
    private readonly IOcrService _ocr;

    public OcrAppService(IOcrService ocr)
    {
        _ocr = ocr;
    }

    public async Task<OperationResult<OcrResultModel>> ExtractAsync(OcrExtractRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _ocr.ExtractAsync(request, ct);
            return OperationResult<OcrResultModel>.Ok(result);
        }
        catch (Exception ex)
        {
            return OperationResult<OcrResultModel>.Fail(new OperationError("Unhandled", ex.Message));
        }
    }

    public async Task<string> ExtractTextAsync(OcrExtractRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _ocr.ExtractTextAsync(request, ct);
            return result;
        }
        catch (Exception ex)
        {
            return "";
        }
    }
}
