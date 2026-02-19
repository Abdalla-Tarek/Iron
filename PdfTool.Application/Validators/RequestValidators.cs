using FluentValidation;
using PdfTool.Application.DTOs.Requests;

namespace PdfTool.Application.Validators;

public sealed class FileInputValidator : AbstractValidator<FileInput>
{
    public FileInputValidator()
    {
        RuleFor(x => x.Bytes).NotNull().Must(b => b.Length > 0).WithMessage("File content is required");
        RuleFor(x => x.FileName).NotEmpty();
    }
}

public sealed class HtmlToPdfRequestValidator : AbstractValidator<HtmlToPdfRequest>
{
    public HtmlToPdfRequestValidator()
    {
        RuleFor(x => x.Html).NotEmpty();
    }
}

public sealed class WatermarkRequestValidator : AbstractValidator<WatermarkRequest>
{
    public WatermarkRequestValidator()
    {
        RuleFor(x => x.PdfFile).SetValidator(new FileInputValidator());
        RuleFor(x => x.Text).NotEmpty();
        RuleFor(x => x.Opacity).InclusiveBetween(0, 100);
    }
}

public sealed class CompressRequestValidator : AbstractValidator<CompressRequest>
{
    public CompressRequestValidator()
    {
        RuleFor(x => x.PdfFile).SetValidator(new FileInputValidator());
    }
}

public sealed class EncryptRequestValidator : AbstractValidator<EncryptRequest>
{
    public EncryptRequestValidator()
    {
        RuleFor(x => x.PdfFile).SetValidator(new FileInputValidator());
        RuleFor(x => x.OwnerPassword).NotEmpty();
    }
}

public sealed class ConvertPdfARequestValidator : AbstractValidator<ConvertPdfARequest>
{
    public ConvertPdfARequestValidator()
    {
        RuleFor(x => x.PdfFile).SetValidator(new FileInputValidator());
    }
}

public sealed class PdfToPngRequestValidator : AbstractValidator<PdfToPngRequest>
{
    public PdfToPngRequestValidator()
    {
        RuleFor(x => x.PdfFile).SetValidator(new FileInputValidator());
        RuleFor(x => x.Dpi).InclusiveBetween(50, 600);
    }
}

public sealed class RemovePageRequestValidator : AbstractValidator<RemovePageRequest>
{
    public RemovePageRequestValidator()
    {
        RuleFor(x => x.PdfFile).SetValidator(new FileInputValidator());
        RuleFor(x => x.PageNumber).GreaterThan(0);
    }
}

public sealed class MergeRequestValidator : AbstractValidator<MergeRequest>
{
    public MergeRequestValidator()
    {
        RuleFor(x => x.Main).SetValidator(new FileInputValidator());
        RuleFor(x => x.Append).SetValidator(new FileInputValidator());
    }
}

public sealed class ImageToPdfRequestValidator : AbstractValidator<ImageToPdfRequest>
{
    public ImageToPdfRequestValidator()
    {
        RuleFor(x => x.ImageFile).SetValidator(new FileInputValidator());
    }
}

public sealed class SignPfxTextRequestValidator : AbstractValidator<SignPfxTextRequest>
{
    public SignPfxTextRequestValidator()
    {
        RuleFor(x => x.PdfFile).SetValidator(new FileInputValidator());
        RuleFor(x => x.PfxFile).SetValidator(new FileInputValidator());
        RuleFor(x => x.PfxPassword).NotEmpty();
    }
}

public sealed class SignPfxImageRequestValidator : AbstractValidator<SignPfxImageRequest>
{
    public SignPfxImageRequestValidator()
    {
        RuleFor(x => x.PdfFile).SetValidator(new FileInputValidator());
        RuleFor(x => x.PfxFile).SetValidator(new FileInputValidator());
        RuleFor(x => x.ImageAppearanceFile).SetValidator(new FileInputValidator());
        RuleFor(x => x.PfxPassword).NotEmpty();
    }
}

public sealed class StampSignatureRequestValidator : AbstractValidator<StampSignatureRequest>
{
    public StampSignatureRequestValidator()
    {
        RuleFor(x => x.PdfFile).SetValidator(new FileInputValidator());
        RuleFor(x => x.SignatureImageFile).SetValidator(new FileInputValidator());
        RuleFor(x => x.X).GreaterThanOrEqualTo(0).When(x => x.X.HasValue);
        RuleFor(x => x.Y).GreaterThanOrEqualTo(0).When(x => x.Y.HasValue);
    }
}

public sealed class GenerateJsTestPdfRequestValidator : AbstractValidator<GenerateJsTestPdfRequest>
{
    public GenerateJsTestPdfRequestValidator()
    {
        RuleFor(x => x.JsCode).NotEmpty();
    }
}

public sealed class ScanPdfRequestValidator : AbstractValidator<ScanPdfRequest>
{
    public ScanPdfRequestValidator()
    {
        RuleFor(x => x.PdfFile).SetValidator(new FileInputValidator());
    }
}

public sealed class SanitizePdfRequestValidator : AbstractValidator<SanitizePdfRequest>
{
    public SanitizePdfRequestValidator()
    {
        RuleFor(x => x.PdfFile).SetValidator(new FileInputValidator());
        RuleFor(x => x.Dpi).InclusiveBetween(50, 600);
    }
}

public sealed class OcrExtractRequestValidator : AbstractValidator<OcrExtractRequest>
{
    public OcrExtractRequestValidator()
    {
        RuleFor(x => x.File).SetValidator(new FileInputValidator());
    }
}
