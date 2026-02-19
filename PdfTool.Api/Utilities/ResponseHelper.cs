using System;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using PdfTool.Api.Models.Responses;
using PdfTool.Domain.Common;
using PdfTool.Domain.Models;

namespace PdfTool.Api.Utilities;

public static class ResponseHelper
{
    public static IActionResult FileOrBase64(OperationResult<BinaryContent> result)
    {
        if (!result.Success || result.Data == null)
        {
            return new BadRequestObjectResult(result);
        }

        if (result.Data.Metadata.TryGetValue("returnBase64", out var returnBase64Obj) && returnBase64Obj is true)
        {
            var base64 = Convert.ToBase64String(result.Data.Bytes);
            return new OkObjectResult(OperationResult<Base64BinaryResponse>.Ok(new Base64BinaryResponse
            {
                Base64 = base64,
                ContentType = result.Data.ContentType,
                FileName = result.Data.FileName
            }));
        }

        return new FileContentResult(result.Data.Bytes, result.Data.ContentType)
        {
            FileDownloadName = result.Data.FileName
        };
    }

    public static IActionResult PngResult(OperationResult<PageImagesResult> result, bool zipOutput)
    {
        if (!result.Success || result.Data == null)
        {
            return new BadRequestObjectResult(result);
        }

        if (!zipOutput)
        {
            var response = new Base64ImagesResponse();
            foreach (var img in result.Data.Images)
            {
                response.Images.Add(new Base64BinaryResponse
                {
                    Base64 = Convert.ToBase64String(img.Bytes),
                    ContentType = img.ContentType,
                    FileName = img.FileName
                });
            }
            return new OkObjectResult(OperationResult<Base64ImagesResponse>.Ok(response));
        }

        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            foreach (var img in result.Data.Images)
            {
                var entry = archive.CreateEntry(img.FileName, CompressionLevel.Fastest);
                using var entryStream = entry.Open();
                entryStream.Write(img.Bytes, 0, img.Bytes.Length);
            }
        }

        return new FileContentResult(ms.ToArray(), "application/zip")
        {
            FileDownloadName = "pages.zip"
        };
    }
}
