using System.Text;
using System.Text.RegularExpressions;
using PdfTool.Domain.Enums;
using PdfTool.Domain.Models;

namespace PdfTool.Application.FraudDetection;

public sealed class FraudDetector
{
    private static readonly Regex InfoRegex = new(@"/(Producer|Creator)\s*\(([^\)]*)\)", RegexOptions.Compiled);
    private static readonly Regex TextObjectRegex = new(@"\bBT\b", RegexOptions.Compiled);

    private readonly FraudDetectionOptions _options;

    public FraudDetector(FraudDetectionOptions options)
    {
        _options = options;
    }

    public FraudReport Analyze(byte[] pdfBytes)
    {
        var text = Encoding.Latin1.GetString(pdfBytes);
        var findings = new List<ScanFinding>();
        var signals = new Dictionary<string, object?>();

        var producer = ExtractInfo(text, "Producer");
        var creator = ExtractInfo(text, "Creator");
        signals["producer"] = producer;
        signals["creator"] = creator;

        if (!string.IsNullOrWhiteSpace(producer) && _options.SuspiciousProducers.Any(p => producer.Contains(p, StringComparison.OrdinalIgnoreCase)))
        {
            findings.Add(NewFinding("metadata.producer", $"Suspicious producer: {producer}", Severity.Medium, producer));
        }

        if (!string.IsNullOrWhiteSpace(creator) && _options.SuspiciousProducers.Any(p => creator.Contains(p, StringComparison.OrdinalIgnoreCase)))
        {
            findings.Add(NewFinding("metadata.creator", $"Suspicious creator: {creator}", Severity.Medium, creator));
        }

        if (!text.Contains("<x:xmpmeta", StringComparison.OrdinalIgnoreCase))
        {
            findings.Add(NewFinding("metadata.xmp_missing", "XMP metadata missing", Severity.Low));
        }

        if (ContainsAny(text, new[] { "/JavaScript", "/JS" }))
        {
            findings.Add(NewFinding("actions.javascript", "Embedded JavaScript detected", Severity.High));
        }

        if (ContainsAny(text, new[] { "/OpenAction", "/AA" }))
        {
            findings.Add(NewFinding("actions.open_action", "OpenAction or Additional Actions detected", Severity.Medium));
        }

        if (ContainsAny(text, new[] { "/Launch" }))
        {
            findings.Add(NewFinding("actions.launch", "Launch action detected", Severity.High));
        }

        if (text.Contains("/EmbeddedFiles", StringComparison.OrdinalIgnoreCase))
        {
            findings.Add(NewFinding("embedded.files", "Embedded files detected", Severity.High));
        }

        if (text.Contains("/Encrypt", StringComparison.OrdinalIgnoreCase))
        {
            findings.Add(NewFinding("encryption.present", "PDF encryption present", Severity.Medium));
        }

        var startXrefCount = CountOccurrences(text, "startxref");
        signals["startxref_count"] = startXrefCount;

        if (startXrefCount == 2 && producer != null && producer.Contains("microsoft") && (producer.Contains("word") || producer.Contains("office")))
            startXrefCount = 1;
        if (startXrefCount > 1)
        {
                    
            findings.Add(NewFinding("structure.incremental_updates", $"Multiple startxref entries ({startXrefCount})", Severity.Medium));
        }

        var objCount = CountOccurrences(text, " obj");
        var endObjCount = CountOccurrences(text, "endobj");
        signals["object_count"] = objCount;
        signals["endobj_count"] = endObjCount;
        if (Math.Abs(objCount - endObjCount) > 10)
        {
            findings.Add(NewFinding("structure.object_mismatch", "Object/endobj count mismatch", Severity.Medium));
        }

        if (ContainsAny(text, new[] { "/JBIG2Decode", "/JPXDecode", "/Crypt" }))
        {
            findings.Add(NewFinding("structure.suspicious_filters", "Suspicious filters detected", Severity.Medium));
        }

        var hasImages = ContainsAny(text, new[] { "/Image", "/Subtype/Image", "/XObject" });
        var hasFonts = text.Contains("/Font", StringComparison.OrdinalIgnoreCase);
        var hasTextObjects = TextObjectRegex.IsMatch(text);
        signals["has_images"] = hasImages;
        signals["has_fonts"] = hasFonts;
        signals["has_text_objects"] = hasTextObjects;
        if (hasImages && (!hasFonts || !hasTextObjects))
        {
            findings.Add(NewFinding("content.scanned_or_screenshot", "Document appears to be scanned or a screenshot (image-heavy with little/no text)", Severity.Low));
        }

        foreach (var keyword in _options.SuspiciousKeywords)
        {
            if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                findings.Add(NewFinding("content.keyword", $"Suspicious keyword found: {keyword}", Severity.Low, keyword));
            }
        }

        return new FraudReport
        {
            Findings = findings,
            Signals = signals
        };
    }

    private static string? ExtractInfo(string text, string key)
    {
        foreach (Match match in InfoRegex.Matches(text))
        {
            if (string.Equals(match.Groups[1].Value, key, StringComparison.OrdinalIgnoreCase))
            {
                return DecodePdfString(match.Groups[2].Value);
            }
        }
        return null;
    }

    private static string DecodePdfString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var bytes = Encoding.Latin1.GetBytes(value);
        if (bytes.Length >= 2)
        {
            if (bytes[0] == 0xFE && bytes[1] == 0xFF)
            {
                return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
            }
            if (bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
            }
        }

        return value;
    }

    private static bool ContainsAny(string text, IEnumerable<string> needles)
        => needles.Any(n => text.Contains(n, StringComparison.OrdinalIgnoreCase));

    private static int CountOccurrences(string text, string needle)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(needle, index, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            count++;
            index += needle.Length;
        }
        return count;
    }

    private static ScanFinding NewFinding(string code, string message, Severity severity, string? evidence = null)
        => new() { Code = code, Message = message, Severity = severity, Evidence = evidence };
}
