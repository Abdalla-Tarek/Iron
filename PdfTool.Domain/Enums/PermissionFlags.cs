using System;

namespace PdfTool.Domain.Enums;

[Flags]
public enum PermissionFlags
{
    None = 0,
    Print = 1 << 0,
    Copy = 1 << 1,
    Edit = 1 << 2,
    Annotate = 1 << 3,
    FillForms = 1 << 4,
    All = Print | Copy | Edit | Annotate | FillForms
}
