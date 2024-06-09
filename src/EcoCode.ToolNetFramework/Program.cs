using EcoCode.Analyzers;
using EcoCode.ToolNetFramework.Reports;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace EcoCode.ToolNetFramework;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        int exitCode = await StartAndRunAsync(args);
        if (!Console.IsOutputRedirected) // Running in interactive mode
        {
            Console.WriteLine("Press a key to exit..");
            _ = Console.ReadKey();
        }
        return exitCode;
    }

    private static async Task<int> StartAndRunAsync(string[] args)
    {
        if (Parser.Default.ParseArguments<CommandOptions>(args) is not Parsed<CommandOptions> { Value: { } options })
            return 1; // No need to display the problem, the library already does it in this case

        string? error = options.ValidateAndInitialize() ?? await RunAsync(options).ConfigureAwait(false);
        if (error is not null) return 0;
        Console.WriteLine(error);
        return 1;
    }

    private static async Task<string?> RunAsync(CommandOptions options)
    {
        if (MSBuildLocator.QueryVisualStudioInstances().FirstOrDefault() is not { } instance)
            return "No MSBuild instance was found, exiting.";

        Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");
        MSBuildLocator.RegisterInstance(instance);

        using var workspace = MSBuildWorkspace.Create();
        workspace.WorkspaceFailed += (sender, e) => Console.WriteLine(e.Diagnostic.Message);

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
    }

    private static async Task AnalyzeProject(Project project, ImmutableArray<DiagnosticAnalyzer> analyzers, IAnalyzerReport report)
    {
        Console.WriteLine($"Analyzing project {project.Name}...");

        if (await project.GetCompilationAsync() is not { } compilation)
        {
            Console.WriteLine($"Unable to load the project {project.Name} compilation, skipping");
            return;
        }

        foreach (var diagnostic in await compilation!.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync())
            report.Add(DiagnosticInfo.FromDiagnostic(diagnostic));

        Console.WriteLine($"Analysis complete for project {project.Name}");
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
