namespace EcoCode.CodeFixes;

/// <summary>The code fix provider for avoid async void methods.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GCCollectShouldNotBeUsedFixProvider)), Shared]
public sealed class GCCollectShouldNotBeUsedFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => [GCCollectShouldNotBeUsedAnalyzer.Descriptor.Id];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override Task RegisterCodeFixesAsync(CodeFixContext context) => Task.CompletedTask;
}
