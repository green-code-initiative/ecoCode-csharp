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

        var instance = MSBuildLocator.QueryVisualStudioInstances().FirstOrDefault();
        if (instance is null)
        {
            Console.WriteLine($"No MSBuild instance was found, exiting.");
            return;
        }

        Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");
        MSBuildLocator.RegisterInstance(instance);

        using var workspace = MSBuildWorkspace.Create();
        workspace.WorkspaceFailed += (sender, e) => Console.WriteLine(e.Diagnostic.Message);

        if (Path.GetExtension(path) == ".sln")
        {
            var solution = await workspace.OpenSolutionAsync(path);
            if (solution is null)
            {
                Console.WriteLine("Cannot load the provided solution.");
            }
            else
            {
                foreach (var project in solution.Projects)
                    await AnalyzeProject(project);
            }
        }
        else if (Path.GetExtension(path) == ".csproj")
        {
            var project = await workspace.OpenProjectAsync(path);
            if (project is null)
                Console.WriteLine("Cannot load the provided project");
            else
                await AnalyzeProject(project);
        }
        else
        {
            Console.WriteLine("Please provide a valid .sln or .csproj file.");
        }

        Console.WriteLine("Press a key to exit..");
        _ = Console.ReadKey();
    }

    private static async Task AnalyzeProject(Project project)
    {
        Console.WriteLine($"Analyzing project {project.Name}...");

        var compilation = await project.GetCompilationAsync();
        var analyzers = LoadAnalyzers();

        var compilationWithAnalyzers = compilation!.WithAnalyzers(analyzers);

        var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        foreach (var diagnostic in diagnostics)
            Console.WriteLine(diagnostic.ToString());

        Console.WriteLine($"Analysis complete for project {project.Name}");
    }

    private static ImmutableArray<DiagnosticAnalyzer> LoadAnalyzers()
    {
        // Load your custom analyzers here.
        // For example, if your analyzers are in a separate assembly, you can use reflection to load them.
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
            new AvoidAsyncVoidMethods(),
            new UseListIndexer()
        );

        return analyzers;
    }
}
