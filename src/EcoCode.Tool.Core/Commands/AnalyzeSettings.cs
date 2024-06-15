using System.ComponentModel;

namespace EcoCode.Tool.Core.Commands;

internal enum SourceType
{
    Solution,
    Project
}

internal enum OutputType
{
    Html,
    Json,
    Csv
}

internal sealed class AnalyzeSettings : CommandSettings
{
    // Arg 0: Source path
    // Arg 1: Output path
    // -i or --include to specify the analyzers to include. Default is All. Ex: -i "EC72;EC75;EC81"
    // -e or --exclude to specify the analyzers to exclude. Default is Empty. Ex: -e "EC84;EC85;EC86". Has priority over -i for conflicts.
    // -s or --severity to specify the minimum severity of the analyzers to use. Can be Info, Warning or Error. Default is Info.
    // -q or --quiet to stop any console output. Default is not quiet.

    [Description("Path to the .sln/.slnf/.csproj file to load and analyze.")]
    [CommandArgument(0, "[sourcePath]")]
    public string Source { get; set; } = default!;

    [Description("Path to the .html/.json/.csv file to save the analysis into.")]
    [CommandArgument(1, "[outputPath]")]
    public string Output { get; set; } = default!;

    public SourceType SourceType => IsSolution(Path.GetExtension(Source)) ? SourceType.Solution : SourceType.Project;

    private static bool IsSolution(string? ext) =>
        string.Equals(ext, ".sln", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(ext, ".slnf", StringComparison.OrdinalIgnoreCase);

    private static bool IsProject(string? ext) =>
        string.Equals(ext, ".csproj", StringComparison.OrdinalIgnoreCase);

    public override ValidationResult Validate() =>
        Path.GetExtension(Source) is not { } sourceExt || !IsSolution(sourceExt) && !IsProject(sourceExt)
        ? ValidationResult.Error($"The source path {Source} must point to a valid .sln, .slnf or .csproj file.")
        : Path.GetExtension(Output) is not { } outputExt ||
            !string.Equals(outputExt, ".html", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(outputExt, ".json", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(outputExt, ".csv", StringComparison.OrdinalIgnoreCase)
        ? ValidationResult.Error($"The output path {Output} must point to a .html, .json or .csv file.")
        : ValidationResult.Success();
}
