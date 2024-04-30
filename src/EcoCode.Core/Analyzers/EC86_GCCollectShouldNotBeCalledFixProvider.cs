namespace EcoCode.CodeFixes;

/// <summary>The code fix provider for avoid async void methods.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GCCollectShouldNotBeCalledFixProvider)), Shared]
public sealed class GCCollectShouldNotBeCalledFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => [GCCollectShouldNotBeCalledAnalyzer.Descriptor.Id];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override Task RegisterCodeFixesAsync(CodeFixContext context) => Task.CompletedTask;
}
