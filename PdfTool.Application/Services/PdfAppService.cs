using System;
using System.Threading;
using System.Threading.Tasks;
using PdfTool.Application.DTOs.Requests;
using PdfTool.Application.Interfaces;
using PdfTool.Domain.Common;
using PdfTool.Domain.Models;

namespace PdfTool.Application.Services;

public sealed class PdfAppService : IPdfAppService
{
    private readonly IPdfCreator _creator;
    private readonly IPdfWatermarker _watermarker;
    private readonly IPdfCompressor _compressor;
    private readonly IPdfEncryptor _encryptor;
    private readonly IPdfConverter _converter;
    private readonly IPdfEditor _editor;
    private readonly IPdfMerger _merger;
    private readonly IPdfSigner _signer;

    public PdfAppService(
        IPdfCreator creator,
        IPdfWatermarker watermarker,
        IPdfCompressor compressor,
        IPdfEncryptor encryptor,
        IPdfConverter converter,
        IPdfEditor editor,
        IPdfMerger merger,
        IPdfSigner signer)
    {
        _creator = creator;
        _watermarker = watermarker;
        _compressor = compressor;
        _encryptor = encryptor;
        _converter = converter;
        _editor = editor;
        _merger = merger;
        _signer = signer;
    }

    public async Task<OperationResult<BinaryContent>> HtmlToPdfAsync(HtmlToPdfRequest request, CancellationToken ct)
        => await WrapAsync(() => _creator.HtmlToPdfAsync(request, ct));

    public async Task<OperationResult<BinaryContent>> WatermarkAsync(WatermarkRequest request, CancellationToken ct)
        => await WrapAsync(() => _watermarker.AddWatermarkAsync(request, ct));

    public async Task<OperationResult<CompressResult>> CompressAsync(CompressRequest request, CancellationToken ct)
        => await WrapAsync(() => _compressor.CompressAsync(request, ct));

    public async Task<OperationResult<BinaryContent>> EncryptAsync(EncryptRequest request, CancellationToken ct)
        => await WrapAsync(() => _encryptor.EncryptAsync(request, ct));

    public async Task<OperationResult<BinaryContent>> ConvertToPdfAAsync(ConvertPdfARequest request, CancellationToken ct)
        => await WrapAsync(() => _converter.ConvertToPdfAAsync(request, ct));

    public async Task<OperationResult<PageImagesResult>> PdfToPngAsync(PdfToPngRequest request, CancellationToken ct)
        => await WrapAsync(() => _converter.PdfToPngAsync(request, ct));

    public async Task<OperationResult<BinaryContent>> RemovePageAsync(RemovePageRequest request, CancellationToken ct)
        => await WrapAsync(() => _editor.RemovePageAsync(request, ct));

    public async Task<OperationResult<BinaryContent>> MergeAsync(MergeRequest request, CancellationToken ct)
        => await WrapAsync(() => _merger.MergeAsync(request, ct));

    public async Task<OperationResult<BinaryContent>> ImageToPdfAsync(ImageToPdfRequest request, CancellationToken ct)
        => await WrapAsync(() => _converter.ImageToPdfAsync(request, ct));

    public async Task<OperationResult<BinaryContent>> SignPfxTextAsync(SignPfxTextRequest request, CancellationToken ct)
        => await WrapAsync(() => _signer.SignPfxTextAsync(request, ct));

    public async Task<OperationResult<BinaryContent>> SignPfxImageAsync(SignPfxImageRequest request, CancellationToken ct)
        => await WrapAsync(() => _signer.SignPfxImageAsync(request, ct));

    public async Task<OperationResult<BinaryContent>> StampSignatureAsync(StampSignatureRequest request, CancellationToken ct)
        => await WrapAsync(() => _signer.StampSignatureAsync(request, ct));

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
