namespace EcoCode.Tool.Core.Reports;

internal sealed class CsvAnalysisReport : AnalysisReport
{
    private const string Header = "Directory;File;Location;Severity;Code;Message";

    protected override void WriteToStream(StreamWriter writer)
    {
        writer.WriteLine(Header);
        foreach (var diag in Diagnostics)
            writer.WriteLine($"{diag.Directory};{diag.File};{diag.Location};{diag.Severity};{diag.Code};{diag.Message}");
    }
}
