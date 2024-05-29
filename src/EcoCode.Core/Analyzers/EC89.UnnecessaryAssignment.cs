using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Threading;

namespace EcoCode.Analyzers;

/// <summary>RCS1179 : Unnecessary assignment.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnnecessaryAssignment : DiagnosticAnalyzer
{
    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
       id: Rule.Ids.EC89_UnnecessaryAssignment,
       title: "Unused variable assignment",
       message: "Variable '{0}' is assigned unnecessarily.",
       category: Rule.Categories.Usage,
       severity: DiagnosticSeverity.Warning,
       description: "Detects variables assigned in both branches of an if-else statement and used immediately after.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
    }

    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (IsSimpleIf(ifStatement))
            return;

        if (!IsFirstElementOfStatement(ifStatement))
            return;

        BlockSyntax? blockSyntax = ifStatement.Parent as BlockSyntax;
        if (blockSyntax == null)
            return;

        ReturnStatementSyntax? returnStatement = FindReturnStatementBelow(blockSyntax.Statements, ifStatement);
        ExpressionSyntax? expression = returnStatement?.Expression;

        if (expression is null)
            return;

        //if (ifStatement.SpanOrTrailingTriviaContainsDirectives())
        //    return;

        //if (returnStatement.SpanOrLeadingTriviaContainsDirectives())
        //    return;

        SemanticModel semanticModel = context.SemanticModel;
        CancellationToken cancellationToken = context.CancellationToken;

        ISymbol? symbol = GetSymbol(semanticModel, expression, cancellationToken);

        //if (symbol is null)
        //    return;

        //if (!IsLocalDeclaredInScopeOrNonRefOrOutParameterOfEnclosingSymbol(symbol, statementsInfo.Parent, semanticModel, cancellationToken))
        //    return;

        //ITypeSymbol returnTypeSymbol = semanticModel.GetTypeSymbol(expression, cancellationToken);

        //foreach (IfStatementOrElseClause ifOrElse in ifStatement.AsCascade())
        //{
        //    StatementSyntax statement = ifOrElse.Statement;

        //    if (statement.IsKind(SyntaxKind.Block))
        //        statement = ((BlockSyntax)statement).Statements.LastOrDefault();

        //    if (!statement.IsKind(SyntaxKind.ThrowStatement)
        //        && !IsSymbolAssignedInStatementWithCorrectType(symbol, statement, semanticModel, returnTypeSymbol, cancellationToken))
        //    {
        //        return;
        //    }
        //}

        //DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UnnecessaryAssignment, ifStatement);
        //var returnStatement = FindReturnStatementBelow(blockSyntax.Statements, ifStatement);

        //ExpressionSyntax? expression = returnStatement?.Expression;
        //if (expression is null)
        //    return;

        //SemanticModel semanticModel = context.SemanticModel;
        //CancellationToken cancellationToken = context.CancellationToken;

        //ISymbol symbol = semanticModel.GetSymbol(expression, cancellationToken);

        //if (symbol is null)
        //    return;

        //if (!IsLocalDeclaredInScopeOrNonRefOrOutParameterOfEnclosingSymbol(symbol, statementsInfo.Parent, semanticModel, cancellationToken))
        //    return;

        //ITypeSymbol returnTypeSymbol = semanticModel.GetTypeSymbol(expression, cancellationToken);

        //foreach (IfStatementOrElseClause ifOrElse in ifStatement.AsCascade())
        //{
        //    StatementSyntax statement = ifOrElse.Statement;

        //    if (statement.IsKind(SyntaxKind.Block))
        //        statement = ((BlockSyntax)statement).Statements.LastOrDefault();

        //    if (!statement.IsKind(SyntaxKind.ThrowStatement)
        //        && !IsSymbolAssignedInStatementWithCorrectType(symbol, statement, semanticModel, returnTypeSymbol, cancellationToken))
        //    {
        //        return;
        //    }
        //}

        //DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UnnecessaryAssignment, ifStatement);

        //// Track the start and end of the entire if-else if-else chain
        var startSpan = ifStatement.Span.Start;
        var endSpan = ifStatement.Span.End;

        //// Check the assignments in all branches
        var assignments = GetAllAssignments(ifStatement, ref endSpan);

        //// Find common assignments across all branches
        var commonAssignments = assignments.First().Keys.Intersect(assignments.Last().Keys).ToList();

        if (commonAssignments.Any())
        {
            var span = TextSpan.FromBounds(startSpan, endSpan);
            var location = Location.Create(ifStatement.SyntaxTree, span);

            foreach (var variable in commonAssignments)
            {
                var diagnostic = Diagnostic.Create(Descriptor, location, variable);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
    private static List<Dictionary<string, AssignmentExpressionSyntax>> GetAllAssignments(IfStatementSyntax ifStatement, ref int endSpan)
    {
        var assignments = new List<Dictionary<string, AssignmentExpressionSyntax>>();

        while (ifStatement != null)
        {
            assignments.Add(GetAssignments(ifStatement.Statement));
            if (ifStatement.Else != null)
            {
                if (ifStatement.Else.Statement is IfStatementSyntax elseIfStatement)
                {
                    ifStatement = elseIfStatement;
                }
                else
                {
                    assignments.Add(GetAssignments(ifStatement.Else.Statement));
                    endSpan = ifStatement.Else.Span.End;
                    break;
                }
            }
            else
            {
                break;
            }
        }

        return assignments;
    }
    private static Dictionary<string, AssignmentExpressionSyntax> GetAssignments(StatementSyntax statement)
    {
        var assignments = new Dictionary<string, AssignmentExpressionSyntax>();

        if (statement is BlockSyntax block)
        {
            foreach (var stmt in block.Statements)
            {
                if (stmt is ExpressionStatementSyntax exprStmt && exprStmt.Expression is AssignmentExpressionSyntax assignment)
                {
                    var identifier = (assignment.Left as IdentifierNameSyntax)?.Identifier.ValueText;
                    if (identifier != null)
                    {
                        assignments[identifier] = assignment;
                    }
                }
            }
        }
        else if (statement is ExpressionStatementSyntax singleExprStmt && singleExprStmt.Expression is AssignmentExpressionSyntax singleAssignment)
        {
            var identifier = (singleAssignment.Left as IdentifierNameSyntax)?.Identifier.ValueText;
            if (identifier != null)
            {
                assignments[identifier] = singleAssignment;
            }
        }

        return assignments;
    }

    internal static bool IsFirstElementOfStatement(IfStatementSyntax ifStatement)
    {
        SyntaxNode? parent = ifStatement.Parent;
        if (parent == null)
            return false;

        if (parent?.Kind() is SyntaxKind.Block)
            return true;
        else
            return false;
    }
    internal static bool IsSimpleIf(IfStatementSyntax ifStatement) => ifStatement?.Parent.IsKind(SyntaxKind.ElseClause) == false
            && ifStatement.Else is null;
    internal static ReturnStatementSyntax? FindReturnStatementBelow(SyntaxList<StatementSyntax> statements, StatementSyntax statement)
    {
        int index = statements.IndexOf(statement);

        if (index < statements.Count - 1)
        {
            StatementSyntax nextStatement = statements[index + 1];

            if (nextStatement.IsKind(SyntaxKind.ReturnStatement))
                return (ReturnStatementSyntax)nextStatement;
        }

        return null;
    }
    internal static ISymbol? GetSymbol(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken cancellationToken = default) =>
        Microsoft.CodeAnalysis.CSharp.CSharpExtensions
        .GetSymbolInfo(semanticModel, expression, cancellationToken)
        .Symbol;
}
