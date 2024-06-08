using CommandLine;
using System.IO;

namespace EcoCode.ToolNetFramework;

internal class CommandOptions
{
    // -s or --source: .sln, .slnf or .csproj in absolute/relative path.
    // -o or --output: .htm, .html, .json, csv or .txt in absolute/relative path. In the same directory as the source if the path is not specified, with the same name if not specified.
    // -a or --analyzers to specify the analyzers to use. Default is All. Ex: -a "EC72;EC75;EC81"
    // -i or --ignore to specify the analyzers to ignore. Default is Empty. Ex: -i "EC84;EC85;EC86". Has priority over -a.
    // -r or --severity to specify the minimum severity of the analyzers to use. Can be All, Info, Warning or Error. Default is All.
    // -v or --verbosity to customize information display about the analysis. Can be None, Normal or Detailed. Default is Normal.

    [Option('s', "source", Required = true, HelpText = "Absolute/relative path to the .sln/.slnf/.csproj file to load and analyze.")]
    public string Source { get; set; } = default!;

    [Option('o', "output", Required = false, HelpText = "Absolute/relative path to the .htm/.html/.json/.csv/.txt to output the analysis into.")]
    public string Output { get; set; } = default!;

    /*[Option('a', "analyzers", Required = false, HelpText = "Analyzers to include, all if unspecified. Ex: -a EC72;EC75;EC81")]
    public string? Analyzers { get; set; }*/

    public void Validate()
    {
        if (!File.Exists(Source))
            throw new InvalidOperationException($"The file {Source} does not exist.");

        string fileExtension = Path.GetExtension(Source);
        if (fileExtension is not ".sln" and not ".slnf" and not ".csproj")
            throw new InvalidOperationException($"The file {Source} must be a .sln, .slnf or .csproj file.");

        if (Output is null)
            Output = Path.GetFileNameWithoutExtension(Source) + ".html";
        else
            _ = Directory.CreateDirectory(Path.GetDirectoryName(Output)); // Will throw if the directory cannot be opened/created.
    }
}
