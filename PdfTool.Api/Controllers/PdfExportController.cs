using Microsoft.AspNetCore.Mvc;
using PdfTool.Api.Models.Requests;
using PdfTool.Api.Models.Responses;
using PdfTool.Api.Utilities;
using PdfTool.Application.DTOs.Requests;
using PdfTool.Application.Interfaces;
using PdfTool.Application.Services;
using PdfTool.Domain.Common;
using PdfTool.Domain.Models;
using System.IO;
using System.IO.Compression;

namespace PdfTool.Api.Controllers;

/// <summary>PDF export endpoints.</summary>
[ApiController]
[Route("api/pdf/export")]
public sealed class PdfExportController : ControllerBase
{
    private readonly IPdfAppService _service;
    private readonly IFileTypeValidator _validator;
    private readonly IFileStorage _fileStorage;

    public PdfExportController(IPdfAppService service, IFileTypeValidator validator, IFileStorage fileStorage)
    {
        _service = service;
        _validator = validator;
        _fileStorage = fileStorage;
    }

    /// <summary>Exports PDF pages to PNG images.</summary>
    [HttpPost("png")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> PngForm([FromForm] PdfToPngFormRequest request, CancellationToken ct)
    {
        var file = await FileMapping.FromFormFileAsync(request.PdfFile);
        if (!_validator.IsPdf(file.Bytes))
        {
            return BadRequest(ValidationHelper.InvalidFile<PageImagesResult>("Only PDF files are allowed"));
        }

        var appRequest = new PdfToPngRequest
        {
            PdfFile = file,
            Dpi = request.Dpi,
            ZipOutput = request.ZipOutput,
            PageRange = request.PageRange,
            ReturnBase64 = request.ReturnBase64
        };

        var result = await _service.PdfToPngAsync(appRequest, ct);
        if (!result.Success || result.Data == null)
        {
            return BadRequest(result);
        }

        if (request.ReturnBase64)
        {
            var response = new Base64ImagesResponse();
            foreach (var img in result.Data.Images)
            {
                response.Images.Add(new Base64BinaryResponse
                {
                    Base64 = Convert.ToBase64String(img.Bytes),
                    ContentType = img.ContentType,
                    FileName = img.FileName
                });
            }
            return Ok(OperationResult<Base64ImagesResponse>.Ok(response));
        }

        if (request.ZipOutput)
        {
            var zipBytes = BuildZip(result.Data);
            var key = await _fileStorage.SaveAsync("pages.zip", zipBytes, ct);
            var response = new DownloadLinkResponse
            {
                Url = BuildFileUrl(key),
                ContentType = "application/zip",
                FileName = "pages.zip"
            };
            return Ok(OperationResult<DownloadLinkResponse>.Ok(response));
        }

        var links = new DownloadLinksResponse();
        foreach (var img in result.Data.Images)
        {
            var key = await _fileStorage.SaveAsync(img.FileName, img.Bytes, ct);
            links.Files.Add(new DownloadLinkResponse
            {
                Url = BuildFileUrl(key),
                ContentType = img.ContentType,
                FileName = img.FileName
            });
        }
        return Ok(OperationResult<DownloadLinksResponse>.Ok(links));
    }

    private string BuildFileUrl(string key)
    {
        return $"{Request.Scheme}://{Request.Host}/api/files/{key}";
    }

    private static byte[] BuildZip(PageImagesResult result)
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            foreach (var img in result.Images)
            {
                var entry = archive.CreateEntry(img.FileName, CompressionLevel.Fastest);
                using var entryStream = entry.Open();
                entryStream.Write(img.Bytes, 0, img.Bytes.Length);
            }
        }
        return ms.ToArray();
    }
}
