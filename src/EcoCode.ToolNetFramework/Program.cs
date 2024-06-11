using EcoCode.ToolNetFramework.Commands;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading.Tasks;

namespace EcoCode.ToolNetFramework;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        int errorCode = await new CommandApp<AnalyzeCommand>().RunAsync(args);
        if (!Console.IsOutputRedirected) // Running in interactive mode
        {
            AnsiConsole.WriteLine("Press a key to exit..");
            _ = Console.ReadKey();
        }
        return errorCode;
    }
}
