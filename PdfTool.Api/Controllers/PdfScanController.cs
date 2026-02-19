using Microsoft.AspNetCore.Mvc;
using PdfTool.Api.Models.Requests;
using PdfTool.Api.Utilities;
using PdfTool.Application.DTOs.Requests;
using PdfTool.Application.Interfaces;
using PdfTool.Application.Services;
using PdfTool.Domain.Models;

namespace PdfTool.Api.Controllers;

/// <summary>PDF scanning and test generation endpoints.</summary>
[ApiController]
[Route("api/pdf/scan")]
public sealed class PdfScanController : ControllerBase
{
    private readonly IPdfScanAppService _service;
    private readonly IFileTypeValidator _validator;

    public PdfScanController(IPdfScanAppService service, IFileTypeValidator validator)
    {
        _service = service;
        _validator = validator;
    }

    /// <summary>Scans an uploaded PDF for risks and returns a report.</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ScanForm([FromForm] ScanPdfFormRequest request, CancellationToken ct)
    {
        var file = await FileMapping.FromFormFileAsync(request.PdfFile);
        if (!_validator.IsPdf(file.Bytes))
        {
            return BadRequest(ValidationHelper.InvalidFile<PdfScanReport>("Only PDF files are allowed"));
        }

        var result = await _service.ScanAsync(new ScanPdfRequest { PdfFile = file, PdfPassword = request.PdfPassword }, ct);
        return Ok(result);
    }

    /// <summary>Generates a JavaScript test PDF from form data.</summary>
    //[HttpPost("generate-js/form")]
    //[Consumes("multipart/form-data")]
    //public async Task<IActionResult> GenerateJsForm([FromForm] GenerateJsTestPdfFormRequest request, CancellationToken ct)
    //{
    //    var result = await _service.GenerateJsTestPdfAsync(new GenerateJsTestPdfRequest { JsCode = request.JsCode, ReturnBase64 = request.ReturnBase64 }, ct);
    //    if (result.Data != null)
    //    {
    //        result.Data.Metadata["returnBase64"] = request.ReturnBase64;
    //    }
    //    return ResponseHelper.FileOrBase64(result);
    //}

    /// <summary>Generates a JavaScript test PDF from JSON.</summary>
    //[HttpPost("generate-js/json")]
    //[Consumes("application/json")]
    //public async Task<IActionResult> GenerateJsJson([FromBody] GenerateJsTestPdfJsonRequest request, CancellationToken ct)
    //{
    //    var result = await _service.GenerateJsTestPdfAsync(new GenerateJsTestPdfRequest { JsCode = request.JsCode, ReturnBase64 = request.ReturnBase64 }, ct);
    //    if (result.Data != null)
    //    {
    //        result.Data.Metadata["returnBase64"] = request.ReturnBase64;
    //    }
    //    return ResponseHelper.FileOrBase64(result);
    //}
}
