using Microsoft.CodeAnalysis.MSBuild;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace EcoCode.Tool.Commands;

internal sealed class AnalyzeCommand : AsyncCommand<AnalyzeCommand.Settings>
{
    public const int ERROR_CODE_NO_SOLUTION_FILE = -1;
    public const int ERROR_CODE_TOO_MANY_SOLUTION_FILE = -2;

    public sealed class Settings : CommandSettings
    {
        [Description(
            """
            Path to solution to analyze.
            Defaults to unique current directory.
            If multiple solutions are found, an error will be thrown.
        """)]
        [CommandArgument(0, "[solutionPath]")]
        public string? SolutionPath { get; init; }
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings)
    {
        var panel = new Panel("[italic yellow]Make the code green again.[/]")
        {
            Header = new PanelHeader("[bold blue]EcoCode Tool[/]").Centered()
        };
        AnsiConsole.Write(panel);

        string solutionPath = default!;
        if (settings.SolutionPath is null)
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            var solutionFiles = directory.GetFiles("*.sln");

            if (solutionFiles.Length <= 0)
            {
                AnsiConsole.Write(new Markup("[bold red]No solution file found in current directory.[/]"));
                return ERROR_CODE_NO_SOLUTION_FILE;
            }
            if (solutionFiles.Length > 1)
            {
                AnsiConsole.Write(new Markup("[bold red]Multiple solution files found in current directory, must find only one.[/]"));
                return ERROR_CODE_TOO_MANY_SOLUTION_FILE;
            }
        }
        else
        {
            solutionPath = settings.SolutionPath;
        }

        using (var workspace = MSBuildWorkspace.Create())
        {
            var solution = await workspace.OpenSolutionAsync(solutionPath).ConfigureAwait(false);
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

        return 0;
    }
}
