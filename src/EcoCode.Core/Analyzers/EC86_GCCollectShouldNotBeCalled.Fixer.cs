namespace EcoCode.Analyzers;

/// <summary>The code fix provider for avoid async void methods.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GCCollectShouldNotBeCalledFixer)), Shared]
public sealed class GCCollectShouldNotBeCalledFixer: CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => [GCCollectShouldNotBeCalled.Descriptor.Id];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override Task RegisterCodeFixesAsync(CodeFixContext context) => Task.CompletedTask;
}
