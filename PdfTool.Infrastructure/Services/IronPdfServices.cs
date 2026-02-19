using System;
using System.Collections.Generic;
using SD = System.Drawing;
using SDI = System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IronPdf;
using IronPdf.Editing;
using IronPdf.Imaging;
using IronPdf.Rendering;
using IronPdf.Security;
using IronPdf.Signing;
using IronSoftware.Drawing;
using PdfTool.Application.DTOs.Requests;
using PdfTool.Application.FraudDetection;
using PdfTool.Application.Interfaces;
using PdfTool.Application.Utilities;
using PdfTool.Domain.Enums;
using PdfTool.Domain.Models;

namespace PdfTool.Infrastructure.Services;

public sealed class IronPdfCreator : IPdfCreator
{
    public Task<BinaryContent> HtmlToPdfAsync(HtmlToPdfRequest request, CancellationToken ct)
    {
        var renderer = new ChromePdfRenderer();
        var options = renderer.RenderingOptions;
        options.PaperSize = ParsePaperSize(request.PageOptions.PageSize);
        options.DPI = request.PageOptions.Dpi;
        options.MarginTop = request.Margins.Top;
        options.MarginBottom = request.Margins.Bottom;
        options.MarginLeft = request.Margins.Left;
        options.MarginRight = request.Margins.Right;
        options.PaperOrientation = request.Landscape ? PdfPaperOrientation.Landscape : PdfPaperOrientation.Portrait;

        var pdf = request.BaseUrl is null
            ? renderer.RenderHtmlAsPdf(request.Html)
            : renderer.RenderHtmlAsPdf(request.Html, new Uri(request.BaseUrl));

        return Task.FromResult(new BinaryContent
        {
            Bytes = pdf.BinaryData,
            ContentType = "application/pdf",
            FileName = "document.pdf"
        });
    }

    private static PdfPaperSize ParsePaperSize(string pageSize)
    {
        if (Enum.TryParse<PdfPaperSize>(pageSize, true, out var result))
        {
            return result;
        }
        return PdfPaperSize.A4;
    }
}

public sealed class IronPdfWatermarker : IPdfWatermarker
{
    public Task<BinaryContent> AddWatermarkAsync(WatermarkRequest request, CancellationToken ct)
    {
        using var pdf = PdfPasswordHelper.Open(request.PdfFile.Bytes, request.PdfPassword);
        var stamper = new TextStamper(request.Text)
        {
            Opacity = (int)request.Opacity,
            Rotation = request.Rotation,
            FontSize = request.FontSize,
            HorizontalAlignment = ParseHorizontal(request.Position),
            VerticalAlignment = ParseVertical(request.Position)
        };
        pdf.ApplyStamp(stamper);
        return Task.FromResult(new BinaryContent
        {
            Bytes = pdf.BinaryData,
            ContentType = "application/pdf",
            FileName = "watermarked.pdf"
        });
    }

    private static HorizontalAlignment ParseHorizontal(string position)
    {
        return position switch
        {
            "TopLeft" or "BottomLeft" => HorizontalAlignment.Left,
            "TopRight" or "BottomRight" => HorizontalAlignment.Right,
            _ => HorizontalAlignment.Center
        };
    }

    private static VerticalAlignment ParseVertical(string position)
    {
        return position switch
        {
            "TopLeft" or "TopRight" => VerticalAlignment.Top,
            "BottomLeft" or "BottomRight" => VerticalAlignment.Bottom,
            _ => VerticalAlignment.Middle
        };
    }
}

