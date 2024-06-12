using System.ComponentModel;

namespace EcoCode.Tool.Library.Commands;

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

internal sealed class AnalyzeSettings : CommandSettings
{
    // -a or --analyzers to specify the analyzers to use. Default is All. Ex: -a "EC72;EC75;EC81"
    // -i or --ignore to specify the analyzers to ignore. Default is Empty. Ex: -i "EC84;EC85;EC86". Has priority over -a.
    // -r or --severity to specify the minimum severity of the analyzers to use. Can be All, Info, Warning or Error. Default is All.
    // -v or --verbosity to customize information display about the analysis. Can be None, Normal or Detailed. Default is Normal.

    [Description("Path to the .sln/.slnf/.csproj file to load and analyze.")]
    [CommandArgument(0, "[sourcePath]")]
    public string Source { get; set; } = default!;

    [Description("Path to the .html/.json/.csv/.txt file to save the analysis into.")]
    [CommandArgument(1, "[outputPath]")]
    public string Output { get; set; } = default!;

    public SourceType SourceType => IsSolution(Path.GetExtension(Source)) ? SourceType.Solution : SourceType.Project;

    private static bool IsSolution(string? ext) =>
        string.Equals(ext, ".sln", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(ext, ".slnf", StringComparison.OrdinalIgnoreCase);

    private static bool IsProject(string? ext) =>
        string.Equals(ext, ".csproj", StringComparison.OrdinalIgnoreCase);

    public override ValidationResult Validate()
    {
        if (Path.GetExtension(Source) is not { } sourceExt || !IsSolution(sourceExt) && !IsProject(sourceExt))
            return ValidationResult.Error($"The source path {Source} must point to a valid .sln, .slnf or .csproj file.");

        if (Path.GetExtension(Output) is not { } outputExt ||
            !string.Equals(outputExt, ".html", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(outputExt, ".json", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(outputExt, ".csv", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(outputExt, ".txt", StringComparison.OrdinalIgnoreCase))
        {
            return ValidationResult.Error($"The output path {Output} must point to a .html, .json, .csv or .txt file.");
        }

        return ValidationResult.Success();
    }
}
