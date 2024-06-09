namespace EcoCode.ToolNetFramework;

internal enum SourceType
{
    Solution,
    Project
}

internal enum OutputType
{
    Html,
    Json,
    Csv,
    Txt
}

internal class CommandOptions
{
    // -a or --analyzers to specify the analyzers to use. Default is All. Ex: -a "EC72;EC75;EC81"
    // -i or --ignore to specify the analyzers to ignore. Default is Empty. Ex: -i "EC84;EC85;EC86". Has priority over -a.
    // -r or --severity to specify the minimum severity of the analyzers to use. Can be All, Info, Warning or Error. Default is All.
    // -v or --verbosity to customize information display about the analysis. Can be None, Normal or Detailed. Default is Normal.

    [Option('s', "source", Required = true, HelpText = "Path to the .sln/.slnf/.csproj file to load and analyze.")]
    public string Source { get; set; } = default!;

    [Option('o', "output", Required = false, HelpText = "Path to the .html/.json/.csv/.txt file to save the analysis into.")]
    public string Output { get; set; } = default!;

    /*[Option('a', "analyzers", Required = false, HelpText = "Analyzers to include, all if unspecified. Ex: -a EC72;EC75;EC81")]
    public string? Analyzers { get; set; }*/

    public SourceType SourceType { get; private set; }

    public OutputType OutputType { get; private set; }

    public string? ValidateAndInitialize()
    {
        if (!File.Exists(Source))
            return $"The file {Source} does not exist.";

        string sourceExt = Path.GetExtension(Source);
        if (string.Equals(sourceExt, ".sln", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(sourceExt, ".slnf", StringComparison.OrdinalIgnoreCase))
        {
            SourceType = SourceType.Solution;
        }
        else if (string.Equals(sourceExt, ".csproj", StringComparison.OrdinalIgnoreCase))
        {
            SourceType = SourceType.Project;
        }
        else
        {
            return $"The file {Source} must be a .sln, .slnf or .csproj file.";
        }

        if (Output is null)
        {
            Output = Path.Combine(Path.GetDirectoryName(Source), Path.GetFileNameWithoutExtension(Source) + ".html");
        }
        else
        {
            string outputExt = Path.GetExtension(Output);
            if (string.Equals(outputExt, ".html", StringComparison.OrdinalIgnoreCase))
                OutputType = OutputType.Html;
            else if (string.Equals(outputExt, ".json", StringComparison.OrdinalIgnoreCase))
                OutputType = OutputType.Json;
            else if (string.Equals(outputExt, ".csv", StringComparison.OrdinalIgnoreCase))
                OutputType = OutputType.Csv;
            else if (string.Equals(outputExt, ".txt", StringComparison.OrdinalIgnoreCase))
                OutputType = OutputType.Txt;
            else
                return "The output file must be a .html, .json, .csv or .txt file.";

            if (Path.GetDirectoryName(Output) is not string outputDir)
            {
                Output = Path.Combine(Path.GetDirectoryName(Source), Output);
            }
            else
            {
                try
                {
                    _ = Directory.CreateDirectory(outputDir);
                }
                catch (Exception ex)
                {
                    return $"The output directory {outputDir} cannot be opened or created:{Environment.NewLine}{ex.Message}";
                }
            }
        }

        return null;
    }
}
