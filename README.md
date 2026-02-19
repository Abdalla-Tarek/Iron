# PdfTool (IronPDF + IronOCR) Clean Architecture API

**Projects**
- `PdfTool.Api` (presentation)
- `PdfTool.Application` (DTOs, interfaces, validators, services)
- `PdfTool.Domain` (entities/enums/result models)
- `PdfTool.Infrastructure` (IronPDF/IronOCR implementations, storage)
- `PdfTool.Tests` (unit tests)

## Create Solution (dotnet CLI)
```bash
mkdir PdfTool
cd PdfTool

dotnet new sln -n PdfTool

dotnet new webapi -n PdfTool.Api

dotnet new classlib -n PdfTool.Application

dotnet new classlib -n PdfTool.Domain

dotnet new classlib -n PdfTool.Infrastructure

dotnet new xunit -n PdfTool.Tests

# Add projects to solution
 dotnet sln PdfTool.slnx add PdfTool.Api/PdfTool.Api.csproj \
  PdfTool.Application/PdfTool.Application.csproj \
  PdfTool.Domain/PdfTool.Domain.csproj \
  PdfTool.Infrastructure/PdfTool.Infrastructure.csproj \
  PdfTool.Tests/PdfTool.Tests.csproj

# Project references
 dotnet add PdfTool.Application/PdfTool.Application.csproj reference PdfTool.Domain/PdfTool.Domain.csproj
 dotnet add PdfTool.Infrastructure/PdfTool.Infrastructure.csproj reference PdfTool.Application/PdfTool.Application.csproj PdfTool.Domain/PdfTool.Domain.csproj
 dotnet add PdfTool.Api/PdfTool.Api.csproj reference PdfTool.Application/PdfTool.Application.csproj PdfTool.Infrastructure/PdfTool.Infrastructure.csproj PdfTool.Domain/PdfTool.Domain.csproj
 dotnet add PdfTool.Tests/PdfTool.Tests.csproj reference PdfTool.Application/PdfTool.Application.csproj PdfTool.Domain/PdfTool.Domain.csproj PdfTool.Infrastructure/PdfTool.Infrastructure.csproj
```

## Run
```bash
dotnet build

dotnet run --project PdfTool.Api/PdfTool.Api.csproj
```

Swagger UI
- `https://localhost:5001/swagger` (development only)
Swagger shows request/response schemas. Detailed multipart and base64 examples are included below.

Health
- `GET /health`

## Configuration
`PdfTool.Api/appsettings.json`
- `IronPdfLicenseKey`
- `IronOcrLicenseKey`
- `ApiKey`
- `Security:RequireApiKey`
- `FraudDetection:SuspiciousProducers`
- `FraudDetection:SuspiciousKeywords`
- `MaxUploadMB`
- `FileStorage:Mode` (`InMemory` or `LocalDisk`)
- `FileStorage:Path`

## Endpoints
- `POST /api/pdf/create/html`
- `POST /api/pdf/watermark`
- `POST /api/pdf/compress`
- `POST /api/pdf/encrypt`
- `POST /api/pdf/convert/pdfa`
- `POST /api/pdf/export/png`
- `POST /api/pdf/pages/remove`
- `POST /api/pdf/merge`
- `POST /api/pdf/from-image`
- `POST /api/pdf/sign/pfx-text`
- `POST /api/pdf/sign/pfx-image`
- `POST /api/pdf/sign/stamp`
- `POST /api/pdf/scan`
- `POST /api/pdf/scan/generate-js`
- `POST /api/pdf/sanitize`
- `POST /api/ocr/extract`

## Curl Examples
All requests accept:
- `multipart/form-data`
- JSON with base64

All requests support:
- `ReturnBase64` to return JSON base64 instead of binary

### 1) HTML to PDF
Multipart
```bash
curl -X POST https://localhost:5001/api/pdf/create/html \
  -H "X-API-KEY: change-me" \
  -F "Html=<h1>Hello</h1>" \
  -F "ReturnBase64=false"
```
JSON
```bash
curl -X POST https://localhost:5001/api/pdf/create/html \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: change-me" \
  -d '{"html":"<h1>Hello</h1>","returnBase64":true}'
```

