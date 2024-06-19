namespace EcoCode.Tool.Reports;

internal abstract class AnalysisReport : IAnalysisReport
{
    protected List<DiagnosticInfo> Diagnostics { get; } = [];

    protected AnalysisReport()
    {
    }

    public void Add(DiagnosticInfo diagnostic) => Diagnostics.Add(diagnostic);

    public void WriteToFile(string outputPath)
    {
        using var writer = new StreamWriter(outputPath, append: false);
        if (Diagnostics.Count != 0) WriteToStream(writer);
    }

    protected abstract void WriteToStream(StreamWriter writer);

    public static IAnalysisReport Create(OutputType outputType) => outputType switch
    {
        OutputType.Html => new HtmlAnalysisReport(),
        OutputType.Json => new JsonAnalysisReport(),
        OutputType.Csv => new CsvAnalysisReport(),
        _ => throw new NotSupportedException()
    };
}
