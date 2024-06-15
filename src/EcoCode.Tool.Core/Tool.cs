namespace EcoCode.Tool.Core;

/// <summary>Carries the logic for the ecoCode analysis when called from the tools.</summary>
public static class Tool
{
    internal sealed class Workspace(Func<string, Task<Solution>> openSolutionAsync, Func<string, Task<Project>> openProjectAsync)
    {
        public Func<string, Task<Solution>> OpenSolutionAsync { get; } = openSolutionAsync;

        public Func<string, Task<Project>> OpenProjectAsync { get; } = openProjectAsync;
    }

    /// <summary>Runs the tool.</summary>
    /// <param name="args">The arguments.</param>
    /// <param name="openSolutionAsync">An async lambda to open a solution.</param>
    /// <param name="openProjectAsync">An async lambda to open a project.</param>
    /// <returns>The exit code, 0 for ok.</returns>
    public static Task<int> RunAsync(string[] args,
        Func<string, Task<Solution>> openSolutionAsync,
        Func<string, Task<Project>> openProjectAsync)
    {
        var registrations = new ServiceCollection();
        _ = registrations.AddSingleton(new Workspace(openSolutionAsync, openProjectAsync));

        var app = new CommandApp(TypeRegistrarService.CreateRegistrar(registrations));
        app.Configure(config => config.AddCommand<AnalyzeCommand>("analyze"));
        return app.RunAsync(args);
    }

    /// <summary>Writes a line to the console.</summary>
    /// <param name="line">The line to write.</param>
    /// <param name="color">The color to use, null for default.</param>
    public static void WriteLine(string line, string? color = null)
    {
        if (string.IsNullOrWhiteSpace(color))
            AnsiConsole.WriteLine(line);
        else
            AnsiConsole.MarkupLine($"[{color}]{line}[/]");
    }
}
