using EcoCode.Analyzers;
using EcoCode.ToolNetFramework.Reports;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace EcoCode.ToolNetFramework.Commands;

internal sealed class AnalyzeCommand : Command<AnalyzeSettings>
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
                return ValidationResult.Error($"The output directory {outputDir} cannot be opened or created:{Environment.NewLine}{ex.Message}");
            }
        }

        return base.Validate(context, settings);
    }

    public override int Execute(CommandContext context, AnalyzeSettings settings)
    {
        if (MSBuildLocator.QueryVisualStudioInstances().FirstOrDefault() is not { } instance)
        {
            AnsiConsole.WriteLine("[red]No MSBuild instance was found, exiting.[/]");
            return 1;
        }

        AnsiConsole.WriteLine($"Using MSBuild found at {instance.MSBuildPath}.");
        MSBuildLocator.RegisterInstance(instance);

        using var workspace = MSBuildWorkspace.Create();
        workspace.WorkspaceFailed += (sender, e) => AnsiConsole.WriteLine($"[red]{e.Diagnostic.Message}[/]");

        var report = new HtmlReport(); // TODO : options

        if (options.SourceType is SourceType.Solution)
        {
            if (await workspace.OpenSolutionAsync(options.Source) is not { } solution)
                return "Cannot load the provided solution.";

            var analyzers = LoadAnalyzers();
            foreach (var project in solution.Projects)
                await AnalyzeProject(project, analyzers, report);
        }
        else // options.SourceType is SourceType.Project
        {
            if (await workspace.OpenProjectAsync(options.Source) is not { } project)
                return "Cannot load the provided project.";

            await AnalyzeProject(project, LoadAnalyzers(), report);
        }

        report.WriteToFile(options.Output!);
        return null;

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
