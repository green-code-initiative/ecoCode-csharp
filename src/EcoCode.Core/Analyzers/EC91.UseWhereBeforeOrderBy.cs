using System.Linq;

namespace EcoCode.Analyzers;

/// <summary>EC88: Dispose resource asynchronously.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseWhereBeforeOrderBy : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> UsingStatementKinds = [SyntaxKind.UsingStatement];
    private static readonly ImmutableArray<SyntaxKind> UsingDeclarationKinds = [SyntaxKind.LocalDeclarationStatement];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.EC91_UseWhereBeforeOrderBy,
        title: "Use Where before Order by",
        message: "With LINQ use Where before Order by",
        category: Rule.Categories.Usage,
        severity: DiagnosticSeverity.Warning,
        description: "Discrease the number of element to try.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeQueryExpression, SyntaxKind.QueryExpression);
        context.RegisterSyntaxNodeAction(AnalyzeMethodChain, SyntaxKind.QueryBody);
    }

    private static void AnalyzeQueryExpression(SyntaxNodeAnalysisContext context)
    {
        var queryExpression = (QueryExpressionSyntax)context.Node;

        var whereClause = queryExpression.Body.Clauses.OfType<WhereClauseSyntax>().FirstOrDefault();
        var orderByClause = queryExpression.Body.Clauses.OfType<OrderByClauseSyntax>().FirstOrDefault();

        if (whereClause != null && orderByClause != null)
        {
            var whereIndex = queryExpression.Body.Clauses.IndexOf(whereClause);
            var orderByIndex = queryExpression.Body.Clauses.IndexOf(orderByClause);

            if (whereIndex > orderByIndex)
            {
                var diagnostic = Diagnostic.Create(Descriptor, orderByClause.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static void AnalyzeMethodChain(SyntaxNodeAnalysisContext context)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var methodChain = GetMethodChain(memberAccess).ToList();

            var whereIndex = methodChain.FindIndex(m => m.Identifier.Text == "Where");
            var orderByIndex = methodChain.FindIndex(m => m.Identifier.Text == "OrderBy");

            if (whereIndex > orderByIndex && orderByIndex != -1 && whereIndex != -1)
            {
                var diagnostic = Diagnostic.Create(Descriptor, memberAccess.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static IEnumerable<SimpleNameSyntax> GetMethodChain(MemberAccessExpressionSyntax memberAccess)
    {
        while (memberAccess != null)
        {
            if (memberAccess.Name is SimpleNameSyntax simpleName)
            {
                yield return simpleName;
            }

            if (memberAccess.Expression is MemberAccessExpressionSyntax innerMemberAccess)
            {
                memberAccess = innerMemberAccess;
            }
            else
            {
                yield break;
            }
        }
    }
}

