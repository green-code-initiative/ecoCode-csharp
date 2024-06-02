namespace EcoCode.Analyzers;

/// <summary>EC93 fixer: Return Task directly.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReturnTaskDirectly)), Shared]
public sealed class ReturnTaskDirectlyFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [ReturnTaskDirectly.Descriptor.Id];

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override Task RegisterCodeFixesAsync(CodeFixContext context) => Task.CompletedTask;
}
