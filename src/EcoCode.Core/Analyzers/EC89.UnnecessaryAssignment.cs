using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;

namespace EcoCode.Analyzers;

/// <summary>EC89 : Unnecessary assignment.</summary>
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
        var cancellationToken = context.CancellationToken;

        var ifStatement = (IfStatementSyntax)context.Node;
        var blockSyntax = ifStatement.Parent as BlockSyntax;

        if (blockSyntax == null) return;
        if (blockSyntax.Kind() is not SyntaxKind.ElseClause && ifStatement.Else is null) return;
        if (blockSyntax.Kind() is not SyntaxKind.Block) return;

        var returnStatement = FindReturnStatementBelow(blockSyntax.Statements, ifStatement);
        var expression = returnStatement?.Expression;
        if (expression is null) return;

        //if (ifStatement.SpanOrTrailingTriviaContainsDirectives())
        //    return;

        //if (returnStatement.SpanOrLeadingTriviaContainsDirectives())
        //    return;

        var semanticModel = context.SemanticModel;
        var symbol = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetSymbolInfo(semanticModel, expression, cancellationToken).Symbol;
        if (symbol is null) return;

        if (symbol.Kind is SymbolKind.Local)
        {
            var localSymbol = (ILocalSymbol)symbol;

            var localDeclarationStatement = symbol.DeclaringSyntaxReferences[0]?.GetSyntax(cancellationToken).Parent?.Parent as LocalDeclarationStatementSyntax;

            if (localDeclarationStatement?.Parent == blockSyntax.Parent) return;
            
        }
        var returnTypeSymbol = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetTypeInfo(semanticModel, expression, cancellationToken).Type;

        foreach (SyntaxNode ifOrElse in ifStatement.ChildNodes())
        {
            if (ifOrElse is StatementSyntax statement)
            {
                if (statement.IsKind(SyntaxKind.Block))
                    statement = ((BlockSyntax)statement)?.Statements?.LastOrDefault();

                if (!statement.IsKind(SyntaxKind.ThrowStatement) /*&& !IsSymbolAssignedInStatementWithCorrectType(symbol, statement, semanticModel, returnTypeSymbol, cancellationToken)*/)
                {
                    return;
                }

            }
        }
        context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
    }
    
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

    //private static bool IsSymbolAssignedInStatementWithCorrectType(ISymbol symbol, StatementSyntax statement, SemanticModel semanticModel, ITypeSymbol typeSymbol, CancellationToken cancellationToken)
    //{
    //    SimpleAssignmentStatementInfo assignmentInfo = SyntaxInfo.SimpleAssignmentStatementInfo(statement);

    //    return assignmentInfo.Success
    //        && SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbol(assignmentInfo.Left, cancellationToken), symbol)
    //        && SymbolEqualityComparer.Default.Equals(typeSymbol, semanticModel.GetTypeSymbol(assignmentInfo.Right, cancellationToken));
    //}
}
