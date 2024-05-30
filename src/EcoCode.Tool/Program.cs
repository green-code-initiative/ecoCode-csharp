using Microsoft.CodeAnalysis.MSBuild;
using Spectre.Console;

namespace EcoCode.Tool;

/// <summary>
/// EcoCode.Tool Main program
/// </summary>
public static class Program
{
    /// <summary>
    /// EcoCode.tool Entry point
    /// </summary>
    /// <param name="args">CLI arguments</param>
    public static async Task Main(string[] args)
    {
        const string targetPath = @"C:\ecocode\ecoCode-csharp-test-project\ecoCode-csharp-test-project.sln";

        AnsiConsole.Write(new Rule("[bold yellow]EcoCode.Tool[/]").Centered());

        using (var workspace = MSBuildWorkspace.Create())
        {
            var solution = await workspace.OpenSolutionAsync(targetPath).ConfigureAwait(false);
            foreach (var project in solution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    var semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);

                    if (semanticModel is null)
                    {
                        AnsiConsole.Write(new Markup($"[bold red]{document.FilePath}: SemanticModel cannot be generated.[/]"));
                        continue;
                    }
                    var diagnostics = semanticModel.GetDiagnostics();

                    foreach (var diagnostic in diagnostics)
                    {
                        AnsiConsole.Write(new Markup($"[blue]{document.FilePath}: {diagnostic}.[/]"));
                    }
                }
            }
        }

        Console.ReadKey();
    }
}