### 2) Watermark
Multipart
```bash
curl -X POST https://localhost:5001/api/pdf/watermark \
  -H "X-API-KEY: change-me" \
  -F "PdfFile=@input.pdf" \
  -F "Text=CONFIDENTIAL"
```
JSON
```bash
curl -X POST https://localhost:5001/api/pdf/watermark \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: change-me" \
  -d '{"pdfFile":{"base64":"<BASE64>","fileName":"input.pdf","contentType":"application/pdf"},"text":"CONFIDENTIAL"}'
```

### 3) Compress
Multipart
```bash
curl -X POST https://localhost:5001/api/pdf/compress \
  -H "X-API-KEY: change-me" \
  -F "PdfFile=@input.pdf" \
  -F "CompressionProfile=BestSizeMultiPass"
```
JSON
```bash
curl -X POST https://localhost:5001/api/pdf/compress \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: change-me" \
  -d '{"pdfFile":{"base64":"<BASE64>","fileName":"input.pdf","contentType":"application/pdf"},"compressionProfile":"Balanced","returnBase64":true}'
```

### 4) Encrypt + Permissions
Multipart
```bash
curl -X POST https://localhost:5001/api/pdf/encrypt \
  -H "X-API-KEY: change-me" \
  -F "PdfFile=@input.pdf" \
  -F "OwnerPassword=secret" \
  -F "Permissions=Print,Copy"
```
JSON
```bash
curl -X POST https://localhost:5001/api/pdf/encrypt \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: change-me" \
  -d '{"pdfFile":{"base64":"<BASE64>","fileName":"input.pdf","contentType":"application/pdf"},"ownerPassword":"secret"}'
```

### 5) Convert to PDF/A
Multipart
```bash
curl -X POST https://localhost:5001/api/pdf/convert/pdfa \
  -H "X-API-KEY: change-me" \
  -F "PdfFile=@input.pdf" \
  -F "PdfAType=PDFA2b"
```
JSON
```bash
curl -X POST https://localhost:5001/api/pdf/convert/pdfa \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: change-me" \
  -d '{"pdfFile":{"base64":"<BASE64>","fileName":"input.pdf","contentType":"application/pdf"},"pdfAType":"PDFA1b"}'
```

### 6) PDF to PNG
Multipart
```bash
curl -X POST https://localhost:5001/api/pdf/export/png \
  -H "X-API-KEY: change-me" \
  -F "PdfFile=@input.pdf" \
  -F "ZipOutput=true"
```
JSON
```bash
curl -X POST https://localhost:5001/api/pdf/export/png \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: change-me" \
  -d '{"pdfFile":{"base64":"<BASE64>","fileName":"input.pdf","contentType":"application/pdf"},"zipOutput":false}'
```

### 7) Remove Page
Multipart
```bash
curl -X POST https://localhost:5001/api/pdf/pages/remove \
  -H "X-API-KEY: change-me" \
  -F "PdfFile=@input.pdf" \
  -F "PageNumber=1"
```
JSON
```bash
curl -X POST https://localhost:5001/api/pdf/pages/remove \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: change-me" \
  -d '{"pdfFile":{"base64":"<BASE64>","fileName":"input.pdf","contentType":"application/pdf"},"pageNumber":1}'
```

### 8) Merge PDFs
Multipart
```bash
curl -X POST https://localhost:5001/api/pdf/merge \
  -H "X-API-KEY: change-me" \
  -F "Main=@main.pdf" \
  -F "Append=@append.pdf"
```
JSON
```bash
curl -X POST https://localhost:5001/api/pdf/merge \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: change-me" \
  -d '{"main":{"base64":"<BASE64>","fileName":"main.pdf","contentType":"application/pdf"},"append":{"base64":"<BASE64>","fileName":"append.pdf","contentType":"application/pdf"}}'
```

### 9) Image to PDF + Compress
Multipart
```bash
curl -X POST https://localhost:5001/api/pdf/from-image \
  -H "X-API-KEY: change-me" \
  -F "ImageFile=@photo.jpg"
```
JSON
```bash
curl -X POST https://localhost:5001/api/pdf/from-image \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: change-me" \
  -d '{"imageFile":{"base64":"<BASE64>","fileName":"photo.jpg","contentType":"image/jpeg"},"compressAfter":true}'
```

