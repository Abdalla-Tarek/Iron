using IronPdf;
using IronQr;
using IronSoftware.Drawing;
using Microsoft.AspNetCore.Mvc;
using PdfTool.Api.Models.Requests;
using PdfTool.Api.Utilities;
using PdfTool.Application.Interfaces;

namespace PdfTool.Api.Controllers;

/// <summary>QR read/generate endpoints.</summary>
[ApiController]
[Route("api/qr")]
public sealed class QrController : ControllerBase
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
    private static readonly string[] AllowedImageExtensions = { ".png", ".jpg", ".jpeg" };
    private static readonly string[] AllowedImageContentTypes = { "image/png", "image/jpeg" };

    private readonly IFileTypeValidator _validator;

    public QrController(IFileTypeValidator validator)
    {
        _validator = validator;
    }

    /// <summary>Reads a QR from an uploaded file or generates one from text.</summary>
    [HttpPost("read")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Read([FromForm] QrReadFormRequest request, CancellationToken ct)
    {
        try
        {
            // If text is provided, generate QR and return base64 image.
            //if (!string.IsNullOrWhiteSpace(request.Text))
            //{
            //    var qr = QrWriter.Write(request.Text);
            //    using var qrImage = qr.Save();
            //    var pngBytes = qrImage.ExportBytes(AnyBitmap.ImageFormat.Png, 100);

            //    return Ok(new
            //    {
            //        success = true,
            //        mode = "generated",
            //        value = request.Text,
            //        base64Image = Convert.ToBase64String(pngBytes)
            //    });
            //}

            // Validate file presence.
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { success = false, error = "Either 'text' or 'file' must be provided." });
            }

            // Enforce max file size.
            if (request.File.Length > MaxFileSizeBytes)
            {
                return BadRequest(new { success = false, error = "File size exceeds 10MB limit." });
            }

            var input = await FileMapping.FromFormFileAsync(request.File);
            ct.ThrowIfCancellationRequested();

            // Validate file type by signature and allowed content types.
            if (_validator.IsImage(input.Bytes))
            {
                if (!IsSupportedImage(request.File))
                {
                    return BadRequest(new { success = false, error = "Only PNG or JPG images are allowed." });
                }

                var value = ReadFromImageBytes(input.Bytes);
                return value != null
                    ? Ok(new { success = true, mode = "decoded", value })
                    : Ok(new { success = false, message = "No QR code detected." });
            }

            if (_validator.IsPdf(input.Bytes))
            {
                var value = ReadFromPdfBytes(input.Bytes);
                return value != null
                    ? Ok(new { success = true, mode = "decoded", value })
                    : Ok(new { success = false, message = "No QR code detected." });
            }

            return BadRequest(new { success = false, error = "Unsupported file type. Only PDF or PNG/JPG images are allowed." });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, error = ex.Message });
        }
    }

    private static bool IsSupportedImage(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (ext != null && AllowedImageExtensions.Contains(ext))
        {
            return true;
        }

        var contentType = file.ContentType?.ToLowerInvariant();
        return contentType != null && AllowedImageContentTypes.Contains(contentType);
    }

    private static string? ReadFromImageBytes(byte[] bytes)
    {
        using var bitmap = new AnyBitmap(bytes);
        var input = new QrImageInput(bitmap);
        var reader = new QrReader();
        var results = reader.Read(input);

        return results.FirstOrDefault()?.Value;
    }

    private static string? ReadFromPdfBytes(byte[] bytes)
    {
        using var pdf = new PdfDocument(bytes);
        // Render PDF pages to images so we can scan each page for QR codes.
        var pages = Enumerable.Range(0, pdf.PageCount).ToList();
        var bitmaps = pdf.ToBitmap(pages, 200, true);
        var reader = new QrReader();

        try
        {
            foreach (var bmp in bitmaps)
            {
                var input = new QrImageInput(bmp);
                var results = reader.Read(input);
                var value = results.FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }
        finally
        {
            // Ensure all bitmaps are disposed if not already.
            foreach (var bmp in bitmaps)
            {
                bmp?.Dispose();
            }
        }

        return null;
    }
}
