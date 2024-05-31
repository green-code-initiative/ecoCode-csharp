using System.Diagnostics.CodeAnalysis;

namespace EcoCode.Analyzers;

/// <summary>EC86 fixer : GC Collect should not be called.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GCCollectShouldNotBeCalledFixer)), Shared]
public sealed class GCCollectShouldNotBeCalledFixer: CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [GCCollectShouldNotBeCalled.Descriptor.Id];

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override Task RegisterCodeFixesAsync(CodeFixContext context) => Task.CompletedTask;
}
