namespace EcoCode.CodeFixes;

/// <summary>The code fix provider for don't call functions in a loop condition.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DontCallFunctionsInLoopConditionCodeFixProvider)), Shared]
public sealed class DontCallFunctionsInLoopConditionCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => [DontCallFunctionsInLoopCondition.Descriptor.Id];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override Task RegisterCodeFixesAsync(CodeFixContext context) => Task.CompletedTask;
}