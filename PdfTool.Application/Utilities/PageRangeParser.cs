using System.Collections.Generic;
using System.Linq;

namespace PdfTool.Application.Utilities;

public static class PageRangeParser
{
    public static IReadOnlyList<int> Parse(string? range, int pageCount)
    {
        if (string.IsNullOrWhiteSpace(range))
        {
            return Enumerable.Range(0, pageCount).ToList();
        }

        var result = new HashSet<int>();
        var parts = range.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            if (part.Contains('-'))
            {
                var bounds = part.Split('-', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
                if (bounds.Length == 2 && int.TryParse(bounds[0], out var start) && int.TryParse(bounds[1], out var end))
                {
                    for (var i = start; i <= end; i++)
                    {
                        if (i >= 1 && i <= pageCount)
                        {
                            result.Add(i - 1);
                        }
                    }
                }
            }
            else if (int.TryParse(part, out var single))
            {
                if (single >= 1 && single <= pageCount)
                {
                    result.Add(single - 1);
                }
            }
        }

        return result.OrderBy(x => x).ToList();
    }
}
