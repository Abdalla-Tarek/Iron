using Microsoft.AspNetCore.Mvc;
using PdfTool.Api.Models.Requests;
using PdfTool.Api.Utilities;
using PdfTool.Application.DTOs.Requests;
using PdfTool.Application.Interfaces;
using PdfTool.Application.Services;
using PdfTool.Domain.Common;
using PdfTool.Domain.Models;

namespace PdfTool.Api.Controllers;

/// <summary>PDF merge endpoints.</summary>
[ApiController]
[Route("api/pdf/merge")]
public sealed class PdfMergeController : ControllerBase
{
    private readonly IPdfAppService _service;
    private readonly IFileTypeValidator _validator;

    public PdfMergeController(IPdfAppService service, IFileTypeValidator validator)
    {
        _service = service;
        _validator = validator;
    }

    /// <summary>Merges two uploaded PDFs into one.</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> MergeForm([FromForm] MergeFormRequest request, CancellationToken ct)
    {
        var main = await FileMapping.FromFormFileAsync(request.Main);
        var append = await FileMapping.FromFormFileAsync(request.Append);
        if (!_validator.IsPdf(main.Bytes) || !_validator.IsPdf(append.Bytes))
        {
            return BadRequest(ValidationHelper.InvalidFile<BinaryContent>("Only PDF files are allowed"));
        }

        var appRequest = new MergeRequest
        {
            Main = main,
            Append = append,
            MainPassword = request.MainPassword,
            AppendPassword = request.AppendPassword,
            ReturnBase64 = request.ReturnBase64
        };

        var result = await _service.MergeAsync(appRequest, ct);
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
