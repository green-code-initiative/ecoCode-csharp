namespace EcoCode.Analyzers;

/// <summary>Analyzer for don't call loop invariant functions in loop conditions.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DontCallFunctionsInLoopConditionsAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> SyntaxKinds = [
        SyntaxKind.ForStatement,
        SyntaxKind.WhileStatement,
        SyntaxKind.DoStatement];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = new(
        Rule.Ids.EC69_DontCallFunctionsInLoopConditions,
        title: "Don't call loop invariant functions in loop conditions",
        messageFormat: "Don't call loop invariant functions in loop conditions",
        Rule.Categories.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: string.Empty,
        helpLinkUri: Rule.GetHelpUri(Rule.Ids.EC69_DontCallFunctionsInLoopConditions));

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(static context => AnalyzeLoopNode(context), SyntaxKinds);
    }

    private static void AnalyzeLoopNode(SyntaxNodeAnalysisContext context)
    {
        var (condition, expression, incrementors) = context.Node switch
        {
            ForStatementSyntax forStatement => (forStatement.Condition, forStatement.Statement, forStatement.Incrementors),
            WhileStatementSyntax whileStatement => (whileStatement.Condition, whileStatement.Statement, default),
            DoStatementSyntax doStatement => (doStatement.Condition, doStatement.Statement, default),
            _ => default
        };
        if (condition is null) return;

        // Step 1: Identify the variables used in the conditions
        var loopInvariantSymbols = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        foreach (var node in condition.DescendantNodes())
        {
            if (node is not InvocationExpressionSyntax invocation) continue;

            foreach (var arg in invocation.ArgumentList.Arguments)
            {
                if (context.SemanticModel.GetSymbolInfo(arg.Expression).Symbol is ISymbol symbol && symbol.IsVariable())
                    _ = loopInvariantSymbols.Add(symbol);
            }
        }
        if (loopInvariantSymbols.Count == 0) return;

        // Step 2: Remove the variables that are mutated in the loop body or the for loop incrementors
        RemoveMutatedSymbols(expression.DescendantNodes(), loopInvariantSymbols, context.SemanticModel);
        if (loopInvariantSymbols.Count == 0) return;

        foreach (var inc in incrementors)
            RemoveMutatedSymbols(inc.DescendantNodesAndSelf(), loopInvariantSymbols, context.SemanticModel);
        if (loopInvariantSymbols.Count == 0) return;

        // Step 3: Identify conditions that are loop invariant
        foreach (var node in condition.DescendantNodes())
        {
            if (node is not InvocationExpressionSyntax invocation) continue;

            bool loopInvariant = true;

            foreach (var arg in invocation.ArgumentList.Arguments)
            {
                if (context.SemanticModel.GetSymbolInfo(arg.Expression).Symbol is ISymbol symbol && !loopInvariantSymbols.Contains(symbol))
                {
                    loopInvariant = false;
                    break;
                }
            }

            if (loopInvariant)
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.GetLocation()));
        }

        static void RemoveMutatedSymbols(IEnumerable<SyntaxNode> nodes, HashSet<ISymbol> loopInvariantSymbols, SemanticModel semanticModel)
        {
            if (loopInvariantSymbols.Count == 0) return;
            foreach (var node in nodes)
            {
                var symbol = node switch
                {
                    AssignmentExpressionSyntax assignment => semanticModel.GetSymbolInfo(assignment.Left).Symbol,
                    PrefixUnaryExpressionSyntax prefix => semanticModel.GetSymbolInfo(prefix.Operand).Symbol,
                    PostfixUnaryExpressionSyntax postfix => semanticModel.GetSymbolInfo(postfix.Operand).Symbol,
                    _ => null
                };

                if (symbol is not null && loopInvariantSymbols.Remove(symbol) && loopInvariantSymbols.Count == 0)
                    return;
            }
        }
    }
}
