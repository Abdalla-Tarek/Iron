using System.Collections.Generic;

namespace PdfTool.Domain.Models;

public sealed class OcrResultModel
{
    public List<OcrField> Fields { get; init; } = new();
}

public sealed class OcrField
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}
