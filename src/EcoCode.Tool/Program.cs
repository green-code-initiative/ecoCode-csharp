using Microsoft.Build.Locator;

namespace EcoCode.Tool;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (MSBuildLocator.QueryVisualStudioInstances().FirstOrDefault() is not { } instance)
        {
            WriteLine("No MSBuild instance was found, exiting.", "red");
            return 1;
        }

        WriteLine($"Using MSBuild found at {instance.MSBuildPath}.");
        MSBuildLocator.RegisterInstance(instance);

        var app = new CommandApp();
        app.Configure(config => config.AddCommand<AnalyzeCommand>("analyze"));
        int errorCode = await app.RunAsync(args).ConfigureAwait(false);

        if (!Console.IsOutputRedirected) // Interactive mode
        {
            WriteLine("Press a key to exit..");
            _ = Console.ReadKey();
        }

        return errorCode;
    }

    public static void WriteLine(string line) => AnsiConsole.WriteLine(line);

    public static void WriteLine(string line, string color) => AnsiConsole.MarkupLine($"[{color}]{line}[/]");
}
