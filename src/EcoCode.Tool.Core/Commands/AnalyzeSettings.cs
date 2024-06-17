using System.ComponentModel;

namespace EcoCode.Tool.Core.Commands;

internal sealed class AnalyzeSettings : CommandSettings
{
    [Description("Path to the .sln/.slnf/.slnx/.csproj file to load and analyze.")]
    [CommandArgument(0, "[sourcePath]")]
    public string Source { get; set; }

    [Description("Path to the .html/.json/.csv file to save the analysis into.")]
    [CommandArgument(1, "[outputPath]")]
    public string Output { get; set; }

    [Description("The analyzers to include, default is empty for all. Ex: \"EC72;EC75;EC81\"")]
    [CommandOption("-i|--include")]
    [DefaultValue("")]
    public string Include { get; set; }

    [Description("The analyzers to exclude, default is empty for none. Ex: \"EC84;EC85;EC83\"")]
    [CommandOption("-e|--exclude")]
    [DefaultValue("")]
    public string Exclude { get; set; }

    [Description("The minimum severity of the analyzers to run. Can be Info, Warning or Error. Default is Info.")]
    [CommandOption("-s|--severity")]
    [DefaultValue("Info")]
    public string Severity { get; set; }

    public SourceType SourceType { get; }

    public OutputType OutputType { get; }

    public SeverityLevel SeverityLevel { get; }

    public AnalyzeSettings(string source, string output, string include, string exclude, string severity)
    {
        Source = source;
        Output = output;
        Include = include;
        Exclude = exclude;
        Severity = severity;

        string? ext = Path.GetExtension(source);
        SourceType = Extensions.Solution.Contains(ext, StringComparer.OrdinalIgnoreCase) ? SourceType.Solution
            : string.Equals(ext, Extensions.Project, StringComparison.OrdinalIgnoreCase) ? SourceType.Project
            : SourceType.Unknown;

        ext = Path.GetExtension(output); // Substring(1) to remove the leading dot
        OutputType = Enum.TryParse<OutputType>(ext?.Substring(1), ignoreCase: true, out var extType) ? extType : OutputType.Unknown;

        SeverityLevel = Enum.TryParse<SeverityLevel>(severity, ignoreCase: true, out var level) ? level : SeverityLevel.Unknown;
    }

    public override ValidationResult Validate()
    {
        if (SourceType is SourceType.Unknown)
            return ValidationResult.Error($"The source path {Source} must point to a valid .sln, .slnf, .slnx or .csproj file.");

        if (OutputType is OutputType.Unknown)
            return ValidationResult.Error($"The output path {Output} must point to a .html, .json or .csv file.");

        if (SeverityLevel is SeverityLevel.Unknown)
            return ValidationResult.Error("The severity level must be Info, Warn or Error.");

        var validIds = new HashSet<string>(AnalyzeService.Analyzers
            .SelectMany(analyzer => analyzer.SupportedDiagnostics.Select(diag => diag.Id)));

        return Include.Length != 0 && Include.Split(';').Any(token => !validIds.Contains(token))
            ? ValidationResult.Error("The include option contains invalid analyzer ids.")
            : Exclude.Length != 0 && Exclude.Split(';').Any(token => !validIds.Contains(token))
            ? ValidationResult.Error("The exclude option contains invalid analyzer ids.")
            : ValidationResult.Success();
    }

    public ImmutableArray<DiagnosticAnalyzer> GetActiveAnalyzers()
    {
        Func<DiagnosticAnalyzer, bool>? predicate =
            Include.Length == 0
            ? Exclude.Length == 0
                ? null
                : analyzer => !analyzer.SupportedDiagnostics.All(diag => Exclude.Contains(diag.Id))
            : Exclude.Length == 0
                ? analyzer => analyzer.SupportedDiagnostics.Any(diag => Include.Contains(diag.Id))
                : analyzer =>
                    analyzer.SupportedDiagnostics.Any(diag => Include.Contains(diag.Id)) &&
                    !analyzer.SupportedDiagnostics.All(diag => Exclude.Contains(diag.Id));

        return predicate is null ? AnalyzeService.Analyzers : AnalyzeService.Analyzers.Where(predicate).ToImmutableArray();
    }
}
