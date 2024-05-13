namespace EcoCode.Extensions;

/// <summary>Extension methods for <see cref="Document"/>.</summary>
public static class DocumentExtensions
{
    /// <summary>Returns the language version of the document.</summary>
    /// <param name="document">The document.</param>
    /// <returns>The language version.</returns>
    public static LanguageVersion GetLanguageVersion(this Document document) =>
        document.Project.ParseOptions is CSharpParseOptions options ? options.LanguageVersion : LanguageVersion.Latest;
}