public sealed class IronPdfCompressor : IPdfCompressor
{
    public Task<CompressResult> CompressAsync(CompressRequest request, CancellationToken ct)
    {
        using var pdf = PdfPasswordHelper.Open(request.PdfFile.Bytes, request.PdfPassword);
        var originalSize = pdf.BinaryData.LongLength;

        byte[] bestBytes;
        switch (request.CompressionProfile)
        {
            case CompressionProfile.BestSizeMultiPass:
                bestBytes = MultiPassCompress(pdf, request);
                break;
            case CompressionProfile.ImageFocused:
                ApplyImageFocused(pdf, request);
                bestBytes = pdf.BinaryData;
                break;
            default:
                ApplyBalanced(pdf, request);
                bestBytes = pdf.BinaryData;
                break;
        }

        var result = new CompressResult
        {
            Pdf = new BinaryContent
            {
                Bytes = bestBytes,
                ContentType = "application/pdf",
                FileName = "compressed.pdf",
                Metadata = new Dictionary<string, object?>
                {
                    ["originalSize"] = originalSize,
                    ["finalSize"] = bestBytes.LongLength
                }
            },
            OriginalSize = originalSize,
            FinalSize = bestBytes.LongLength
        };
        return Task.FromResult(result);
    }

    private static void ApplyBalanced(PdfDocument pdf, CompressRequest request)
    {
        var options = new CompressionOptions
        {
            CompressImages = true,
            JpegQuality = request.ImageQuality ?? 75,
            ShrinkImages = true,
            RemoveStructureTree = request.RemoveUnusedObjects
        };
        pdf.Compress(options);
    }

    private static void ApplyImageFocused(PdfDocument pdf, CompressRequest request)
    {
        var quality = request.ImageQuality ?? 70;
        pdf.CompressImages(quality, true, true);
    }

    private static byte[] MultiPassCompress(PdfDocument pdf, CompressRequest request)
    {
        var settings = new[] { 50, 60, 70, 80 };
        byte[] best = pdf.BinaryData;
        foreach (var quality in settings)
        {
            using var working = new PdfDocument(best);
            working.Compress(new CompressionOptions
            {
                CompressImages = true,
                JpegQuality = request.ImageQuality ?? quality,
                ShrinkImages = true,
                RemoveStructureTree = request.RemoveUnusedObjects
            });
            var bytes = working.BinaryData;
            if (bytes.Length < best.Length)
            {
                best = bytes;
            }
        }
        return best;
    }
}

public sealed class IronPdfEncryptor : IPdfEncryptor
{
    public Task<BinaryContent> EncryptAsync(EncryptRequest request, CancellationToken ct)
    {
        using var pdf = PdfPasswordHelper.Open(request.PdfFile.Bytes, request.PdfPassword);
        var security = pdf.SecuritySettings;
        security.OwnerPassword = request.OwnerPassword;
        if (!string.IsNullOrWhiteSpace(request.UserPassword))
        {
            security.UserPassword = request.UserPassword;
        }

        security.AllowUserPrinting = request.Permissions.HasFlag(PermissionFlags.Print)
            ? PdfPrintSecurity.FullPrintRights
            : PdfPrintSecurity.NoPrint;

        security.AllowUserCopyPasteContent = request.Permissions.HasFlag(PermissionFlags.Copy);

        security.AllowUserEdits = request.Permissions.HasFlag(PermissionFlags.Edit)
            ? PdfEditSecurity.EditAll
            : PdfEditSecurity.NoEdit;

        security.AllowUserAnnotations = request.Permissions.HasFlag(PermissionFlags.Annotate);
        security.AllowUserFormData = request.Permissions.HasFlag(PermissionFlags.FillForms);

        return Task.FromResult(new BinaryContent
        {
            Bytes = pdf.BinaryData,
            ContentType = "application/pdf",
            FileName = "encrypted.pdf"
        });
    }
}

public sealed class IronPdfConverter : IPdfConverter
{
    public Task<BinaryContent> ConvertToPdfAAsync(ConvertPdfARequest request, CancellationToken ct)
    {
        using var pdf = PdfPasswordHelper.Open(request.PdfFile.Bytes, request.PdfPassword);
        var pdfaVersion = request.PdfAType switch
        {
            PdfAType.PDFA1b => PdfAVersions.PdfA1b,
            PdfAType.PDFA2b => PdfAVersions.PdfA2b,
            _ => PdfAVersions.PdfA3b
        };
        pdf.ConvertToPdfA(pdfaVersion);
        return Task.FromResult(new BinaryContent
        {
            Bytes = pdf.BinaryData,
            ContentType = "application/pdf",
            FileName = "pdfa.pdf"
        });
    }

