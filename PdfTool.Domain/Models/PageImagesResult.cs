using System.Collections.Generic;

namespace PdfTool.Domain.Models;

public sealed class PageImagesResult
{
    public List<BinaryContent> Images { get; init; } = new();
}
