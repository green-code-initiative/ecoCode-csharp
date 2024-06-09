namespace EcoCode.ToolNetFramework.Reports;

internal interface IAnalyzerReport
{
    void Add(DiagnosticInfo diagnosticInfo);

    void WriteToFile(string outputPath);
}