    public Task<PageImagesResult> PdfToPngAsync(PdfToPngRequest request, CancellationToken ct)
    {
        using var pdf = PdfPasswordHelper.Open(request.PdfFile.Bytes, request.PdfPassword);
        var pages = PageRangeParser.Parse(request.PageRange, pdf.PageCount);
        if (pages.Count == 0)
        {
            pages = Enumerable.Range(0, pdf.PageCount).ToList();
        }
        var bitmaps = pdf.ToBitmap(pages, request.Dpi, true);
        var result = new PageImagesResult();

        for (var i = 0; i < bitmaps.Length; i++)
        {
            using var bmp = bitmaps[i];
            var bytes = bmp.ExportBytes(AnyBitmap.ImageFormat.Png, 100);
            result.Images.Add(new BinaryContent
            {
                Bytes = bytes,
                ContentType = "image/png",
                FileName = $"page-{pages[i] + 1}.png"
            });
        }

        return Task.FromResult(result);
    }

    public Task<BinaryContent> ImageToPdfAsync(ImageToPdfRequest request, CancellationToken ct)
    {
        using var bitmap = new AnyBitmap(request.ImageFile.Bytes);
        var pdf = ImageToPdfConverter.ImageToPdf(bitmap, ImageBehavior.FitToPageAndMaintainAspectRatio);
        if (request.CompressAfter)
        {
            pdf.Compress(new CompressionOptions
            {
                CompressImages = true,
                JpegQuality = request.ImageQuality ?? 75,
                ShrinkImages = true
            });
        }
        return Task.FromResult(new BinaryContent
        {
            Bytes = pdf.BinaryData,
            ContentType = "application/pdf",
            FileName = "image.pdf"
        });
    }
}

public sealed class IronPdfEditor : IPdfEditor
{
    public Task<BinaryContent> RemovePageAsync(RemovePageRequest request, CancellationToken ct)
    {
        using var pdf = PdfPasswordHelper.Open(request.PdfFile.Bytes, request.PdfPassword);
        pdf.RemovePage(request.PageNumber - 1);
        return Task.FromResult(new BinaryContent
        {
            Bytes = pdf.BinaryData,
            ContentType = "application/pdf",
            FileName = "removed-page.pdf"
        });
    }
}

public sealed class IronPdfMerger : IPdfMerger
{
    public Task<BinaryContent> MergeAsync(MergeRequest request, CancellationToken ct)
    {
        using var main = PdfPasswordHelper.Open(request.Main.Bytes, request.MainPassword);
        using var append = PdfPasswordHelper.Open(request.Append.Bytes, request.AppendPassword);
        main.AppendPdf(append);
        return Task.FromResult(new BinaryContent
        {
            Bytes = main.BinaryData,
            ContentType = "application/pdf",
            FileName = "merged.pdf"
        });
    }
}

public sealed class IronPdfSigner : IPdfSigner
{
    public Task<BinaryContent> SignPfxTextAsync(SignPfxTextRequest request, CancellationToken ct)
    {
        using var pdf = PdfPasswordHelper.Open(request.PdfFile.Bytes, request.PdfPassword);
        var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(request.PfxFile.Bytes, request.PfxPassword);
        var signature = new PdfSignature(cert)
        {
            SigningReason = request.Reason,
            SigningLocation = request.Location
        };

        var imgBytes = SignatureImageBuilder.BuildTextSignatureImage(request.VisibleText);
        var rect = new IronSoftware.Drawing.Rectangle((int)request.Rect.X, (int)request.Rect.Y, (int)request.Rect.Width, (int)request.Rect.Height);
        signature.SignatureImage = new PdfSignatureImage(imgBytes, request.PageNumber - 1, rect);

        pdf.Sign(signature, SignaturePermissions.NoChangesAllowed);
        return Task.FromResult(new BinaryContent
        {
            Bytes = pdf.BinaryData,
            ContentType = "application/pdf",
            FileName = "signed.pdf"
        });
    }

