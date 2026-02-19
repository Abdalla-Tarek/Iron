using Microsoft.AspNetCore.Mvc;
using PdfTool.Api.Models.Requests;
using PdfTool.Api.Utilities;
using PdfTool.Application.DTOs.Requests;
using PdfTool.Application.Interfaces;
using PdfTool.Application.Services;
using PdfTool.Domain.Common;
using PdfTool.Domain.Models;

namespace PdfTool.Api.Controllers;

/// <summary>PDF page operations.</summary>
[ApiController]
[Route("api/pdf/pages")]
public sealed class PdfPagesController : ControllerBase
{
    private readonly IPdfAppService _service;
    private readonly IFileTypeValidator _validator;

    public PdfPagesController(IPdfAppService service, IFileTypeValidator validator)
    {
        _service = service;
        _validator = validator;
    }

    /// <summary>Removes a page from an uploaded PDF.</summary>
    [HttpPost("remove")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> RemoveForm([FromForm] RemovePageFormRequest request, CancellationToken ct)
    {
        var file = await FileMapping.FromFormFileAsync(request.PdfFile);
        if (!_validator.IsPdf(file.Bytes))
        {
            return BadRequest(ValidationHelper.InvalidFile<BinaryContent>("Only PDF files are allowed"));
        }

        var appRequest = new RemovePageRequest
        {
            PdfFile = file,
            PdfPassword = request.PdfPassword,
            PageNumber = request.PageNumber,
            ReturnBase64 = request.ReturnBase64
        };

        var result = await _service.RemovePageAsync(appRequest, ct);
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
