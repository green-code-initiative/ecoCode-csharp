namespace EcoCode.Tool.Core.Reports;

internal abstract class BaseAnalysisReport : IAnalysisReport
{
    protected List<DiagnosticInfo> Diagnostics { get; } = [];

    protected BaseAnalysisReport()
    {
    }

    public void Add(DiagnosticInfo diagnostic) => Diagnostics.Add(diagnostic);

    public void WriteToFile(string outputPath)
    {
        using var writer = new StreamWriter(outputPath, append: false);
        WriteToStream(writer);
    }

    protected abstract void WriteToStream(StreamWriter writer);
}
