using System.Collections.Generic;
using PdfTool.Domain.Enums;

namespace PdfTool.Domain.Models;

public sealed class ScanFinding
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public required Severity Severity { get; init; }
    public string? Evidence { get; init; }
}

public sealed class PdfScanReport
{
    public List<ScanFinding> Findings { get; init; } = new();
    public Dictionary<string, object?> Metadata { get; init; } = new();
}

public sealed class FraudReport
{
    public List<ScanFinding> Findings { get; init; } = new();
    public Dictionary<string, object?> Signals { get; init; } = new();
}

public sealed class PdfSanitizeReport
{
    public required PdfScanReport Before { get; init; }
    public required PdfScanReport After { get; init; }
}

public sealed class CompressResult
{
    public required BinaryContent Pdf { get; init; }
    public long OriginalSize { get; init; }
    public long FinalSize { get; init; }
    public double Ratio => OriginalSize == 0 ? 0 : (double)FinalSize / OriginalSize;
}
