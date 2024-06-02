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

internal class Program
{
    private const string SolutionPath = @"C:\Users\vlajoumard\source\ecoCode-csharp-test-project\ecoCode-csharp-test-project.sln";

    public static async Task Main(string[] args)
    {
        string path = args.Length == 0 ? SolutionPath : args[0];

        if (!File.Exists(path))
        {
            Console.WriteLine($"The file {path} does not exist.");
            return;
        }

        if (MSBuildLocator.QueryVisualStudioInstances().FirstOrDefault() is not { } instance)
        {
            Console.WriteLine($"No MSBuild instance was found, exiting.");
            return;
        }
        Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");
        MSBuildLocator.RegisterInstance(instance);

        using var workspace = MSBuildWorkspace.Create();
        workspace.WorkspaceFailed += (sender, e) => Console.WriteLine(e.Diagnostic.Message);

        string extension = Path.GetExtension(path);
        if (string.Equals(extension, ".sln", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(extension, ".slnf", StringComparison.OrdinalIgnoreCase))
        {
            var solution = await workspace.OpenSolutionAsync(path);
            if (solution is null)
            {
                Console.WriteLine("Cannot load the provided solution.");
            }
            else
            {
                var analyzers = LoadAnalyzers();
                foreach (var project in solution.Projects)
                    await AnalyzeProject(project, analyzers);
            }
        }
        else if (string.Equals(extension, ".csproj", StringComparison.OrdinalIgnoreCase))
        {
            var project = await workspace.OpenProjectAsync(path);
            if (project is null)
                Console.WriteLine("Cannot load the provided project");
            else
                await AnalyzeProject(project, LoadAnalyzers());
        }
        else
        {
            Console.WriteLine("Please provide a valid .sln, .slnf or .csproj file.");
        }

        Console.WriteLine("Press a key to exit..");
        _ = Console.ReadKey();
    }

    private static async Task AnalyzeProject(Project project, ImmutableArray<DiagnosticAnalyzer> analyzers)
    {
        Console.WriteLine($"Analyzing project {project.Name}...");

        if (await project.GetCompilationAsync() is not { } compilation)
        {
            Console.WriteLine($"Unable to load the project {project.Name} compilation, skipping");
            return;
        }

        foreach (var diagnostic in await compilation!.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync())
            Console.WriteLine(diagnostic.ToString());

        Console.WriteLine($"Analysis complete for project {project.Name}");
    }

    private static ImmutableArray<DiagnosticAnalyzer> LoadAnalyzers()
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
