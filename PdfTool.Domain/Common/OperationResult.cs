using System.Collections.Generic;

namespace PdfTool.Domain.Common;

public sealed class OperationResult<T>
{
    public bool Success { get; private set; }
    public List<OperationError> Errors { get; } = new();
    public List<OperationWarning> Warnings { get; } = new();
    public T? Data { get; private set; }
    public Dictionary<string, object?> Metadata { get; } = new();

    public static OperationResult<T> Ok(T data)
    {
        return new OperationResult<T>
        {
            Success = true,
            Data = data
        };
    }

    public static OperationResult<T> Fail(params OperationError[] errors)
    {
        var result = new OperationResult<T> { Success = false };
        result.Errors.AddRange(errors);
        return result;
    }

    public OperationResult<T> WithWarning(string code, string message)
    {
        Warnings.Add(new OperationWarning(code, message));
        return this;
    }

    public OperationResult<T> WithMetadata(string key, object? value)
    {
        Metadata[key] = value;
        return this;
    }
}
