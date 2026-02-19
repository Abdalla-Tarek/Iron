using System.Collections.Generic;

namespace PdfTool.Application.FraudDetection;

public sealed class FraudDetectionOptions
{
    public List<string> SuspiciousProducers { get; set; } = new();
    public List<string> SuspiciousKeywords { get; set; } = new();
}