### 10) Digital Sign (PFX) – Visible Text
Multipart
```bash
curl -X POST https://localhost:5001/api/pdf/sign/pfx-text \
  -H "X-API-KEY: change-me" \
  -F "PdfFile=@input.pdf" \
  -F "PfxFile=@cert.pfx" \
  -F "PfxPassword=secret" \
  -F "VisibleText=Signed" \
  -F "PageNumber=1" \
  -F "X=50" -F "Y=50" -F "Width=200" -F "Height=60"
```
JSON
```bash
curl -X POST https://localhost:5001/api/pdf/sign/pfx-text \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: change-me" \
  -d '{"pdfFile":{"base64":"<BASE64>","fileName":"input.pdf","contentType":"application/pdf"},"pfxFile":{"base64":"<BASE64>","fileName":"cert.pfx","contentType":"application/x-pkcs12"},"pfxPassword":"secret","visibleText":"Signed","pageNumber":1,"x":50,"y":50,"width":200,"height":60}'
```

### 11) Digital Sign (PFX) – Visible Image
Multipart
```bash
curl -X POST https://localhost:5001/api/pdf/sign/pfx-image \
  -H "X-API-KEY: change-me" \
  -F "PdfFile=@input.pdf" \
  -F "PfxFile=@cert.pfx" \
  -F "PfxPassword=secret" \
  -F "ImageAppearanceFile=@signature.png" \
  -F "PageNumber=1" \
  -F "X=50" -F "Y=50" -F "Width=200" -F "Height=60"
```
JSON
```bash
curl -X POST https://localhost:5001/api/pdf/sign/pfx-image \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: change-me" \
  -d '{"pdfFile":{"base64":"<BASE64>","fileName":"input.pdf","contentType":"application/pdf"},"pfxFile":{"base64":"<BASE64>","fileName":"cert.pfx","contentType":"application/x-pkcs12"},"pfxPassword":"secret","imageAppearanceFile":{"base64":"<BASE64>","fileName":"signature.png","contentType":"image/png"},"pageNumber":1,"x":50,"y":50,"width":200,"height":60}'
```

### 12) Signature Image Only (Stamp)
Multipart
```bash
curl -X POST https://localhost:5001/api/pdf/sign/stamp \
  -H "X-API-KEY: change-me" \
  -F "PdfFile=@input.pdf" \
  -F "SignatureImageFile=@signature.png"
```
JSON
```bash
curl -X POST https://localhost:5001/api/pdf/sign/stamp \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: change-me" \
  -d '{"pdfFile":{"base64":"<BASE64>","fileName":"input.pdf","contentType":"application/pdf"},"signatureImageFile":{"base64":"<BASE64>","fileName":"signature.png","contentType":"image/png"}}'
```

### 13) Cleaner Scan + JS Test PDF
Generate JS test PDF
```bash
curl -X POST https://localhost:5001/api/pdf/scan/generate-js \
  -H "X-API-KEY: change-me" \
  -F "JsCode=app.alert('test');"
```
Scan
```bash
curl -X POST https://localhost:5001/api/pdf/scan \
  -H "X-API-KEY: change-me" \
  -F "PdfFile=@input.pdf"
```

### 14) Sanitize + Re-scan
Multipart
```bash
curl -X POST https://localhost:5001/api/pdf/sanitize \
  -H "X-API-KEY: change-me" \
  -F "PdfFile=@input.pdf" \
  -F "SanitizeMode=RasterizeAllPages"
```
JSON
```bash
curl -X POST https://localhost:5001/api/pdf/sanitize \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: change-me" \
  -d '{"pdfFile":{"base64":"<BASE64>","fileName":"input.pdf","contentType":"application/pdf"},"sanitizeMode":"RasterizeAllPages","returnBase64":true}'
```

### OCR Extract
Multipart
```bash
curl -X POST https://localhost:5001/api/ocr/extract \
  -H "X-API-KEY: change-me" \
  -F "File=@id-card.jpg"
```
JSON
```bash
curl -X POST https://localhost:5001/api/ocr/extract \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: change-me" \
  -d '{"file":{"base64":"<BASE64>","fileName":"id.jpg","contentType":"image/jpeg"}}'
```

## Licensing Notes
IronPDF and IronOCR require valid licenses for production use. Set `IronPdfLicenseKey` and `IronOcrLicenseKey` in `appsettings.json` or environment variables. The API will still run without keys but may have trial limitations.
