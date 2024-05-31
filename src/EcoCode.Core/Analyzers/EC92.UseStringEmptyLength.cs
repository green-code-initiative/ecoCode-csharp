namespace EcoCode.Analyzers;

/// <summary>EC92: Use string.Length instead of comparison with empty string.</summary>
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
        context.RegisterSyntaxNodeAction(static context => AnalyzeComparison(context), EqualsExpression);
        context.RegisterSyntaxNodeAction(static context => AnalyzeComparison(context), NotEqualsExpression);
    }

    private static void AnalyzeComparison(SyntaxNodeAnalysisContext context)
    {
        var binaryExpression = (BinaryExpressionSyntax)context.Node;
        var (left, right) = (binaryExpression.Left, binaryExpression.Right);
        if (IsStringLiteral(left, context.SemanticModel) && IsEmptyString(right) || IsStringLiteral(right, context.SemanticModel) && IsEmptyString(left))
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, binaryExpression.GetLocation()));

        static bool IsStringLiteral(ExpressionSyntax expression, SemanticModel semanticModel) =>
            semanticModel.GetTypeInfo(expression).Type?.SpecialType is SpecialType.System_String;

        static bool IsEmptyString(ExpressionSyntax expression) =>
            expression is LiteralExpressionSyntax { Token.ValueText.Length: 0 };
    }
}
