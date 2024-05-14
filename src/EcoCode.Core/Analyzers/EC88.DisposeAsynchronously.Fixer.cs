namespace EcoCode.Analyzers;

/// <summary>EC88 fixer: Dispose asynchronously.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisposeAsynchronouslyFixer)), Shared]
public sealed class DisposeAsynchronouslyFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [DisposeAsynchronously.Descriptor.Id];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override Task RegisterCodeFixesAsync(CodeFixContext context) => Task.CompletedTask;
}
