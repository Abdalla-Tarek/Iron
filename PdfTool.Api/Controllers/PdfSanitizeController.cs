//using System;
//using Microsoft.AspNetCore.Mvc;
//using PdfTool.Api.Models.Requests;
//using PdfTool.Api.Utilities;
//using PdfTool.Application.DTOs.Requests;
//using PdfTool.Application.Interfaces;
//using PdfTool.Application.Services;
//using PdfTool.Domain.Common;
//using PdfTool.Domain.Models;

//namespace PdfTool.Api.Controllers;

///// <summary>PDF sanitization endpoints.</summary>
//[ApiController]
//[Route("api/pdf/sanitize")]
//public sealed class PdfSanitizeController : ControllerBase
//{
//    private readonly IPdfScanAppService _service;
//    private readonly IFileTypeValidator _validator;

//    public PdfSanitizeController(IPdfScanAppService service, IFileTypeValidator validator)
//    {
//        _service = service;
//        _validator = validator;
//    }

//    /// <summary>Sanitizes an uploaded PDF and returns a report.</summary>
//    [HttpPost]
//    [Consumes("multipart/form-data")]
//    public async Task<IActionResult> SanitizeForm([FromForm] SanitizePdfFormRequest request, CancellationToken ct)
//    {
//        var file = await FileMapping.FromFormFileAsync(request.PdfFile);
//        if (!_validator.IsPdf(file.Bytes))
//        {
//            return BadRequest(ValidationHelper.InvalidFile<PdfSanitizeReport>("Only PDF files are allowed"));
//        }

//        var result = await _service.SanitizeAsync(new SanitizePdfRequest
//        {
//            PdfFile = file,
//            SanitizeMode = request.SanitizeMode,
//            Dpi = request.Dpi,
//            RemoveMetadata = request.RemoveMetadata,
//            ReturnBase64 = request.ReturnBase64
//        }, ct);
//        if (!result.Success)
//        {
//            return BadRequest(result);
//        }

//        result.Data.Pdf.Metadata["returnBase64"] = request.ReturnBase64;

//        if (request.ReturnBase64)
//        {
//            var base64 = Convert.ToBase64String(result.Data.Pdf.Bytes);
//            return Ok(OperationResult<object>.Ok(new
//            {
//                base64,
//                contentType = result.Data.Pdf.ContentType,
//                fileName = result.Data.Pdf.FileName,
//                before = result.Data.Before,
//                after = result.Data.After
//            }));
//        }

//        return new FileContentResult(result.Data.Pdf.Bytes, "application/pdf") { FileDownloadName = result.Data.Pdf.FileName };
//    }

//}
