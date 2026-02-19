using Microsoft.AspNetCore.Mvc;
using PdfTool.Application.Interfaces;

namespace PdfTool.Api.Controllers;

/// <summary>File download endpoints.</summary>
[ApiController]
[Route("api/files")]
public sealed class FilesController : ControllerBase
{
    private readonly IFileStorage _fileStorage;

    public FilesController(IFileStorage fileStorage)
    {
        _fileStorage = fileStorage;
    }

    /// <summary>Downloads a stored file by key.</summary>
    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key, CancellationToken ct)
    {
        var bytes = await _fileStorage.GetAsync(key, ct);
        if (bytes == null)
        {
            return NotFound();
        }

        var contentType = GuessContentType(key);
        var fileName = ExtractFileName(key);
        return new FileContentResult(bytes, contentType)
        {
            FileDownloadName = fileName
        };
    }

    private static string GuessContentType(string key)
    {
        var lower = key.ToLowerInvariant();
        if (lower.EndsWith(".png"))
        {
            return "image/png";
        }
        if (lower.EndsWith(".zip"))
        {
            return "application/zip";
        }
        if (lower.EndsWith(".pdf"))
        {
            return "application/pdf";
        }
        return "application/octet-stream";
    }

    private static string ExtractFileName(string key)
    {
        var index = key.IndexOf('-');
        if (index >= 0 && index + 1 < key.Length)
        {
            return key[(index + 1)..];
        }
        return key;
    }
}
