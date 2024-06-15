using EcoCode.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EcoCode.Tool.Core.Services;

internal static class AnalyzeService
{
    public static async Task<Solution?> OpenSolutionAsync(Tool.Workspace workspace, string solutionFilePath)
    {
        try
        {
            return await workspace.OpenSolutionAsync(solutionFilePath);
        }
        catch (Exception ex)
        {
            Tool.WriteLine($"Cannot load the provided solution: {ex.Message}", "red");
            return null;
        }
    }

    public static async Task<Project?> OpenProjectAsync(Tool.Workspace workspace, string projectFilePath)
    {
        try
        {
            return await workspace.OpenProjectAsync(projectFilePath);
        }
        catch (Exception ex)
        {
            Tool.WriteLine($"Cannot load the provided project: {ex.Message}", "red");
            return null;
        }
    }

    public static async Task AnalyzeProject(Project project, ImmutableArray<DiagnosticAnalyzer> analyzers, IAnalysisReport report)
    {
        Tool.WriteLine($"Analyzing project {project.Name}...", "darkorange");

        if (await project.GetCompilationAsync() is not { } compilation)
        {
            Tool.WriteLine($"Unable to load the project {project.Name} compilation, skipping.", "red");
            return;
        }

        foreach (var diagnostic in await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync())
            report.Add(DiagnosticInfo.FromDiagnostic(diagnostic));

        Tool.WriteLine($"Analysis complete for project {project.Name}", "green");
    }

    public static ImmutableArray<DiagnosticAnalyzer> LoadAnalyzers() // TODO : options
    {
        var analyzers = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>(64);
        foreach (var type in typeof(DontCallFunctionsInLoopConditions).Assembly.GetTypes())
        {
            if (type.IsSealed && type.IsSubclassOf(typeof(DiagnosticAnalyzer)))
                analyzers.Add((DiagnosticAnalyzer)Activator.CreateInstance(type));
        }
        return analyzers.ToImmutableArray();
    }
}
