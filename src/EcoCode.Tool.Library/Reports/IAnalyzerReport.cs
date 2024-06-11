using EcoCode.Tool.Library.Models;

namespace EcoCode.Tool.Library.Reports;

internal interface IAnalyzerReport
{
    void Add(DiagnosticInfo diagnosticInfo);

    void WriteToFile(string outputPath);
}
