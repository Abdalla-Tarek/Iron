using PdfTool.Application.Interfaces;

namespace PdfTool.Infrastructure.Security;

public sealed class FileTypeValidator : IFileTypeValidator
{
    public bool IsPdf(byte[] bytes)
    {
        if (bytes.Length < 4) return false;
        return bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46; // %PDF
    }

    public bool IsImage(byte[] bytes)
    {
        return IsPng(bytes) || IsJpeg(bytes) || IsGif(bytes) || IsBmp(bytes) || IsTiff(bytes);
    }

    private static bool IsPng(byte[] b) => b.Length > 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47;
    private static bool IsJpeg(byte[] b) => b.Length > 3 && b[0] == 0xFF && b[1] == 0xD8 && b[^2] == 0xFF && b[^1] == 0xD9;
    private static bool IsGif(byte[] b) => b.Length > 6 && b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46;
    private static bool IsBmp(byte[] b) => b.Length > 2 && b[0] == 0x42 && b[1] == 0x4D;
    private static bool IsTiff(byte[] b) => b.Length > 4 && ((b[0] == 0x49 && b[1] == 0x49) || (b[0] == 0x4D && b[1] == 0x4D));
}
