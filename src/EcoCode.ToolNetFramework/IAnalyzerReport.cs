namespace EcoCode.ToolNetFramework;

internal interface IAnalyzerReport
{
    void Add(DiagnosticInfo diagnosticInfo);

    void Generate(string outputPath);
}
