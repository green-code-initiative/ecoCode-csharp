using EcoCode.Tool.Reports;
using Microsoft.CodeAnalysis.MSBuild;
using System.Diagnostics.CodeAnalysis;

namespace EcoCode.Tool.Commands;

[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "General error handling needed here")]
internal sealed class AnalyzeCommand : AsyncCommand<AnalyzeSettings>
{
    public override ValidationResult Validate(CommandContext context, AnalyzeSettings settings)
    {
        if (!File.Exists(settings.Source))
            return ValidationResult.Error($"The source file {settings.Source} does not exist.");

        if (Path.GetDirectoryName(settings.Output) is { } outputDir)
        {
            try
            {
                _ = Directory.CreateDirectory(outputDir);
            }
            catch (Exception ex)
            {
                return ValidationResult.Error($"The output directory {outputDir} cannot be opened or created: {ex.Message}");
            }
        }

        return base.Validate(context, settings);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, AnalyzeSettings settings)
    {
        using var workspace = MSBuildWorkspace.Create();
        workspace.WorkspaceFailed += (sender, e) => Program.WriteLine(e.Diagnostic.Message, "red");

        IAnalysisReport report;
        if (settings.SourceType is SourceType.Solution)
        {
            Solution solution;
            try
            {
                solution = await workspace.OpenSolutionAsync(settings.Source).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Program.WriteLine($"Cannot load the provided solution: {ex.Message}", "red");
                Program.WriteLine(ex.StackTrace!);
                return 1;
            }

            var analizers = settings.GetActiveAnalyzers();
            report = AnalysisReport.Create(settings.OutputType);
            foreach (var project in solution.Projects)
                await AnalyzeProject(project, analizers, report).ConfigureAwait(false);
        }
        else // options.SourceType is SourceType.Project
        {
            Project project;
            try
            {
                project = await workspace.OpenProjectAsync(settings.Source).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Program.WriteLine($"Cannot load the provided project: {ex.Message}", "red");
                Program.WriteLine(ex.StackTrace!);
                return 1;
            }

            var analizers = settings.GetActiveAnalyzers();
            report = AnalysisReport.Create(settings.OutputType);
            await AnalyzeProject(project, analizers, report).ConfigureAwait(false);
        }

        report.WriteToFile(settings.Output!);
        return 0;
    }

    private static async Task AnalyzeProject(Project project, ImmutableArray<DiagnosticAnalyzer> analyzers, IAnalysisReport report)
    {
        Program.WriteLine($"Analyzing project {project.Name}...", "darkorange");

        if (await project.GetCompilationAsync().ConfigureAwait(false) is not { } compilation)
        {
            Program.WriteLine($"Unable to load the project {project.Name} compilation, skipping.", "red");
            return;
        }

        foreach (var diagnostic in await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync().ConfigureAwait(false))
            report.Add(DiagnosticInfo.FromDiagnostic(diagnostic));

        Program.WriteLine($"Analysis complete for project {project.Name}", "green");
    }
}
