using EcoCode.Analyzers;
using System.ComponentModel;

namespace EcoCode.Tool.Commands;

internal sealed class AnalyzeSettings : CommandSettings
{
    private static readonly ImmutableArray<DiagnosticAnalyzer> AllAnalyzers =
        typeof(DontCallFunctionsInLoopConditions).Assembly.GetTypes()
        .Where(type => type.IsSealed && type.IsSubclassOf(typeof(DiagnosticAnalyzer)))
        .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type)!)
        .ToImmutableArray();

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
        SourceType = Extensions.IsProject(ext) ? SourceType.Project
            : Extensions.IsSolution(ext) ? SourceType.Solution
            : SourceType.Unknown;

        OutputType = Enum.TryParse<OutputType>(Path.GetExtension(output)?[1..], ignoreCase: true, out var extType)
            ? extType // [1..] to remove the leading dot
            : OutputType.Unknown;

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

        var validIds = new HashSet<string>(AllAnalyzers.SelectMany(analyzer => analyzer.SupportedDiagnostics.Select(diag => diag.Id)));

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
                : analyzer => !analyzer.SupportedDiagnostics.All(diag => Exclude.Contains(diag.Id, StringComparison.OrdinalIgnoreCase))
            : Exclude.Length == 0
                ? analyzer => analyzer.SupportedDiagnostics.Any(diag => Include.Contains(diag.Id, StringComparison.OrdinalIgnoreCase))
                : analyzer =>
                    analyzer.SupportedDiagnostics.Any(diag => Include.Contains(diag.Id, StringComparison.OrdinalIgnoreCase)) &&
                    !analyzer.SupportedDiagnostics.All(diag => Exclude.Contains(diag.Id, StringComparison.OrdinalIgnoreCase));

        return predicate is null ? AllAnalyzers : AllAnalyzers.Where(predicate).ToImmutableArray();
    }
}
