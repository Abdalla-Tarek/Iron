using System;
using Microsoft.AspNetCore.Mvc;
using PdfTool.Api.Models.Requests;
using PdfTool.Api.Utilities;
using PdfTool.Application.DTOs.Requests;
using PdfTool.Application.Interfaces;
using PdfTool.Application.Services;
using PdfTool.Domain.Common;
using PdfTool.Domain.Models;

namespace PdfTool.Api.Controllers;

/// <summary>PDF compression endpoints.</summary>
[ApiController]
[Route("api/pdf/compress")]
public sealed class PdfCompressController : ControllerBase
{
    private readonly IPdfAppService _service;
    private readonly IFileTypeValidator _validator;

    public PdfCompressController(IPdfAppService service, IFileTypeValidator validator)
    {
        _service = service;
        _validator = validator;
    }

    /// <summary>Compresses an uploaded PDF and returns the result.</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CompressForm([FromForm] CompressFormRequest request, CancellationToken ct)
    {
        var file = await FileMapping.FromFormFileAsync(request.PdfFile);
        if (!_validator.IsPdf(file.Bytes))
        {
            return BadRequest(ValidationHelper.InvalidFile<CompressResult>("Only PDF files are allowed"));
        }

        var appRequest = new CompressRequest
        {
            PdfFile = file,
            PdfPassword = request.PdfPassword,
            CompressionProfile = request.CompressionProfile,
            //ImageQuality = request.ImageQuality,
            //Dpi = request.Dpi,
            //RemoveUnusedObjects = request.RemoveUnusedObjects,
            ReturnBase64 = request.ReturnBase64
        };

        var result = await _service.CompressAsync(appRequest, ct);
        if (!result.Success || result.Data == null)
        {
            return BadRequest(result);
        }
        if (result.Data?.Pdf != null)
        {
            result.Data.Pdf.Metadata["returnBase64"] = request.ReturnBase64;
        }

        if (request.ReturnBase64)
        {
            var base64 = Convert.ToBase64String(result.Data!.Pdf.Bytes);
            return Ok(OperationResult<object>.Ok(new
            {
                base64,
                contentType = result.Data.Pdf.ContentType,
                fileName = result.Data.Pdf.FileName,
                originalSize = result.Data.OriginalSize,
                finalSize = result.Data.FinalSize,
                ratio = result.Data.Ratio
            }));
        }

        return new FileContentResult(result.Data!.Pdf.Bytes, "application/pdf") { FileDownloadName = result.Data.Pdf.FileName };
    }

}
