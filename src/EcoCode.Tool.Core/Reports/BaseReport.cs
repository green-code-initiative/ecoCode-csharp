using EcoCode.Tool.Core.Models;
using System.Collections.Generic;
using System.IO;

namespace EcoCode.Tool.Core.Reports;

internal abstract class BaseReport : IAnalyzerReport
{
    protected List<DiagnosticInfo> Diagnostics { get; } = [];

    protected BaseReport()
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
