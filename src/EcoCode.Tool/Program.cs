using EcoCode.Tool.Commands;
using Spectre.Console.Cli;

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
    public static int Main(string[] args)
    {
        var app = new CommandApp<AnalyzeCommand>();

        app.Configure(config =>
        {
            config.SetApplicationName("EcoCode.Tool");
            config.SetApplicationVersion("1.0.0");
            config.AddCommand<AnalyzeCommand>("analyze");
        });

        return app.Run(args);
    }
}
