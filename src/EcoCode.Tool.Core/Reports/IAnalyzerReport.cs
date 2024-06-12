using EcoCode.Tool.Core.Models;

namespace EcoCode.Tool.Core.Reports;

internal interface IAnalyzerReport
{
    void Add(DiagnosticInfo diagnosticInfo);

    void WriteToFile(string outputPath);
}
