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
    // private const string SolutionPath = @"C:\Users\vlajoumard\source\ConsoleApp1\ConsoleApp1.sln";
    private const string SolutionPath = @"C:\Users\vlajoumard\source\ecoCode-csharp-test-project\ecoCode-csharp-test-project.sln";

    // private readonly Type TestType = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);

    public static async Task Main(string[] args)
    {
        string path = args.Length == 0 ? SolutionPath : args[0];

        if (!File.Exists(path))
        {
            Console.WriteLine($"The file {path} does not exist.");
            return;
        }

        // Register MSBuild instance
        var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
        var instance = visualStudioInstances.Length == 1
            ? visualStudioInstances[0]
            : SelectVisualStudioInstance(visualStudioInstances);

        Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");
        MSBuildLocator.RegisterInstance(instance);

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

    private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
    {
        Console.WriteLine("Multiple installs of MSBuild detected, please select one:");
        for (int i = 0; i < visualStudioInstances.Length; i++)
        {
            Console.WriteLine($"Instance {i + 1}");
            Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
            Console.WriteLine($"    Version: {visualStudioInstances[i].Version}");
            Console.WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
        }

        while (true)
        {
            string userResponse = Console.ReadLine();
            if (int.TryParse(userResponse, out int instanceNumber) &&
                instanceNumber > 0 &&
                instanceNumber <= visualStudioInstances.Length)
            {
                return visualStudioInstances[instanceNumber - 1];
            }
            Console.WriteLine("Input not accepted, try again.");
        }
    }
}
