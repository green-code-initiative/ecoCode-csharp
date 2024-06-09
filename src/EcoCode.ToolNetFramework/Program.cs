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
    public static async Task<int> Main(string[] args)
    {
        var result = await Parser.Default
            .ParseArguments<CommandOptions>(args)
            .WithParsed(options => options.ValidateAndInitialize())
            .WithParsedAsync(options => options.HasError ? Task.CompletedTask : RunAsync(options));

        int exitCode = 1;
        if (result is Parsed<CommandOptions> parsed)
        {
            if (parsed.Value.HasError)
                Console.WriteLine(parsed.Value.Error);
            else
                exitCode = 0;
        }

        if (!Console.IsOutputRedirected) // Running in interactive mode
        {
            Console.WriteLine("Press a key to exit..");
            _ = Console.ReadKey();
        }

        return exitCode;
    }

    private static async Task RunAsync(CommandOptions options)
    {
        if (MSBuildLocator.QueryVisualStudioInstances().FirstOrDefault() is not { } instance)
        {
            options.Fail("No MSBuild instance was found, exiting.");
            return;
        }
        
        Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");
        MSBuildLocator.RegisterInstance(instance);

        using var workspace = MSBuildWorkspace.Create();
        workspace.WorkspaceFailed += (sender, e) => Console.WriteLine(e.Diagnostic.Message);

        string extension = Path.GetExtension(options.Source);
        if (string.Equals(extension, ".sln", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(extension, ".slnf", StringComparison.OrdinalIgnoreCase))
        {
            if (await workspace.OpenSolutionAsync(options.Source) is not { } solution)
            {
                options.Fail("Cannot load the provided solution.");
                return;
            }
            
            var report = new HtmlReport();
            var analyzers = LoadAnalyzers();
            foreach (var project in solution.Projects)
                await AnalyzeProject(project, analyzers, report);
            report.Generate(options.Output!);
        }
        else if (string.Equals(extension, ".csproj", StringComparison.OrdinalIgnoreCase))
        {
            if (await workspace.OpenProjectAsync(options.Source) is not { } project)
            {
                options.Fail("Cannot load the provided project.");
                return;
            }

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
