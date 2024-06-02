namespace EcoCode.Analyzers;

/// <summary>EC93: Return Task directly.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReturnTaskDirectly : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> MethodDeclarations = [SyntaxKind.MethodDeclaration];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.EC93_ReturnTaskDirectly,
        title: "Consider returning Task directly",
        message: "Consider returning a Task directly instead of awaiting a single statement",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Info,
        description: "Consider returning a Task directly instead of awaiting a single statement, as this can save performance.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(static context => AnalyzeSyntaxNode(context), MethodDeclarations);
    }

    private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        int asyncIndex = methodDeclaration.Modifiers.IndexOf(SyntaxKind.AsyncKeyword);
        if (asyncIndex == -1) return;

        if (methodDeclaration.ExpressionBody is { Expression: AwaitExpressionSyntax } ||
            methodDeclaration.Body?.Statements.SingleOrDefault() is ExpressionStatementSyntax { Expression: AwaitExpressionSyntax })
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, methodDeclaration.Modifiers[asyncIndex].GetLocation()));
        }
    }
}
