namespace EcoCode.Tool.Commands;

internal enum SourceType
{
    Unknown,
    Project,
    Solution
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
    private const string Project = ".csproj";

    private static readonly ImmutableArray<string> Solution = [".sln", ".slnf", ".slnx"];

    public static bool IsProject(string? ext) => string.Equals(ext, Project, StringComparison.OrdinalIgnoreCase);

    public static bool IsSolution(string? ext) => ext?.Length > 0 && Solution.Contains(ext, StringComparer.OrdinalIgnoreCase);
}
