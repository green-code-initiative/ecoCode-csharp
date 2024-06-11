using EcoCode.Analyzers;
using EcoCode.Tool.Library.Models;
using EcoCode.Tool.Library.Reports;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;

namespace EcoCode.Tool.Library.Commands;

internal sealed class AnalyzeCommand(Tool.Delegates delegates) : AsyncCommand<AnalyzeSettings>
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
        var report = new HtmlReport(); // TODO : options

        if (settings.SourceType is SourceType.Solution)
        {
            Solution solution;
            try
            {
                solution = await delegates.OpenSolutionAsync(settings.Source);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine($"[red]Cannot load the provided solution: {ex.Message}[/]");
                return 1;
            }

            var analyzers = LoadAnalyzers();
            foreach (var project in solution.Projects)
                await AnalyzeProject(project, analyzers, report);
        }
        else // options.SourceType is SourceType.Project
        {
            Project project;
            try
            {
                project = await delegates.OpenProjectAsync(settings.Source);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine($"[red]Cannot load the provided project: {ex.Message}[/]");
                return 1;
            }
            await AnalyzeProject(project, LoadAnalyzers(), report);
        }

        report.WriteToFile(settings.Output!);
        return 0;
    }

    private static async Task AnalyzeProject(Project project, ImmutableArray<DiagnosticAnalyzer> analyzers, IAnalyzerReport report)
    {
        AnsiConsole.WriteLine($"[orange]Analyzing[/] project {project.Name}...");

        if (await project.GetCompilationAsync() is not { } compilation)
        {
            AnsiConsole.WriteLine($"[red]Unable to load the project {project.Name} compilation, skipping.[/]");
            return;
        }

        foreach (var diagnostic in await compilation!.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync())
            report.Add(DiagnosticInfo.FromDiagnostic(diagnostic));

        AnsiConsole.WriteLine($"[green]Analysis complete[/] for project {project.Name}");
    }

    private static ImmutableArray<DiagnosticAnalyzer> LoadAnalyzers() // TODO : options
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
