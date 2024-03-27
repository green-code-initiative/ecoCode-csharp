namespace EcoCode.CodeFixes;

/// <summary>The code fix provider for don't execute sql commands in loops.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DontExecuteSqlCommandsInLoopsCodeFixProvider)), Shared]
public sealed class DontExecuteSqlCommandsInLoopsCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => [DontExecuteSqlCommandsInLoopsAnalyzer.Descriptor.Id];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override Task RegisterCodeFixesAsync(CodeFixContext context) => Task.CompletedTask;
}