    public Task<BinaryContent> SignPfxImageAsync(SignPfxImageRequest request, CancellationToken ct)
    {
        using var pdf = PdfPasswordHelper.Open(request.PdfFile.Bytes, request.PdfPassword);
        var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(request.PfxFile.Bytes, request.PfxPassword);
        var signature = new PdfSignature(cert)
        {
            SigningReason = request.Reason,
            SigningLocation = request.Location
        };
        var rect = new IronSoftware.Drawing.Rectangle((int)request.Rect.X, (int)request.Rect.Y, (int)request.Rect.Width, (int)request.Rect.Height);
        signature.SignatureImage = new PdfSignatureImage(request.ImageAppearanceFile.Bytes, request.PageNumber - 1, rect);
        pdf.Sign(signature, SignaturePermissions.NoChangesAllowed);
        return Task.FromResult(new BinaryContent
        {
            Bytes = pdf.BinaryData,
            ContentType = "application/pdf",
            FileName = "signed.pdf"
        });
    }

    public Task<BinaryContent> StampSignatureAsync(StampSignatureRequest request, CancellationToken ct)
    {
        using var pdf = PdfPasswordHelper.Open(request.PdfFile.Bytes, request.PdfPassword);
        var pageIndex = request.PageNumber <= 0 ? pdf.PageCount - 1 : request.PageNumber - 1;
        using var bitmap = new AnyBitmap(request.SignatureImageFile.Bytes);
        var scale = request.Scale;
        
        var stamper = new ImageStamper(bitmap)
        {
            Scale = scale
        };
        if (request.X.HasValue || request.Y.HasValue)
        {
            stamper.HorizontalAlignment = HorizontalAlignment.Left;
            stamper.VerticalAlignment = VerticalAlignment.Top;
            stamper.HorizontalOffset = new Length((int)(request.X ?? 0), MeasurementUnit.Pixel, 0);
            stamper.VerticalOffset = new Length((int)(request.Y ?? 0), MeasurementUnit.Pixel, 0);
        }
        // If the stamp appears "behind" content, flattening the page makes the stamp visible.
        pdf.Flatten(new[] { pageIndex });
        pdf.ApplyStamp(stamper, pageIndex);
        return Task.FromResult(new BinaryContent
        {
            Bytes = pdf.BinaryData,
            ContentType = "application/pdf",
            FileName = "stamped.pdf"
        });
    }
}

public sealed class IronPdfScanner : IPdfScanner
{
    private readonly FraudDetector _detector;

    public IronPdfScanner(FraudDetector detector)
    {
        _detector = detector;
    }

    public Task<PdfScanReport> ScanAsync(ScanPdfRequest request, CancellationToken ct)
    {
        var scanBytes = PdfPasswordHelper.DecryptBytes(request.PdfFile.Bytes, request.PdfPassword);
        var report = _detector.Analyze(scanBytes);
        var scan = new PdfScanReport
        {
            Findings = report.Findings,
            Metadata = report.Signals
        };

        scan.Metadata["size_bytes"] = scanBytes.LongLength;
        try
        {
            using var pdf = new PdfDocument(scanBytes);
            scan.Metadata["page_count"] = pdf.PageCount;
        }
        catch
        {
            scan.Metadata["page_count"] = null;
        }

        return Task.FromResult(scan);
    }

    public Task<BinaryContent> GenerateJsTestPdfAsync(GenerateJsTestPdfRequest request, CancellationToken ct)
    {
        var bytes = JsTestPdfBuilder.Build(request.JsCode);
        return Task.FromResult(new BinaryContent
        {
            Bytes = bytes,
            ContentType = "application/pdf",
            FileName = "js-test.pdf"
        });
    }
}

public sealed class IronPdfSanitizer : IPdfSanitizer
{
    private readonly IPdfScanner _scanner;

    public IronPdfSanitizer(IPdfScanner scanner)
    {
        _scanner = scanner;
    }

