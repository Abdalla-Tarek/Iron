using System.Collections.Generic;

namespace PdfTool.Api.Models.Responses;

public sealed class Base64BinaryResponse
{
    public required string Base64 { get; init; }
    public required string ContentType { get; init; }
    public string FileName { get; init; } = "output";
}

public sealed class Base64ImagesResponse
{
    public List<Base64BinaryResponse> Images { get; init; } = new();
}

public sealed class DownloadLinkResponse
{
    public required string Url { get; init; }
    public required string ContentType { get; init; }
    public string FileName { get; init; } = "output";
}

public sealed class DownloadLinksResponse
{
    public List<DownloadLinkResponse> Files { get; init; } = new();
}
