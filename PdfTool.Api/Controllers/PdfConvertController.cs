using Microsoft.AspNetCore.Mvc;
using PdfTool.Api.Models.Requests;
using PdfTool.Api.Utilities;
using PdfTool.Application.DTOs.Requests;
using PdfTool.Application.Interfaces;
using PdfTool.Application.Services;
using PdfTool.Domain.Common;
using PdfTool.Domain.Models;

namespace PdfTool.Api.Controllers;

/// <summary>PDF conversion endpoints.</summary>
[ApiController]
[Route("api/pdf/convert")]
public sealed class PdfConvertController : ControllerBase
{
    private readonly IPdfAppService _service;
    private readonly IFileTypeValidator _validator;

    public PdfConvertController(IPdfAppService service, IFileTypeValidator validator)
    {
        _service = service;
        _validator = validator;
    }

    /// <summary>Converts an uploaded PDF to PDF/A.</summary>
    [HttpPost("pdfa")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ConvertForm([FromForm] ConvertPdfAFormRequest request, CancellationToken ct)
    {
        var file = await FileMapping.FromFormFileAsync(request.PdfFile);
        if (!_validator.IsPdf(file.Bytes))
        {
            return BadRequest(ValidationHelper.InvalidFile<BinaryContent>("Only PDF files are allowed"));
        }

        var appRequest = new ConvertPdfARequest
        {
            PdfFile = file,
            PdfAType = request.PdfAType,
            ReturnBase64 = request.ReturnBase64
        };

        var result = await _service.ConvertToPdfAAsync(appRequest, ct);
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
