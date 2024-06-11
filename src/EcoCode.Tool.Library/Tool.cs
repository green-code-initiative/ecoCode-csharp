using EcoCode.Tool.Library.Commands;
using EcoCode.Tool.Library.Infrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading.Tasks;

namespace EcoCode.Tool.Library;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public static class Tool
{
    public delegate Task<Solution> OpenSolutionAsyncDelegate(string filePath);

    public record Delegates
    {
        public Func<string, Task<Solution>> OpenSolutionAsync { get; }
        public Func<string, Task<Project>> OpenProjectAsync { get; }
        public Delegates(Func<string, Task<Solution>> openSolutionAsync, Func<string, Task<Project>> openProjectAsync) =>
            (OpenSolutionAsync, OpenProjectAsync) = (openSolutionAsync, openProjectAsync);
    }

    public static void WriteLine(string line, string? color = null) =>
        AnsiConsole.WriteLine(string.IsNullOrWhiteSpace(color) ? line : $"{color} {line}");

    public static Task<int> RunAsync(Delegates delegates, string[] args)
    {
        var registrations = new ServiceCollection();
        _ = registrations.AddKeyedSingleton<Delegates>(delegates);

        var registrar = new TypeRegistrar(registrations);
        var app = new CommandApp(registrar);
        app.Configure(config => config.AddCommand<AnalyzeCommand>("analyze"));
        return app.RunAsync(args);
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
