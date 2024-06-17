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

internal static class Extensions
{
    public static ImmutableArray<string> Solution { get; } = [".sln", ".slnf", ".slnx"];

    public const string Project = ".csproj";
}
