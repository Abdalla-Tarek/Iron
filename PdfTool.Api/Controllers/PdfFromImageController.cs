using Microsoft.AspNetCore.Mvc;
using PdfTool.Api.Models.Requests;
using PdfTool.Api.Utilities;
using PdfTool.Application.DTOs.Requests;
using PdfTool.Application.Interfaces;
using PdfTool.Application.Services;
using PdfTool.Domain.Common;
using PdfTool.Domain.Models;

namespace PdfTool.Api.Controllers;

/// <summary>Image-to-PDF endpoints.</summary>
[ApiController]
[Route("api/pdf/from-image")]
public sealed class PdfFromImageController : ControllerBase
{
    private readonly IPdfAppService _service;
    private readonly IFileTypeValidator _validator;

    public PdfFromImageController(IPdfAppService service, IFileTypeValidator validator)
    {
        _service = service;
        _validator = validator;
    }

    /// <summary>Creates a PDF from an uploaded image.</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImageForm([FromForm] ImageToPdfFormRequest request, CancellationToken ct)
    {
        var image = await FileMapping.FromFormFileAsync(request.ImageFile);
        if (!_validator.IsImage(image.Bytes))
        {
            return BadRequest(ValidationHelper.InvalidFile<BinaryContent>("Only image files are allowed"));
        }

        var appRequest = new ImageToPdfRequest
        {
            ImageFile = image,
            PageSize = request.PageSize,
            CompressAfter = request.CompressAfter,
            CompressionProfile = request.CompressionProfile,
            ImageQuality = request.ImageQuality,
            Dpi = request.Dpi,
            ReturnBase64 = request.ReturnBase64
        };

        var result = await _service.ImageToPdfAsync(appRequest, ct);
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
