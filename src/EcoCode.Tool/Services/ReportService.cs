namespace EcoCode.Tool.Services;

internal static partial class ReportService
{
    public static async Task GenerateReportAsync(List<DiagnosticInfo> diagnostics, string outputPath, OutputType outputType)
    {
        await using var writer = new StreamWriter(outputPath, append: false);

        switch (outputType)
        {
            case OutputType.Html: await Html.WriteToStreamAsync(writer, diagnostics); break;
            case OutputType.Json: await Json.WriteToStreamAsync(writer, diagnostics); break;
            case OutputType.Csv: await Csv.WriteToStreamAsync(writer, diagnostics); break;
            default: throw new NotSupportedException();
        }
    }
}
