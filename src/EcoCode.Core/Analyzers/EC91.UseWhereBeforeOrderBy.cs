namespace EcoCode.Analyzers;

/// <summary>EC91: Use Where before OrderBy.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseWhereBeforeOrderBy : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> InvocationExpressions = [SyntaxKind.InvocationExpression];
    private static readonly ImmutableArray<SyntaxKind> QueryExpressions = [SyntaxKind.QueryExpression];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.EC91_UseWhereBeforeOrderBy,
        title: "Use Where before OrderBy",
        message: "Call OrderBy before Where in a LINQ method chain",
        category: Rule.Categories.Usage,
        severity: DiagnosticSeverity.Warning,
        description: "Use the Where clause before the OrderBy clause to avoid sorting unnecessary elements.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(static context => AnalyzeInvocationExpression(context), InvocationExpressions);
        context.RegisterSyntaxNodeAction(static context => AnalyzeQueryExpression(context), QueryExpressions);
    }

    private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
    {
        var currentExpr = (InvocationExpressionSyntax)context.Node;
        if (currentExpr.Expression is not MemberAccessExpressionSyntax { Name.Identifier.Text: "Where" })
            return;

        bool orderByFound = false;
        do
        {
            if (currentExpr.Expression is not MemberAccessExpressionSyntax currentMemberAccess) break;

            if (orderByFound && currentMemberAccess.Name.Identifier.Text is "OrderBy" or "OrderByDescending")
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, currentMemberAccess.Name.GetLocation()));
                return;
            }

            if (currentMemberAccess.Name.Identifier.Text is "Where")
                orderByFound = true;

            currentExpr = currentMemberAccess.Expression as InvocationExpressionSyntax;
        } while (currentExpr is not null);
    }

    private static void AnalyzeQueryExpression(SyntaxNodeAnalysisContext context)
    {
        var clauses = ((QueryExpressionSyntax)context.Node).Body.Clauses;
        int orderByClauseIdx = clauses.IndexOf(static clause => clause is OrderByClauseSyntax);
        if (orderByClauseIdx != -1 && orderByClauseIdx < clauses.IndexOf(static clause => clause is WhereClauseSyntax))
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, clauses[orderByClauseIdx].GetLocation()));
    }
}
