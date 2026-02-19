using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using FluentAssertions;
using IronPdf;
using PdfTool.Application.DTOs.Requests;
using PdfTool.Application.FraudDetection;
using PdfTool.Infrastructure.Services;
using PdfTool.Domain.Enums;
using Xunit;

namespace PdfTool.Tests;

public sealed class PdfToolTests
{
    [Fact]
    public async Task RemovePage_RemovesCorrectPage()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<div>Page1</div><div style='page-break-after:always'></div><div>Page2</div>");
        var input = new FileInput { Bytes = pdf.BinaryData, FileName = "test.pdf", ContentType = "application/pdf" };

        var editor = new IronPdfEditor();
        var result = await editor.RemovePageAsync(new RemovePageRequest { PdfFile = input, PageNumber = 1 }, CancellationToken.None);
        using var output = new PdfDocument(result.Bytes);

        output.PageCount.Should().Be(1);
    }

    [Fact]
    public async Task Merge_AppendsPdfs()
    {
        var renderer = new ChromePdfRenderer();
        var pdf1 = renderer.RenderHtmlAsPdf("<div>One</div>");
        var pdf2 = renderer.RenderHtmlAsPdf("<div>Two</div>");

        var merger = new IronPdfMerger();
        var result = await merger.MergeAsync(new MergeRequest
        {
            Main = new FileInput { Bytes = pdf1.BinaryData, FileName = "a.pdf", ContentType = "application/pdf" },
            Append = new FileInput { Bytes = pdf2.BinaryData, FileName = "b.pdf", ContentType = "application/pdf" }
        }, CancellationToken.None);

        using var merged = new PdfDocument(result.Bytes);
        merged.PageCount.Should().Be(2);
    }

    [Fact]
    public async Task Scan_DetectsJavaScript()
    {
        var detector = new FraudDetector(new FraudDetectionOptions());
        var scanner = new IronPdfScanner(detector);
        var testPdf = await scanner.GenerateJsTestPdfAsync(new GenerateJsTestPdfRequest { JsCode = "app.alert('x');" }, CancellationToken.None);

        var report = await scanner.ScanAsync(new ScanPdfRequest { PdfFile = new FileInput { Bytes = testPdf.Bytes, FileName = "js.pdf", ContentType = "application/pdf" } }, CancellationToken.None);
        report.Findings.Any(f => f.Code == "actions.javascript").Should().BeTrue();
    }

    [Fact]
    public async Task Sanitize_RemovesJavaScriptSignal()
    {
        var detector = new FraudDetector(new FraudDetectionOptions());
        var scanner = new IronPdfScanner(detector);
        var sanitizer = new IronPdfSanitizer(scanner);

        var testPdf = await scanner.GenerateJsTestPdfAsync(new GenerateJsTestPdfRequest { JsCode = "app.alert('x');" }, CancellationToken.None);
        var result = await sanitizer.SanitizeAsync(new SanitizePdfRequest
        {
            PdfFile = new FileInput { Bytes = testPdf.Bytes, FileName = "js.pdf", ContentType = "application/pdf" },
            SanitizeMode = SanitizeMode.RasterizeAllPages,
            Dpi = 100,
            RemoveMetadata = true
        }, CancellationToken.None);

        result.Before.Findings.Any(f => f.Code == "actions.javascript").Should().BeTrue();
        result.After.Findings.Any(f => f.Code == "actions.javascript").Should().BeFalse();
    }

    [Fact]
    public void OcrParsing_ExtractsFields()
    {
        var extractor = new RegexOcrFieldExtractor();
        var raw = "Name: John Doe\nID Number: A1234567\nDOB: 1990-01-02\nExpiry: 2030-12-31\nNationality: USA\nGender: M";
        var fields = extractor.ExtractFields(raw);

        fields.Should().Contain(f => f.Name == "Name" && f.Value.Contains("John Doe"));
        fields.Should().Contain(f => f.Name == "IDNumber" && f.Value == "A1234567");
        fields.Should().Contain(f => f.Name == "DOB");
        fields.Should().Contain(f => f.Name == "Expiry");
        fields.Should().Contain(f => f.Name == "Nationality");
        fields.Should().Contain(f => f.Name == "Gender");
    }
}
