//using Microsoft.AspNetCore.Mvc;
//using PdfTool.Api.Models.Requests;
//using PdfTool.Api.Utilities;
//using PdfTool.Application.DTOs.Requests;
//using PdfTool.Application.Services;
//using PdfTool.Domain.Common;
//using PdfTool.Domain.Models;

//namespace PdfTool.Api.Controllers;

///// <summary>PDF creation endpoints.</summary>
//[ApiController]
//[Route("api/pdf/create")]
//public sealed class PdfCreateController : ControllerBase
//{
//    private readonly IPdfAppService _service;

//    public PdfCreateController(IPdfAppService service)
//    {
//        _service = service;
//    }

//    /// <summary>Creates a PDF from HTML provided as JSON.</summary>
//    [HttpPost("html/json")]
//    [Consumes("application/json")]
//    public async Task<IActionResult> HtmlToPdfJson([FromBody] HtmlToPdfJsonRequest request, CancellationToken ct)
//    {
//        var appRequest = new HtmlToPdfRequest
//        {
//            Html = request.Html,
//            BaseUrl = request.BaseUrl,
//            PageOptions = new PdfPageOptions { PageSize = request.PageSize, Dpi = request.Dpi },
//            Margins = new MarginOptions { Top = request.MarginTop, Right = request.MarginRight, Bottom = request.MarginBottom, Left = request.MarginLeft },
//            Landscape = request.Landscape,
//            ReturnBase64 = request.ReturnBase64
//        };

//        var result = await _service.HtmlToPdfAsync(appRequest, ct);
//        return BuildBinaryResponse(result, request.ReturnBase64);
//    }

//    /// <summary>Creates a PDF from HTML provided as form data.</summary>
//    [HttpPost("html/form")]
//    [Consumes("multipart/form-data")]
//    public async Task<IActionResult> HtmlToPdfForm([FromForm] HtmlToPdfFormRequest request, CancellationToken ct)
//    {
//        var appRequest = new HtmlToPdfRequest
//        {
//            Html = request.Html,
//            BaseUrl = request.BaseUrl,
//            PageOptions = new PdfPageOptions { PageSize = request.PageSize, Dpi = request.Dpi },
//            Margins = new MarginOptions { Top = request.MarginTop, Right = request.MarginRight, Bottom = request.MarginBottom, Left = request.MarginLeft },
//            Landscape = request.Landscape,
//            ReturnBase64 = request.ReturnBase64
//        };

//        var result = await _service.HtmlToPdfAsync(appRequest, ct);
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
