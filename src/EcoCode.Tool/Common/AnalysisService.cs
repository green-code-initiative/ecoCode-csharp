using EcoCode.Analyzers;
using EcoCode.Tool.Reports;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EcoCode.Tool.Common;

internal sealed partial class AnalysisService
{
    private readonly ImmutableArray<DiagnosticAnalyzer> _analyzers;

    private readonly AnalyzerOptions _analyzerOptions;

    private AnalysisService(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions analyzerOptions)
    {
        _analyzers = analyzers;
        _analyzerOptions = analyzerOptions;
    }

    [GeneratedRegex(@"dotnet_diagnostic\.(.+)\.")]
    private static partial Regex ParseAnalyzersRegex();

    public static async Task<AnalysisService> CreateFromConfigFileAsync()
    {
        var globalConfig = await AdditionalFile.LoadGlobalConfigAsync();

        var analyzers = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();

        analyzers.AddRange(GetEcoCodeAnalyzers());
        int ecoCodeAnalyzersCount = analyzers.Count;

        analyzers.AddRange(GetRoslynAnalyzers(globalConfig.Text));
        Program.WriteLine($"Using {ecoCodeAnalyzersCount} EcoCode analyzers and {analyzers.Count - ecoCodeAnalyzersCount} customized Roslyn analyzers");

        return new(analyzers.DrainToImmutable(), new AnalyzerOptions([globalConfig], await CustomAnalyzerConfigOptionsProvider.LoadGlobalConfigAsync()));

        static IEnumerable<DiagnosticAnalyzer> GetEcoCodeAnalyzers() =>
            typeof(DontCallFunctionsInLoopConditions).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(DiagnosticAnalyzer)))
            .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type)!);

        static IEnumerable<DiagnosticAnalyzer> GetRoslynAnalyzers(string config)
        {
            var roslynAnalyzerIds = new HashSet<string>();
            foreach (Match match in ParseAnalyzersRegex().Matches(config))
            {
                if (match.Groups.Count == 2)
                    _ = roslynAnalyzerIds.Add(match.Groups[1].Value);
            }

            var roslynAnalyzerTypes = Assembly.Load("Microsoft.CodeAnalysis.NetAnalyzers").GetTypes()
                .Union(Assembly.Load("Microsoft.CodeAnalysis.CSharp.NetAnalyzers").GetTypes())
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(DiagnosticAnalyzer)));

            foreach (var roslynAnalyzerType in roslynAnalyzerTypes)
            {
                var roslynAnalyzer = (DiagnosticAnalyzer)Activator.CreateInstance(roslynAnalyzerType)!;
                foreach (var supportedDiagnostic in roslynAnalyzer.SupportedDiagnostics)
                {
                    if (roslynAnalyzerIds.Contains(supportedDiagnostic.Id))
                    {
                        yield return roslynAnalyzer;
                        break;
                    }
                }
            }
        }
    }

    public async Task AnalyzeProjectAsync(Project project, DiagnosticSeverity minSeverity, IAnalysisReport report)
    {
        Program.WriteLine($"Analyzing project {project.Name}...", "darkorange");

        if (await project.GetCompilationAsync().ConfigureAwait(false) is not { } compilation)
        {
            Program.WriteLine($"Unable to load the project {project.Name} compilation, skipping.", "red");
            return;
        }

        var compilationWithAnalyzers = compilation.WithAnalyzers(_analyzers, _analyzerOptions);
        foreach (var diagnostic in await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().ConfigureAwait(false))
        {
            if (diagnostic.Severity >= minSeverity)
                report.Add(DiagnosticInfo.FromDiagnostic(diagnostic));
            else
                Program.WriteLine($"Ignoring diagnostic {diagnostic.Id} with severity {diagnostic.Severity}");
        }

        Program.WriteLine($"Analysis complete for project {project.Name}", "green");
    }
}
