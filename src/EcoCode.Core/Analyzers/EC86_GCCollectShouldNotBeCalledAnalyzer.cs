namespace EcoCode.Analyzers;

/// <summary>Analyzer for avoid async void methods.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GCCollectShouldNotBeCalledAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = new(
        Rule.Ids.EC86_GCCollectShouldNotBeCalled,
        title: "Avoid calling GC.Collect() method",
        messageFormat: "Avoid calling GC.Collect() method",
        Rule.Categories.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: null,
        helpLinkUri: Rule.GetHelpUri(Rule.Ids.EC86_GCCollectShouldNotBeCalled));

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(static context => AnalyzeMethod(context), SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;
        if (invocationExpression.Expression.GetText().ToString().Contains("GC.Collect")
            && (
                invocationExpression.ArgumentList.Arguments.Count == 0
                || invocationExpression.ArgumentList.Arguments[0].GetText().ToString() != "0"
                ))
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocationExpression.GetLocation()));
        }
    }
}
