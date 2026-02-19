using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PdfTool.Application.Interfaces;

namespace PdfTool.Infrastructure.Storage;

public sealed class InMemoryFileStorage : IFileStorage
{
    private readonly ConcurrentDictionary<string, byte[]> _store = new();

    public Task<string> SaveAsync(string fileName, byte[] bytes, CancellationToken ct)
    {
        var key = $"{System.Guid.NewGuid():N}-{fileName}";
        _store[key] = bytes;
        return Task.FromResult(key);
    }

    public Task<byte[]?> GetAsync(string key, CancellationToken ct)
    {
        _store.TryGetValue(key, out var bytes);
        return Task.FromResult(bytes);
    }
}

public sealed class LocalDiskFileStorage : IFileStorage
{
    private readonly string _basePath;

    public LocalDiskFileStorage(string basePath)
    {
        _basePath = basePath;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(string fileName, byte[] bytes, CancellationToken ct)
    {
        var key = $"{System.Guid.NewGuid():N}-{fileName}";
        var path = Path.Combine(_basePath, key);
        await File.WriteAllBytesAsync(path, bytes, ct);
        return key;
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken ct)
    {
        var path = Path.Combine(_basePath, key);
        if (!File.Exists(path)) return null;
        return await File.ReadAllBytesAsync(path, ct);
    }
}
