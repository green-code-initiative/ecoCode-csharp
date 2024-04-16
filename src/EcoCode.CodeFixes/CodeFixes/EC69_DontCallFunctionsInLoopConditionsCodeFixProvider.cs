namespace EcoCode.CodeFixes;

/// <summary>The code fix provider for don't call loop invariant functions in loop conditions.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DontCallFunctionsInLoopConditionsCodeFixProvider)), Shared]
public sealed class DontCallFunctionsInLoopConditionsCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [DontCallFunctionsInLoopConditionsAnalyzer.Descriptor.Id];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override Task RegisterCodeFixesAsync(CodeFixContext context) => Task.CompletedTask;
}
