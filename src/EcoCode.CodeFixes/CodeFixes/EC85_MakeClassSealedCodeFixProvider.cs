namespace EcoCode.CodeFixes;

/// <summary>The code fix provider for make class sealed.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeClassSealedCodeFixProvider)), Shared]
public sealed class MakeClassSealedCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [MakeClassSealedAnalyzer.Descriptor.Id];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override Task RegisterCodeFixesAsync(CodeFixContext context) => Task.CompletedTask;
}
