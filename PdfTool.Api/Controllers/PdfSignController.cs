using Microsoft.AspNetCore.Mvc;
using PdfTool.Api.Models.Requests;
using PdfTool.Api.Utilities;
using PdfTool.Application.DTOs.Requests;
using PdfTool.Application.Interfaces;
using PdfTool.Application.Services;
using PdfTool.Domain.Common;
using PdfTool.Domain.Models;

namespace PdfTool.Api.Controllers;

/// <summary>PDF signing endpoints.</summary>
[ApiController]
[Route("api/pdf/sign")]
public sealed class PdfSignController : ControllerBase
{
    private readonly IPdfAppService _service;
    private readonly IFileTypeValidator _validator;

    public PdfSignController(IPdfAppService service, IFileTypeValidator validator)
    {
        _service = service;
        _validator = validator;
    }

    /// <summary>Signs an uploaded PDF using a PFX certificate and visible text.</summary>
    [HttpPost("pfx-text")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SignPfxTextForm([FromForm] SignPfxTextFormRequest request, CancellationToken ct)
    {
        var pdf = await FileMapping.FromFormFileAsync(request.PdfFile);
        var pfx = await FileMapping.FromFormFileAsync(request.PfxFile);
        if (!_validator.IsPdf(pdf.Bytes))
        {
            return BadRequest(ValidationHelper.InvalidFile<BinaryContent>("Only PDF files are allowed"));
        }

        var appRequest = new SignPfxTextRequest
        {
            PdfFile = pdf,
            PfxFile = pfx,
            PfxPassword = request.PfxPassword,
            Reason = request.Reason,
            Location = request.Location,
            VisibleText = request.VisibleText,
            PageNumber = request.PageNumber,
            Rect = new Rect { X = request.X, Y = request.Y, Width = request.Width, Height = request.Height },
            ReturnBase64 = request.ReturnBase64
        };

        var result = await _service.SignPfxTextAsync(appRequest, ct);
        return BuildBinaryResponse(result, request.ReturnBase64);
    }

    /// <summary>Signs an uploaded PDF using a PFX certificate and image appearance.</summary>
    [HttpPost("pfx-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SignPfxImageForm([FromForm] SignPfxImageFormRequest request, CancellationToken ct)
    {
        var pdf = await FileMapping.FromFormFileAsync(request.PdfFile);
        var pfx = await FileMapping.FromFormFileAsync(request.PfxFile);
        var image = await FileMapping.FromFormFileAsync(request.ImageAppearanceFile);
        if (!_validator.IsPdf(pdf.Bytes))
        {
            return BadRequest(ValidationHelper.InvalidFile<BinaryContent>("Only PDF files are allowed"));
        }

        var appRequest = new SignPfxImageRequest
        {
            PdfFile = pdf,
            PfxFile = pfx,
            PfxPassword = request.PfxPassword,
            ImageAppearanceFile = image,
            PageNumber = request.PageNumber,
            Rect = new Rect { X = request.X, Y = request.Y, Width = request.Width, Height = request.Height },
            Reason = request.Reason,
            Location = request.Location,
            ReturnBase64 = request.ReturnBase64
        };

        var result = await _service.SignPfxImageAsync(appRequest, ct);
        return BuildBinaryResponse(result, request.ReturnBase64);
    }

    /// <summary>Applies a stamp signature image to an uploaded PDF.</summary>
    [HttpPost("stamp")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> StampForm([FromForm] StampSignatureFormRequest request, CancellationToken ct)
    {
        var pdf = await FileMapping.FromFormFileAsync(request.PdfFile);
        var image = await FileMapping.FromFormFileAsync(request.SignatureImageFile);
        if (!_validator.IsPdf(pdf.Bytes))
        {
            return BadRequest(ValidationHelper.InvalidFile<BinaryContent>("Only PDF files are allowed"));
        }

        var appRequest = new StampSignatureRequest
        {
            PdfFile = pdf,
            SignatureImageFile = image,
            PageNumber = request.PageNumber,
            Scale = request.Scale,
            X = request.X,
            Y = request.Y,
            ReturnBase64 = request.ReturnBase64
        };

        var result = await _service.StampSignatureAsync(appRequest, ct);
        return BuildBinaryResponse(result, request.ReturnBase64);
    }

    private static IActionResult BuildBinaryResponse(OperationResult<BinaryContent> result, bool returnBase64)
    {
        if (result.Data != null)
        {
            result.Data.Metadata["returnBase64"] = returnBase64;
        }
        return ResponseHelper.FileOrBase64(result);
    }
}
