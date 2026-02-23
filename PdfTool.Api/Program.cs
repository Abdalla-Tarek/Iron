using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Http.Features;
using Serilog;
using PdfTool.Api.Middleware;
using PdfTool.Application.FraudDetection;
using PdfTool.Application.Interfaces;
using PdfTool.Application.Services;
using PdfTool.Application.Validators;
using PdfTool.Infrastructure.Security;
using PdfTool.Infrastructure.Services;
using PdfTool.Infrastructure.Storage;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .WriteTo.Console();
});

builder.Services.AddControllers();
builder.Services.AddDataProtection();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<HtmlToPdfRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

builder.Services.AddHealthChecks();

var maxUploadMb = builder.Configuration.GetValue<int>("MaxUploadMB", 50);
var maxUploadBytes = maxUploadMb * 1024L * 1024L;

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxUploadBytes;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = maxUploadBytes;
});

builder.Services.Configure<FraudDetectionOptions>(builder.Configuration.GetSection("FraudDetection"));
builder.Services.AddSingleton(sp =>
{
    var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<FraudDetectionOptions>>().Value;
    return new FraudDetector(opts);
});

builder.Services.AddScoped<IFileTypeValidator, FileTypeValidator>();

builder.Services.AddScoped<IPdfCreator, IronPdfCreator>();
builder.Services.AddScoped<IPdfWatermarker, IronPdfWatermarker>();
builder.Services.AddScoped<IPdfCompressor, IronPdfCompressor>();
builder.Services.AddScoped<IPdfEncryptor, IronPdfEncryptor>();
builder.Services.AddScoped<IPdfConverter, IronPdfConverter>();
builder.Services.AddScoped<IPdfEditor, IronPdfEditor>();
builder.Services.AddScoped<IPdfMerger, IronPdfMerger>();
builder.Services.AddScoped<IPdfSigner, IronPdfSigner>();
builder.Services.AddScoped<IPdfScanner, IronPdfScanner>();
builder.Services.AddScoped<IPdfSanitizer, IronPdfSanitizer>();
builder.Services.AddScoped<IOcrService, IronOcrService>();
builder.Services.AddScoped<IOcrFieldExtractor, RegexOcrFieldExtractor>();

builder.Services.AddScoped<IPdfAppService, PdfAppService>();
builder.Services.AddScoped<IPdfScanAppService, PdfScanAppService>();
builder.Services.AddScoped<IOcrAppService, OcrAppService>();

builder.Services.AddSingleton<IFileStorage>(sp =>
{
    var mode = builder.Configuration.GetValue<string>("FileStorage:Mode", "InMemory");
    if (string.Equals(mode, "LocalDisk", StringComparison.OrdinalIgnoreCase))
    {
        var path = builder.Configuration.GetValue<string>("FileStorage:Path", "./storage");
        return new LocalDiskFileStorage(path);
    }
    return new InMemoryFileStorage();
});

var app = builder.Build();

var ironPdfKey = builder.Configuration["IronPdfLicenseKey"];
if (!string.IsNullOrWhiteSpace(ironPdfKey))
{
    IronPdf.License.LicenseKey = ironPdfKey;
}

var ironOcrKey = builder.Configuration["IronOcrLicenseKey"];
if (!string.IsNullOrWhiteSpace(ironOcrKey))
{
    IronOcr.License.LicenseKey = ironOcrKey;
    IronQr.License.LicenseKey = ironOcrKey;
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
