using Microsoft.AspNetCore.Mvc;
using PdfTool.Api.Models.Requests;
using PdfTool.Api.Utilities;
using PdfTool.Application.DTOs.Requests;
using PdfTool.Application.Interfaces;
using PdfTool.Application.Services;
using PdfTool.Domain.Models;

namespace PdfTool.Api.Controllers;

/// <summary>OCR extraction endpoints for PDFs and images.</summary>
[ApiController]
[Route("api/ocr")]
public sealed class OcrController : ControllerBase
{
    private readonly IOcrAppService _service;
    private readonly IFileTypeValidator _validator;

    public OcrController(IOcrAppService service, IFileTypeValidator validator)
    {
        _service = service;
        _validator = validator;
    }

    /// <summary>Extracts key/value fields from an uploaded PDF or image.</summary>
    [HttpPost("extract")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ExtractForm([FromForm] OcrExtractFormRequest request, CancellationToken ct)
    {
        var file = await FileMapping.FromFormFileAsync(request.File);
        if (!_validator.IsPdf(file.Bytes) && !_validator.IsImage(file.Bytes))
        {
            return BadRequest(ValidationHelper.InvalidFile<OcrResultModel>("Only PDF or image files are allowed"));
        }

        var result = await _service.ExtractAsync(new OcrExtractRequest
        {
            File = file,
            Keys = request.Keys,
            Language = request.Language,
            KeySeparators = request.KeySeparators
        }, ct);
        return Ok(result);
    }

    /// <summary>Extracts plain text from an uploaded PDF or image.</summary>
    [HttpPost("text")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ExtractText([FromForm] OcrExtractTextRequest request, CancellationToken ct)
    {
        var file = await FileMapping.FromFormFileAsync(request.File);
        if (!_validator.IsPdf(file.Bytes) && !_validator.IsImage(file.Bytes))
        {
            return BadRequest(ValidationHelper.InvalidFile<string>("Only PDF or image files are allowed"));
        }

        var result = await _service.ExtractTextAsync(new OcrExtractRequest { File = file }, ct);
        return Ok(result);
    }

}
