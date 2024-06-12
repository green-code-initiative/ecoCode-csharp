using EcoCode.Tool.Library.Commands;

namespace EcoCode.Tool.Library;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public static class Tool
{
    internal sealed class Workspace(Func<string, Task<Solution>> openSolutionAsync, Func<string, Task<Project>> openProjectAsync)
    {
        public Func<string, Task<Solution>> OpenSolutionAsync { get; } = openSolutionAsync;

        public Func<string, Task<Project>> OpenProjectAsync { get; } = openProjectAsync;
    }

    public static Task<int> RunAsync(string[] args,
        Func<string, Task<Solution>> openSolutionAsync,
        Func<string, Task<Project>> openProjectAsync)
    {
        var registrations = new ServiceCollection();
        _ = registrations.AddSingleton(new Workspace(openSolutionAsync, openProjectAsync));

        var registrar = new TypeRegistrar(registrations);
        var app = new CommandApp(registrar);
        app.Configure(config => config.AddCommand<AnalyzeCommand>("analyze"));
        return app.RunAsync(args);
    }

    public static void WriteLine(string line, string? color = null)
    {
        if (string.IsNullOrWhiteSpace(color))
            AnsiConsole.WriteLine(line);
        else
            AnsiConsole.MarkupLine($"[{color}]{line}[/]");
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
