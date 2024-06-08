using CommandLine;
using EcoCode.Analyzers;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EcoCode.ToolNetFramework;

internal static class Program
{
    // private const string SolutionDir = @"C:\Users\vlajoumard\source\ecoCode-csharp-test-project";
    // private const string SolutionPath = @$"{SolutionDir}\ecoCode-csharp-test-project.sln";

    public static async Task<int> Main(string[] args)
    {
        var result = await Parser.Default.ParseArguments<CommandOptions>(args).WithParsedAsync(RunAsync);

        if (result is not Parsed<CommandOptions> parsed)
            return 1;

        Console.WriteLine("Press a key to exit..");
        _ = Console.ReadKey();
        return 0;
    }

    private static async Task RunAsync(CommandOptions options)
    {
        options.Validate();

        string path = options.Source;

        if (MSBuildLocator.QueryVisualStudioInstances().FirstOrDefault() is not { } instance)
            throw new InvalidOperationException("No MSBuild instance was found, exiting.");
        
        Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");
        MSBuildLocator.RegisterInstance(instance);

        using var workspace = MSBuildWorkspace.Create();
        workspace.WorkspaceFailed += (sender, e) => Console.WriteLine(e.Diagnostic.Message);

        string extension = Path.GetExtension(path);
        if (string.Equals(extension, ".sln", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(extension, ".slnf", StringComparison.OrdinalIgnoreCase))
        {
            var solution = await workspace.OpenSolutionAsync(path)
                ?? throw new InvalidOperationException("Cannot load the provided solution.");
            var analyzers = LoadAnalyzers();

            var report = new HtmlReport();
            foreach (var project in solution.Projects)
                await AnalyzeProject(project, analyzers, report);
            report.Generate(options.Output!);
        }
        else if (string.Equals(extension, ".csproj", StringComparison.OrdinalIgnoreCase))
        {
            var project = await workspace.OpenProjectAsync(path)
                ?? throw new InvalidOperationException("Cannot load the provided project.");

            var report = new HtmlReport();
            await AnalyzeProject(project, LoadAnalyzers(), report);
            report.Generate(options.Output!);
        }
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