    public async Task<(BinaryContent Pdf, PdfScanReport Before, PdfScanReport After)> SanitizeAsync(SanitizePdfRequest request, CancellationToken ct)
    {
        var sourceBytes = PdfPasswordHelper.DecryptBytes(request.PdfFile.Bytes, request.PdfPassword);
        var sourceFile = new FileInput
        {
            Bytes = sourceBytes,
            FileName = request.PdfFile.FileName,
            ContentType = request.PdfFile.ContentType
        };
        var before = await _scanner.ScanAsync(new ScanPdfRequest { PdfFile = sourceFile }, ct);
        using var pdf = new PdfDocument(sourceBytes);
        var bitmaps = pdf.ToBitmap(Enumerable.Range(0, pdf.PageCount).ToList(), request.Dpi, true);
        var imageBitmaps = new List<AnyBitmap>();
        try
        {
            foreach (var b in bitmaps)
            {
                imageBitmaps.Add(b);
            }

            var sanitized = ImageToPdfConverter.ImageToPdf(imageBitmaps, ImageBehavior.FitToPageAndMaintainAspectRatio);
            var sanitizedBytes = sanitized.BinaryData;
            var after = await _scanner.ScanAsync(new ScanPdfRequest { PdfFile = new FileInput { Bytes = sanitizedBytes, FileName = "sanitized.pdf", ContentType = "application/pdf" } }, ct);

            return (new BinaryContent
            {
                Bytes = sanitizedBytes,
                ContentType = "application/pdf",
                FileName = "sanitized.pdf"
            }, before, after);
        }
        finally
        {
            foreach (var b in imageBitmaps)
            {
                b.Dispose();
            }
        }
    }
}

public sealed class IronOcrService : IOcrService
{
    public IronOcrService()
    {
    }

