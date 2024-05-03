namespace EcoCode.Analyzers;

/// <summary>Analyzer for avoid async void methods.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GCCollectShouldNotBeCalled: DiagnosticAnalyzer
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

        //if the expression is not a method or method name is not GC.Collect or Containing type is not System.GC, return
        if (context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol methodSymbol
            || methodSymbol.Name != nameof(GC.Collect)
            || !SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType,
        context.SemanticModel.Compilation.GetTypeByMetadataName("System.GC")))
        {
            return;
        }

        //If there is no arguments or the first argument (assuming it is generation) is a 0 int, raise report
        bool report = !invocationExpression.ArgumentList.Arguments.Any();
        if (!report)
        {
            var firstArgument = invocationExpression.ArgumentList.Arguments[0].Expression;
            var constantValue = context.SemanticModel.GetConstantValue(firstArgument);
            if (constantValue.Value is not int intValue || intValue != 0)
                report = true;
        }

        if(report)
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocationExpression.GetLocation()));
    }
}
