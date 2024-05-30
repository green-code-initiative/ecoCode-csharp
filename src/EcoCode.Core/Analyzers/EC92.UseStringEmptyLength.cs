using Microsoft.CodeAnalysis.CSharp;

namespace EcoCode.Analyzers;

/// <summary>EC92: Use string.Length instead of comparison with empty string</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseStringEmptyLength : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> EqualsExpression = [SyntaxKind.EqualsExpression];
    private static readonly ImmutableArray<SyntaxKind> NotEqualsExpression = [SyntaxKind.NotEqualsExpression];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
                id: Rule.Ids.EC92_UseStringEmptyLength,
                title: "Use string.Length instead of comparison with empty string",
                message: "Use string.Length instead of comparison with empty string",
                category: Rule.Categories.Usage,
                severity: DiagnosticSeverity.Warning,
                description: "Use string.Length instead of comparison with empty string for better readability and performance.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeComparison, EqualsExpression);
        context.RegisterSyntaxNodeAction(AnalyzeComparison, NotEqualsExpression);
    }

    private static void AnalyzeComparison(SyntaxNodeAnalysisContext context)
    {
        var binaryExpression = (BinaryExpressionSyntax)context.Node;

        // Check if either side of the comparison is a string literal ""
        if (IsEmptyStringComparison(binaryExpression, context.SemanticModel))
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, binaryExpression.GetLocation()));
        }
    }

    private static bool IsEmptyStringComparison(BinaryExpressionSyntax binaryExpression, SemanticModel semanticModel)
    {
        // Check if the left or right side of the expression is an empty string literal
        var left = binaryExpression.Left;
        var right = binaryExpression.Right;

        return (IsStringLiteral(left, semanticModel) && IsEmptyString(right)) ||
               (IsStringLiteral(right, semanticModel) && IsEmptyString(left));
    }

    private static bool IsStringLiteral(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        var typeInfo = semanticModel.GetTypeInfo(expression);
        return typeInfo.Type?.SpecialType == SpecialType.System_String;
    }

    private static bool IsEmptyString(ExpressionSyntax expression)
    {
        return expression is LiteralExpressionSyntax literal && literal.Token.ValueText.Length == 0;
    }
}
