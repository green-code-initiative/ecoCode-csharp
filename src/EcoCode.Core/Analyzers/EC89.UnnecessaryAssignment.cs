using Microsoft.CodeAnalysis.Text;
using System.Linq;

namespace EcoCode.Analyzers;

/// <summary>EC89 : Unnecessary assignment.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnnecessaryAssignment : DiagnosticAnalyzer
{
    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
       id: Rule.Ids.EC89_UnnecessaryAssignment,
       title: "Unnecessary assignment",
       message: "Variable is assigned unnecessarily. Return it instead",
       category: Rule.Categories.Usage,
       severity: DiagnosticSeverity.Warning,
       description: "Detects variables assigned in both branches of an if-else statement and returned immediately after.");

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

        if (ifStatement.Parent is not BlockSyntax blockSyntax) return;
        if (blockSyntax.Kind() is not SyntaxKind.ElseClause && ifStatement.Else is null) return;
        if (blockSyntax.Kind() is not SyntaxKind.Block) return;

        if (ContainsPolymorphism(blockSyntax, context.SemanticModel)) return;

        if (ContainsSameVariableAssignment(blockSyntax, context.SemanticModel))
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
        }        
    }

    internal static bool ContainsPolymorphism(BlockSyntax block, SemanticModel model)
    {
        var assignments = block.DescendantNodes().OfType<AssignmentExpressionSyntax>()
            .Where(a => a.Kind() == SyntaxKind.SimpleAssignmentExpression)
            .ToList();

        foreach (var assignment in assignments)
        {
            var leftSymbol = model.GetSymbolInfo(assignment.Left).Symbol as ILocalSymbol;
            if (leftSymbol != null)
            {
                SymbolEqualityComparer comparer = SymbolEqualityComparer.Default;
                var rightType = model.GetTypeInfo(assignment.Right).Type;
                if (rightType != null)
                {
                    var baseType = rightType.BaseType;
                    var typesAssigned = assignments
                        .Select(a => model.GetTypeInfo(a.Right).Type)
                        .Where(t => t != null && t.BaseType != null && SymbolEqualityComparer.Default.Equals(t.BaseType, baseType))
                        .Distinct(SymbolEqualityComparer.Default);

                    if (typesAssigned.Count() > 1)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    internal static bool ContainsSameVariableAssignment(BlockSyntax block, SemanticModel model)
    {
        var ifStatements = block.DescendantNodes().OfType<IfStatementSyntax>();

        foreach (var ifStatement in ifStatements)
        {
            var assignedVariables = ifStatement.Statement.DescendantNodes().OfType<AssignmentExpressionSyntax>()
                .Select(a => model.GetSymbolInfo(a.Left).Symbol as ILocalSymbol)
                .Where(symbol => symbol != null)
                .ToList();

            var elseClause = ifStatement.Else;
            if (elseClause != null)
            {
                var elseAssignedVariables = elseClause.Statement.DescendantNodes().OfType<AssignmentExpressionSyntax>()
                    .Select(a => model.GetSymbolInfo(a.Left).Symbol as ILocalSymbol)
                    .Where(symbol => symbol != null)
                    .ToList();

                var commonVariables = assignedVariables?.Intersect(elseAssignedVariables, SymbolEqualityComparer.Default);
                if (commonVariables.Any())
                {
                    return true;
                }

                // Check for nested if statements within else
                if (elseClause.Statement is BlockSyntax elseBlock)
                {
                    if (ContainsSameVariableAssignment(elseBlock, model))
                    {
                        return true;
                    }
                }
            }

            // Check for nested if statements within if
            if (ifStatement.Statement is BlockSyntax ifBlock)
            {
                if (ContainsSameVariableAssignment(ifBlock, model))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
