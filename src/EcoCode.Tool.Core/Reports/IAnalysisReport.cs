namespace EcoCode.Tool.Core.Reports;

internal interface IAnalysisReport
{
    void Add(DiagnosticInfo diagnosticInfo);

    void WriteToFile(string outputPath);
}
