using PdfTool.Domain.Common;

namespace PdfTool.Api.Utilities;

public static class ValidationHelper
{
    public static OperationResult<T> InvalidFile<T>(string message)
    {
        return OperationResult<T>.Fail(new OperationError("InvalidFile", message));
    }
}
