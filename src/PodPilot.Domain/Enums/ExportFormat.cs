namespace PodPilot.Domain.Enums;

/// <summary>
/// Export file formats for observability data.
/// </summary>
public enum ExportFormat
{
    /// <summary>Comma-separated values.</summary>
    Csv = 0,

    /// <summary>Excel-compatible spreadsheet (CSV-based).</summary>
    Excel = 1,

    /// <summary>JSON format.</summary>
    Json = 2,
}
