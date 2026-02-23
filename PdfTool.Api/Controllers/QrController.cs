using IronPdf;
using IronQr;
using IronSoftware.Drawing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using PdfTool.Api.Models.Requests;
using PdfTool.Api.Utilities;
using PdfTool.Application.Interfaces;
using System.Net;
using System.Text.Json;

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
    private readonly IDataProtector _protector;

    public QrController(IFileTypeValidator validator, IDataProtectionProvider protectionProvider)
    {
        _validator = validator;
        _protector = protectionProvider.CreateProtector("QrPayload.v1");
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

    /// <summary>
    /// Generates a QR code containing an encrypted URL with the supplied parameters.
    /// If "data" is provided, returns the populated form directly.
    /// </summary>
    [HttpGet("generate")]
    public IActionResult Generate(
        [FromQuery] string? fullName,
        [FromQuery] int? yearsOfExperience,
        [FromQuery] decimal? salary)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return BadRequest(new { success = false, error = "'fullName' is required." });
        }

        if (yearsOfExperience is null)
        {
            return BadRequest(new { success = false, error = "'yearsOfExperience' is required." });
        }

        if (salary is null)
        {
            return BadRequest(new { success = false, error = "'salary' is required." });
        }

        if (yearsOfExperience < 0)
        {
            return BadRequest(new { success = false, error = "'yearsOfExperience' must be 0 or greater." });
        }

        if (salary < 0)
        {
            return BadRequest(new { success = false, error = "'salary' must be 0 or greater." });
        }

        var payloadToProtect = new QrPayload
        {
            FullName = fullName,
            YearsOfExperience = yearsOfExperience.Value,
            Salary = salary.Value
        };

        var jsonToProtect = JsonSerializer.Serialize(payloadToProtect);
        var token = _protector.Protect(jsonToProtect);

        var url = Url.Action(nameof(Generate), "Qr", new { data = token }, Request.Scheme);
        if (string.IsNullOrWhiteSpace(url))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, error = "Unable to create QR URL." });
        }

        var qr = QrWriter.Write(url);
        using var qrImage = qr.Save();
        var pngBytes = qrImage.ExportBytes(AnyBitmap.ImageFormat.Png, 100);

        return File(pngBytes, "image/png", "qr.png");
    }

    /// <summary>
    /// Accepts a QR image upload, reads the embedded URL, decrypts the data, and returns the form as an image.
    /// </summary>
    [HttpPost("form-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> FormImage([FromForm] QrReadFormRequest request, CancellationToken ct)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(new { success = false, error = "A QR image file is required." });
        }

        if (request.File.Length > MaxFileSizeBytes)
        {
            return BadRequest(new { success = false, error = "File size exceeds 10MB limit." });
        }

        var input = await FileMapping.FromFormFileAsync(request.File);
        ct.ThrowIfCancellationRequested();

        if (!_validator.IsImage(input.Bytes) || !IsSupportedImage(request.File))
        {
            return BadRequest(new { success = false, error = "Only PNG or JPG images are allowed." });
        }

        var value = ReadFromImageBytes(input.Bytes);
        if (string.IsNullOrWhiteSpace(value))
        {
            return Ok(new { success = false, message = "No QR code detected." });
        }

        var token = ExtractDataToken(value);
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new { success = false, error = "QR value does not contain encrypted data." });
        }

        QrPayload payload;
        try
        {
            var json = _protector.Unprotect(token);
            payload = JsonSerializer.Deserialize<QrPayload>(json) ?? new QrPayload();
        }
        catch
        {
            return BadRequest(new { success = false, error = "Invalid or expired data." });
        }

        var html = BuildFormHtml(payload);
        var renderer = new ChromePdfRenderer();
        using var pdf = renderer.RenderHtmlAsPdf(html);
        var bitmaps = pdf.ToBitmap(new[] { 0 }, 200, true);
        try
        {
            using var bmp = bitmaps[0];
            var pngBytes = bmp.ExportBytes(AnyBitmap.ImageFormat.Png, 100);
            return File(pngBytes, "image/png", "form.png");
        }
        finally
        {
            foreach (var bmp in bitmaps)
            {
                bmp?.Dispose();
            }
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

    private sealed class QrPayload
    {
        public string? FullName { get; init; }
        public int YearsOfExperience { get; init; }
        public decimal Salary { get; init; }
    }

    private static string BuildFormHtml(QrPayload payload)
    {
        var name = WebUtility.HtmlEncode(payload.FullName ?? string.Empty);
        var years = WebUtility.HtmlEncode(payload.YearsOfExperience.ToString());
        var salaryValue = WebUtility.HtmlEncode(payload.Salary.ToString("0.##"));

        return $@"<!doctype html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>QR Details</title>
  <style>
    body {{ font-family: Arial, sans-serif; background: #f6f6f6; margin: 0; padding: 24px; }}
    .card {{ max-width: 520px; margin: 0 auto; background: #fff; border-radius: 8px; padding: 24px; box-shadow: 0 2px 10px rgba(0,0,0,0.08); }}
    h1 {{ font-size: 20px; margin: 0 0 16px; }}
    label {{ display: block; font-weight: 600; margin: 12px 0 6px; }}
    input {{ width: 100%; padding: 10px 12px; border: 1px solid #ccc; border-radius: 6px; font-size: 14px; }}
  </style>
</head>
<body>
  <div class=""card"">
    <h1>QR Details</h1>
    <form>
      <label for=""fullName"">Full name</label>
      <input id=""fullName"" name=""fullName"" value=""{name}"" readonly />

      <label for=""yearsOfExperience"">Years of experience</label>
      <input id=""yearsOfExperience"" name=""yearsOfExperience"" value=""{years}"" readonly />

      <label for=""salary"">Salary</label>
      <input id=""salary"" name=""salary"" value=""{salaryValue}"" readonly />
    </form>
  </div>
</body>
</html>";
    }

    private static string? ExtractDataToken(string value)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            var query = QueryHelpers.ParseQuery(uri.Query);
            if (query.TryGetValue("data", out var data))
            {
                return data.FirstOrDefault();
            }
        }

        if (value.StartsWith("data=", StringComparison.OrdinalIgnoreCase))
        {
            return value["data=".Length..];
        }

        return value;
    }
}
