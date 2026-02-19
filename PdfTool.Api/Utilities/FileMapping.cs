using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PdfTool.Api.Models.Requests;
using PdfTool.Application.DTOs.Requests;

namespace PdfTool.Api.Utilities;

public static class FileMapping
{
    public static async Task<FileInput> FromFormFileAsync(IFormFile file)
    {
        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return new FileInput
        {
            Bytes = ms.ToArray(),
            FileName = file.FileName,
            ContentType = file.ContentType
        };
    }

    public static FileInput FromBase64(Base64File file)
    {
        var bytes = Convert.FromBase64String(file.Base64);
        return new FileInput
        {
            Bytes = bytes,
            FileName = file.FileName,
            ContentType = file.ContentType
        };
    }
}
