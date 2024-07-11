using System.ComponentModel;

namespace EcoCode.Tool.Commands;

internal sealed class AnalyzeSettings : CommandSettings
{
    [Description("Path to the .sln/.slnf/.slnx/.csproj file to load and analyze.")]
    [CommandArgument(0, "[sourcePath]")]
    public string Source { get; set; }

    [Description("Path to the .html/.json/.csv file to save the analysis into.")]
    [CommandArgument(1, "[outputPath]")]
    public string Output { get; set; }

    [Description("The minimum severity of the diagnostics to report. Can be Info, Warning or Error. Default is Info.")]
    [CommandOption("-s|--severity")]
    [DefaultValue("Info")]
    public string Severity { get; set; }

    public SourceType SourceType { get; }

    public OutputType OutputType { get; }

    public DiagnosticSeverity SeverityLevel { get; }

    public AnalyzeSettings(string source, string output, string severity)
    {
        Source = source;
        Output = output;
        Severity = severity;
        SourceType = Files.GetSourceType(source);
        OutputType = Files.GetOutputType(output);
        SeverityLevel = Enum.TryParse<DiagnosticSeverity>(severity, ignoreCase: true, out var level) ? level : DiagnosticSeverity.Hidden;
    }

    public override ValidationResult Validate() =>
        SourceType is SourceType.Unknown
        ? ValidationResult.Error($"The source path {Source} must point to a valid .sln, .slnf, .slnx or .csproj file.")
        : OutputType is OutputType.Unknown
        ? ValidationResult.Error($"The output path {Output} must point to a .html, .json or .csv file.")
        : SeverityLevel is DiagnosticSeverity.Hidden
        ? ValidationResult.Error("The severity level must be Info, Warn or Error.")
        : ValidationResult.Success();
}