    public Task<OcrResultModel> ExtractAsync(OcrExtractRequest request, CancellationToken ct)
    {
        var input = new IronOcr.OcrInput();
        if (request.File.ContentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
        {
            var pdfBytes = PdfPasswordHelper.DecryptBytes(request.File.Bytes, request.PdfPassword);
            input.LoadPdf(pdfBytes, 200, false, default, null);
        }
        else
        {
            input.LoadImage(request.File.Bytes);
        }

        var ocr = new IronOcr.IronTesseract();
        ConfigureLanguage(ocr);
        var result = ocr.Read(input);
        var fields = ExtractKeyValues(result, request.Keys, IsRtlLanguage(request.Language), request.KeySeparators, request.Language);

        return Task.FromResult(new OcrResultModel
        {
            Fields = fields
        });
    }

    public Task<string> ExtractTextAsync(OcrExtractRequest request, CancellationToken ct)
    {
        var input = new IronOcr.OcrInput();
        if (request.File.ContentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
        {
            var pdfBytes = PdfPasswordHelper.DecryptBytes(request.File.Bytes, request.PdfPassword);
            input.LoadPdf(pdfBytes, 200, false, default, null);
        }
        else
        {
            input.LoadImage(request.File.Bytes);
        }

        var ocr = new IronOcr.IronTesseract();
        ConfigureLanguage(ocr);
        var result = ocr.Read(input);
        return Task.FromResult(result.Text ?? string.Empty);
    }

    private static List<OcrField> ExtractKeyValues(IronOcr.OcrResult result, string[]? keys, bool preferRtl, string? keySeparators, string? keyLanguage)
    {
        var fields = new List<OcrField>();
        var keyFilter = BuildKeyFilter(keys);
        var keyLangFilter = BuildKeyLanguageFilter(keyLanguage);
        var separators = BuildSeparators(keySeparators);
        foreach (var page in result.Pages)
        {
            var lines = page.Lines
                .Select((line, index) => new OcrLineInfo(
                    index,
                    Normalize(line.Text),
                    (float)line.Confidence,
                    page.PageNumber,
                    (float)line.X,
                    (float)line.Y,
                    (float)line.Width,
                    (float)line.Height))
                .Where(l => !string.IsNullOrWhiteSpace(l.Text))
                .Where(l => !IsNoiseLine(l.Text))
                .ToList();

            if (lines.Count == 0)
            {
                continue;
            }

            var pageWidth = lines.Max(l => l.Right);
            var used = new HashSet<int>();

            // Pass 1: explicit "key: value"
            foreach (var line in lines)
            {
                if (used.Contains(line.Id))
                {
                    continue;
                }

                if (!TrySplitLabelValue(line.Text, separators, out var key, out var value))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(value))
                {
                    var neighbor = FindNearestNeighbor(line, lines, used, pageWidth, preferRtl, allowLabelValues: false)
                                   ?? FindNearestNeighbor(line, lines, used, pageWidth, preferRtl, allowLabelValues: true);
                    if (neighbor != null)
                    {
                        value = neighbor.Text;
                        used.Add(neighbor.Id);
                    }
                }

                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                {
                    AddField(fields, key, value, keyFilter, keyLangFilter);
                    used.Add(line.Id);
                }
            }

            // Pass 2: nearest neighbor on the same row (hybrid fallback)
            foreach (var line in lines)
            {
                if (used.Contains(line.Id))
                {
                    continue;
                }

                var neighbor = FindNearestNeighbor(line, lines, used, pageWidth, preferRtl, allowLabelValues: false)
                               ?? FindNearestNeighbor(line, lines, used, pageWidth, preferRtl, allowLabelValues: true);
                if (neighbor == null)
                {
                    continue;
                }

                var key = line.Text;
                var value = neighbor.Text;

                if (IsMostlyDigits(key) && !IsMostlyDigits(value))
                {
                    (key, value) = (value, key);
                }

                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                {
                    AddField(fields, key, value, keyFilter, keyLangFilter);
                    used.Add(line.Id);
                    used.Add(neighbor.Id);
                }
            }
        }

        return fields;
    }

    private static void AddField(List<OcrField> fields, string key, string value, HashSet<string>? keyFilter, Func<string, bool>? keyLangFilter)
    {
        var normalizedKey = Normalize(key);
        var normalizedValue = Normalize(value);
        if (string.IsNullOrWhiteSpace(normalizedKey) || string.IsNullOrWhiteSpace(normalizedValue))
        {
            return;
        }

        if (keyLangFilter != null && !keyLangFilter(normalizedKey))
        {
            return;
        }

        if (keyFilter != null && !keyFilter.Contains(normalizedKey))
        {
            return;
        }

        if (fields.Any(f => f.Key.Equals(normalizedKey, StringComparison.OrdinalIgnoreCase) &&
                            f.Value.Equals(normalizedValue, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        fields.Add(new OcrField
        {
            Key = normalizedKey,
            Value = normalizedValue
        });
    }

    private static HashSet<string>? BuildKeyFilter(string[]? keys)
    {
        if (keys == null || keys.Length == 0)
        {
            return null;
        }

        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in keys)
        {
            var normalized = Normalize(key);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                set.Add(normalized);
            }
        }

        return set.Count == 0 ? null : set;
    }

    private static OcrLineInfo? FindNearestNeighbor(OcrLineInfo line, List<OcrLineInfo> lines, HashSet<int> used, float pageWidth, bool preferRtl, bool allowLabelValues)
    {
        var preferLeft = preferRtl;
        var bestLeft = FindNeighbor(line, lines, used, preferLeft: true, allowLabelValues);
        var bestRight = FindNeighbor(line, lines, used, preferLeft: false, allowLabelValues);

        if (preferLeft)
        {
            return bestLeft ?? bestRight;
        }

        return bestRight ?? bestLeft;
    }

    private static OcrLineInfo? FindNeighbor(OcrLineInfo line, List<OcrLineInfo> lines, HashSet<int> used, bool preferLeft, bool allowLabelValues)
    {
        OcrLineInfo? best = null;
        var bestDx = float.MaxValue;
        foreach (var other in lines)
        {
            if (other.Id == line.Id || used.Contains(other.Id))
            {
                continue;
            }

            if (!allowLabelValues && other.IsLabel)
            {
                continue;
            }

            if (!IsSameRow(line, other))
            {
                continue;
            }

            float dx;
            if (preferLeft)
            {
                if (other.Right > line.Left)
                {
                    continue;
                }
                dx = line.Left - other.Right;
            }
            else
            {
                if (other.Left < line.Right)
                {
                    continue;
                }
                dx = other.Left - line.Right;
            }

            if (dx < bestDx)
            {
                bestDx = dx;
                best = other;
            }
        }

        return best;
    }

    private static bool IsSameRow(OcrLineInfo a, OcrLineInfo b)
    {
        var overlap = Math.Min(a.Bottom, b.Bottom) - Math.Max(a.Top, b.Top);
        var minHeight = Math.Min(a.Height, b.Height);
        return overlap >= minHeight * 0.4f;
    }

    private static bool TrySplitLabelValue(string text, char[] separators, out string key, out string value)
    {
        key = string.Empty;
        value = string.Empty;
        var idx = text.IndexOfAny(separators);
        if (idx <= 0)
        {
            return false;
        }

        key = text[..idx].Trim();
        value = text[(idx + 1)..].Trim();
        return !string.IsNullOrWhiteSpace(key);
    }

    private static char[] BuildSeparators(string? keySeparators)
    {
        if (string.IsNullOrWhiteSpace(keySeparators))
        {
            return new[] { ':' };
        }

        var text = keySeparators.Trim();
        if (text.Contains(','))
        {
            var parts = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var list = new List<char>();
            foreach (var part in parts)
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    list.Add(part[0]);
                }
            }
            return list.Count == 0 ? new[] { ':' } : list.Distinct().ToArray();
        }

        var chars = text.Where(c => !char.IsWhiteSpace(c)).Distinct().ToArray();
        return chars.Length == 0 ? new[] { ':' } : chars;
    }

