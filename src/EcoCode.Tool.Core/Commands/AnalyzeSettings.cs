using System.ComponentModel;
using System.Linq;

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

internal enum SeverityLevel
{
    Unknown,
    Info,
    Warning,
    Error
}

internal sealed class AnalyzeSettings : CommandSettings
{
    private static readonly ImmutableArray<string> SolutionExtensions = [ ".sln", ".slnf", ".slnx" ];

    [Description("Path to the .sln/.slnf/.slnx/.csproj file to load and analyze.")]
    [CommandArgument(0, "[sourcePath]")]
    public string Source { get; set; } = default!;

    [Description("Path to the .html/.json/.csv file to save the analysis into.")]
    [CommandArgument(1, "[outputPath]")]
    public string Output { get; set; } = default!;

    [Description("The analyzers to include, default is empty for all. Ex: \"EC72;EC75;EC81\"")]
    [CommandOption("-i|--include")]
    public string? Include { get; set; }

    [Description("The analyzers to exclude, default is empty for none. Ex: \"EC84;EC85;EC83\"")]
    [CommandOption("-e|--exclude")]
    public string? Exclude { get; set; }

    [Description("The minimum severity of the analyzers to run. Can be Info, Warning or Error. Default is Info.")]
    [CommandOption("-s|--severity")]
    [DefaultValue("Info")]
    public string Severity { get; set; } = default!;

    [Description("Stops all console outputs, default is verbose.")]
    [CommandOption("-q|--quiet")]
    public bool Quiet { get; set; }

    public SourceType SourceType =>
        Path.GetExtension(Source) is not { } ext ? SourceType.Unknown
        : SolutionExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase) ? SourceType.Solution
        : string.Equals(ext, ".csproj", StringComparison.OrdinalIgnoreCase) ? SourceType.Project
        : SourceType.Unknown;

    public OutputType OutputType =>
        Path.GetExtension(Output) is not { } ext ? OutputType.Unknown
        : string.Equals(ext, ".html", StringComparison.OrdinalIgnoreCase) ? OutputType.Html
        : string.Equals(ext, ".json", StringComparison.OrdinalIgnoreCase) ? OutputType.Json
        : string.Equals(ext, ".csv", StringComparison.OrdinalIgnoreCase) ? OutputType.Csv
        : OutputType.Unknown;

    public SeverityLevel SeverityLevel =>
        string.Equals(Severity, nameof(SeverityLevel.Info), StringComparison.OrdinalIgnoreCase) ? SeverityLevel.Info
        : string.Equals(Severity, nameof(SeverityLevel.Warning), StringComparison.OrdinalIgnoreCase) ? SeverityLevel.Warning
        : string.Equals(Severity, nameof(SeverityLevel.Error), StringComparison.OrdinalIgnoreCase) ? SeverityLevel.Error
        : SeverityLevel.Unknown;

    public override ValidationResult Validate()
    {
        if (SourceType is SourceType.Unknown)
            return ValidationResult.Error($"The source path {Source} must point to a valid .sln, .slnf, .slnx or .csproj file.");

        if (OutputType is OutputType.Unknown)
            return ValidationResult.Error($"The output path {Output} must point to a .html, .json or .csv file.");

        if (SeverityLevel is SeverityLevel.Unknown)
            return ValidationResult.Error($"The severity level is unknown.");

        var validIds = new HashSet<string>(AnalyzeService.Analyzers
            .SelectMany(analyzer => analyzer.SupportedDiagnostics.Select(diag => diag.Id)));

        return Include?.Split(';').Any(token => !validIds.Contains(token)) == true
            ? ValidationResult.Error("The include option contains invalid analyzer ids.")
            : Exclude?.Split(';').Any(token => !validIds.Contains(token)) == true
            ? ValidationResult.Error("The exclude option contains invalid analyzer ids.")
            : ValidationResult.Success();
    }
}
