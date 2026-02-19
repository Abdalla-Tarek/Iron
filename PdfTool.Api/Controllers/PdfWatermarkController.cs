//using Microsoft.AspNetCore.Mvc;
//using PdfTool.Api.Models.Requests;
//using PdfTool.Api.Utilities;
//using PdfTool.Application.DTOs.Requests;
//using PdfTool.Application.Interfaces;
//using PdfTool.Application.Services;
//using PdfTool.Domain.Common;
//using PdfTool.Domain.Models;

//namespace PdfTool.Api.Controllers;

///// <summary>PDF watermarking endpoints.</summary>
//[ApiController]
//[Route("api/pdf/watermark")]
//public sealed class PdfWatermarkController : ControllerBase
//{
//    private readonly IPdfAppService _service;
//    private readonly IFileTypeValidator _validator;

//    public PdfWatermarkController(IPdfAppService service, IFileTypeValidator validator)
//    {
//        _service = service;
//        _validator = validator;
//    }

//    /// <summary>Adds a text watermark to an uploaded PDF.</summary>
//    [HttpPost]
//    [Consumes("multipart/form-data")]
//    public async Task<IActionResult> WatermarkForm([FromForm] WatermarkFormRequest request, CancellationToken ct)
//    {
//        var file = await FileMapping.FromFormFileAsync(request.PdfFile);
//        if (!_validator.IsPdf(file.Bytes))
//        {
//            return BadRequest(ValidationHelper.InvalidFile<BinaryContent>("Only PDF files are allowed"));
//        }

//        var appRequest = new WatermarkRequest
//        {
//            PdfFile = file,
//            Text = request.Text,
//            Opacity = request.Opacity,
//            Rotation = request.Rotation,
//            Position = request.Position,
//            FontSize = request.FontSize,
//            ReturnBase64 = request.ReturnBase64
//        };

//        var result = await _service.WatermarkAsync(appRequest, ct);
//        return BuildBinaryResponse(result, request.ReturnBase64);
//    }

//    private static IActionResult BuildBinaryResponse(OperationResult<BinaryContent> result, bool returnBase64)
//    {
//        if (result.Data != null)
//        {
//            result.Data.Metadata["returnBase64"] = returnBase64;
//        }
//        return ResponseHelper.FileOrBase64(result);
//    }
//}
