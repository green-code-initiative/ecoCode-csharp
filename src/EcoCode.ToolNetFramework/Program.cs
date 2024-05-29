using EcoCode.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Immutable;
using System.IO;
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

        using var workspace = MSBuildWorkspace.Create();
        workspace.WorkspaceFailed += (sender, e) => Console.WriteLine(e.Diagnostic.Message);

        Project? project = null;
        Solution? solution = null;

        if (Path.GetExtension(path) == ".sln")
        {
            solution = await workspace.OpenSolutionAsync(path);
        }
        else if (Path.GetExtension(path) == ".csproj")
        {
            project = await workspace.OpenProjectAsync(path);
        }
        else
        {
            Console.WriteLine("Please provide a valid .sln or .csproj file.");
            return;
        }

        if (solution != null)
        {
            foreach (var proj in solution.Projects)
                await AnalyzeProject(proj);
        }
        else if (project != null)
        {
            await AnalyzeProject(project);
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
