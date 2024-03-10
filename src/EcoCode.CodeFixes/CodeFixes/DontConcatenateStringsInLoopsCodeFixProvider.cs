namespace EcoCode.CodeFixes;

/// <summary>The code fix provider for don't concatenate strings in loops.</summary>
/// <remarks>Hard to do right because we need to insert new code with a StringBuilder, don't fix it automatically for now.</remarks>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DontConcatenateStringsInLoopsCodeFixProvider)), Shared]
public sealed class DontConcatenateStringsInLoopsCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => [DontConcatenateStringsInLoops.Descriptor.Id];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override Task RegisterCodeFixesAsync(CodeFixContext context) => Task.CompletedTask;
}