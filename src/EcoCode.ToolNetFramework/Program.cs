using EcoCode.ToolNetFramework.Commands;

int errorCode = await new CommandApp<AnalyzeCommand>().RunAsync(args);
if (!Console.IsOutputRedirected) // Running in interactive mode
{
    AnsiConsole.WriteLine("Press a key to exit..");
    _ = Console.ReadKey();
}
return errorCode;