    private static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var cleaned = RemoveBidiMarks(text);
        return string.Join(' ', cleaned.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
    }

    private static bool IsMostlyDigits(string text)
    {
        var digits = 0;
        var letters = 0;
        foreach (var ch in text)
        {
            if (char.IsDigit(ch))
            {
                digits++;
            }
            else if (char.IsLetter(ch))
            {
                letters++;
            }
        }
        return digits > 0 && digits >= letters * 2;
    }

    private static bool IsNoiseLine(string text)
    {
        var lower = text.ToLowerInvariant();
        if (lower.Contains("http") || lower.Contains("www.") || lower.Contains("page "))
        {
            return true;
        }
        return false;
    }

    private static string RemoveBidiMarks(string text)
    {
        return text.Replace("\u200E", string.Empty)
            .Replace("\u200F", string.Empty)
            .Replace("\u061C", string.Empty)
            .Replace("\u202A", string.Empty)
            .Replace("\u202B", string.Empty)
            .Replace("\u202C", string.Empty)
            .Replace("\u202D", string.Empty)
            .Replace("\u202E", string.Empty);
    }

    private static void ConfigureLanguage(IronOcr.IronTesseract ocr)
    {
        ocr.Language = IronOcr.OcrLanguage.English;
        ocr.AddSecondaryLanguage(IronOcr.OcrLanguage.Arabic);
    }

    private static Func<string, bool>? BuildKeyLanguageFilter(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return null;
        }

        var normalized = language.Trim().ToLowerInvariant();
        if (normalized.StartsWith("ar") || normalized.Contains("arab"))
        {
            return HasArabicLetters;
        }

        if (normalized.StartsWith("en") || normalized.Contains("latin"))
        {
            return HasLatinLetters;
        }

