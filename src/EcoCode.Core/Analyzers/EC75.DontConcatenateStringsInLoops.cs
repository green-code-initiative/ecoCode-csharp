namespace EcoCode.Analyzers;

/// <summary>EC75: Don't concatenate strings in loops.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DontConcatenateStringsInLoops : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> SyntaxKinds = [
        SyntaxKind.ForStatement,
        SyntaxKind.ForEachStatement,
        SyntaxKind.WhileStatement,
        SyntaxKind.DoStatement];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.EC75_DontConcatenateStringsInLoops,
        title: "Don't concatenate strings in loops",
        message: "A string is concatenated in a loop",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "Strings should not be concatenated in loops, use a more optimized way, such as a StringBuilder.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(static context => AnalyzeLoopNode(context), SyntaxKinds);
    }

    private static void AnalyzeLoopNode(SyntaxNodeAnalysisContext context)
    {
        foreach (var loopStatement in context.Node.GetLoopStatements())
        {
            if (loopStatement is ExpressionStatementSyntax expressionStatement &&
                expressionStatement.Expression is AssignmentExpressionSyntax assignment &&
                assignment.IsKind(SyntaxKind.AddAssignmentExpression) &&
                assignment.Left is IdentifierNameSyntax identifierName &&
                context.SemanticModel.GetSymbolInfo(identifierName).Symbol is ISymbol symbol &&
                symbol.IsVariableOfType(SpecialType.System_String) &&
                symbol.IsDeclaredOutsideLoop(context.Node))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, assignment.Parent!.GetLocation()));
            }
        }
    }
}
