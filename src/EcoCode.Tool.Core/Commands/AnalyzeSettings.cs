using System.ComponentModel;

namespace EcoCode.Tool.Core.Commands;

internal enum SourceType
{
    Unknown,
    Solution,
    Project
}

internal enum OutputType
{
    Unknown,
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

    [Description("Path to the .sln/.slnf/.slnx/.csproj file to load and analyze.")]
    [CommandArgument(0, "[sourcePath]")]
    public string Source { get; set; } = default!;

    [Description("Path to the .html/.json/.csv file to save the analysis into.")]
    [CommandArgument(1, "[outputPath]")]
    public string Output { get; set; } = default!;

    public SourceType SourceType => _sourceTypeLazy.Value;
    private readonly Lazy<SourceType> _sourceTypeLazy;

    public OutputType OutputType => _outputTypeLazy.Value;
    private readonly Lazy<OutputType> _outputTypeLazy;

    public AnalyzeSettings()
    {
        _sourceTypeLazy = new(() => GetSourceType(Source));
        _outputTypeLazy = new(() => GetOutputType(Output));
    }

    public override ValidationResult Validate() =>
        SourceType is SourceType.Unknown
        ? ValidationResult.Error($"The source path {Source} must point to a valid .sln, .slnf, .slnx or .csproj file.")
        : OutputType is OutputType.Unknown
        ? ValidationResult.Error($"The output path {Output} must point to a .html, .json or .csv file.")
        : ValidationResult.Success();

    private static SourceType GetSourceType(string ext) =>
        string.Equals(ext, ".sln", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(ext, ".slnf", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(ext, ".slnx", StringComparison.OrdinalIgnoreCase)
        ? SourceType.Solution
        : string.Equals(ext, ".csproj", StringComparison.OrdinalIgnoreCase)
        ? SourceType.Project
        : SourceType.Unknown;

    private static OutputType GetOutputType(string ext) =>
        string.Equals(ext, ".html", StringComparison.OrdinalIgnoreCase) ? OutputType.Html
        : string.Equals(ext, ".json", StringComparison.OrdinalIgnoreCase) ? OutputType.Json
        : string.Equals(ext, ".csv", StringComparison.OrdinalIgnoreCase) ? OutputType.Csv
        : OutputType.Unknown;
}