        return null;
    }

    private static bool HasArabicLetters(string text)
    {
        foreach (var ch in text)
        {
            if (ch >= '\u0600' && ch <= '\u06FF')
            {
                return true;
            }
        }
        return false;
    }

    private static bool HasLatinLetters(string text)
    {
        foreach (var ch in text)
        {
            if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z'))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsRtlLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return false;
        }

        var normalized = language.Trim().ToLowerInvariant();
        return normalized.StartsWith("ar") || normalized.Contains("arab");
    }

    private sealed class OcrLineInfo
    {
        public OcrLineInfo(int id, string text, float confidence, int pageNumber, float x, float y, float width, float height)
        {
            Id = id;
            Text = text;
            Confidence = confidence;
            PageNumber = pageNumber;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int Id { get; }
        public string Text { get; }
        public float Confidence { get; }
        public int PageNumber { get; }
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }
        public float Left => X;
        public float Right => X + Width;
        public float Top => Y;
        public float Bottom => Y + Height;
        public float CenterX => X + Width / 2f;
        public bool IsLabel => Text.EndsWith(":", StringComparison.Ordinal) ||
                               Text.Contains(':') ||
                               Text.Contains('：') ||
                               Text.Contains('؛');
    }
}

public sealed class RegexOcrFieldExtractor : IOcrFieldExtractor
{
    public List<OcrField> ExtractFields(string rawText)
    {
        return new List<OcrField>();
    }
}

internal static class PdfPasswordHelper
{
    public static PdfDocument Open(byte[] bytes, string? password)
    {
        return string.IsNullOrWhiteSpace(password)
            ? new PdfDocument(bytes)
            : new PdfDocument(bytes, password);
    }

    public static byte[] DecryptBytes(byte[] bytes, string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return bytes;
        }

        using var pdf = new PdfDocument(bytes, password);
        return pdf.BinaryData;
    }
}

internal static class SignatureImageBuilder
{
    public static byte[] BuildTextSignatureImage(string text)
    {
        using var bmp = new SD.Bitmap(400, 150);
        using var graphics = SD.Graphics.FromImage(bmp);
        graphics.Clear(SD.Color.White);
        using var font = new SD.Font("Arial", 24, SD.FontStyle.Bold, SD.GraphicsUnit.Pixel);
        using var brush = new SD.SolidBrush(SD.Color.Black);
        var rect = new SD.RectangleF(10, 10, 380, 130);
        var format = new SD.StringFormat { Alignment = SD.StringAlignment.Center, LineAlignment = SD.StringAlignment.Center };
        graphics.DrawString(text, font, brush, rect, format);
        using var ms = new MemoryStream();
        bmp.Save(ms, SDI.ImageFormat.Png);
        return ms.ToArray();
    }
}

internal static class JsTestPdfBuilder
{
    public static byte[] Build(string jsCode)
    {
        var objects = new List<string>
        {
            "1 0 obj\n<< /Type /Catalog /OpenAction 2 0 R >>\nendobj\n",
            $"2 0 obj\n<< /S /JavaScript /JS ({Escape(jsCode)}) >>\nendobj\n",
            "3 0 obj\n<< /Type /Pages /Count 1 /Kids [4 0 R] >>\nendobj\n",
            "4 0 obj\n<< /Type /Page /Parent 3 0 R /MediaBox [0 0 300 144] /Contents 5 0 R /Resources << /Font << /F1 6 0 R >> >> >>\nendobj\n",
            "5 0 obj\n<< /Length 44 >>\nstream\nBT /F1 12 Tf 72 72 Td (JS Test) Tj ET\nendstream\nendobj\n",
            "6 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n"
        };

        var sb = new StringBuilder();
        sb.Append("%PDF-1.4\n");
        var offsets = new List<int> { 0 };

        foreach (var obj in objects)
        {
            offsets.Add(sb.Length);
            sb.Append(obj);
        }

        var xrefPosition = sb.Length;
        sb.Append($"xref\n0 {objects.Count + 1}\n");
        sb.Append("0000000000 65535 f \n");
        for (var i = 1; i <= objects.Count; i++)
        {
            sb.Append(offsets[i].ToString("D10"));
            sb.Append(" 00000 n \n");
        }

        sb.Append("trailer\n<< /Size ");
        sb.Append(objects.Count + 1);
        sb.Append(" /Root 1 0 R >>\nstartxref\n");
        sb.Append(xrefPosition);
        sb.Append("\n%%EOF");

        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    private static string Escape(string input)
    {
        return input.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }
}
