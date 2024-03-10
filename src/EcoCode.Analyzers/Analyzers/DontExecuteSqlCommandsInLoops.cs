namespace EcoCode.Analyzers;

/// <summary>Analyzer for don't execute SQL commands in loops.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DontExecuteSqlCommandsInLoops : DiagnosticAnalyzer
{
    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = new(
        Rule.Ids.DontExecuteSqlCommandsInLoops,
        title: "Don't execute SQL commands in loops",
        messageFormat: "Don't execute SQL commands in loops",
        Rule.Categories.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: string.Empty,
        helpLinkUri: Rule.GetHelpUri(Rule.Ids.DontExecuteSqlCommandsInLoops));

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            if (compilationStartContext.Compilation.GetTypeByMetadataName("System.Data.IDbCommand") is not INamedTypeSymbol iDbCommandTypeSymbol)
                return;

            compilationStartContext.RegisterSyntaxNodeAction(context =>
            {
                if (context.SemanticModel.GetSymbolInfo((InvocationExpressionSyntax)context.Node).Symbol is IMethodSymbol symbol &&
                    symbol.ImplementsInterface(iDbCommandTypeSymbol) &&
                    symbol.Name is "ExecuteNonQuery" or "ExecuteScalar" or "ExecuteReader" &&
                    context.Node.IsInsideLoop())
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
                }
            }, SyntaxKind.InvocationExpression);
        });
    }
}