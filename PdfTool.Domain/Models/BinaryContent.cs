using System.Collections.Generic;

namespace PdfTool.Domain.Models;

public sealed class BinaryContent
{
    public required byte[] Bytes { get; init; }
    public required string ContentType { get; init; }
    public string FileName { get; init; } = "output";
    public Dictionary<string, object?> Metadata { get; init; } = new();
}
