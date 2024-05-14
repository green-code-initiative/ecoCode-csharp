namespace EcoCode.Analyzers;

/// <summary>EC88: Dispose asynchronously.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DisposeAsynchronously : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> SyntaxKinds = [SyntaxKind.UsingStatement];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = new(
        Rule.Ids.EC88_DisposeAsynchronously,
        title: "Dispose asynchronously",
        messageFormat: "A resource can be disposed asynchronously",
        Rule.Categories.Usage,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Resources that implement IAsyncDisposable should be disposed asynchronously within asynchronous methods.",
        helpLinkUri: Rule.GetHelpUri(Rule.Ids.EC88_DisposeAsynchronously));

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(static context => AnalyzeUsingStatement(context), SyntaxKinds);
    }

    private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
    {
        var usingStatement = (UsingStatementSyntax)context.Node;
        if (usingStatement.Declaration is not { } declaration) return;

        var typeInfo = context.SemanticModel.GetTypeInfo(declaration.Type);

        if (typeInfo.Type is INamedTypeSymbol namedTypeSymbol &&
            context.Compilation.GetTypeByMetadataName(typeof(IAsyncDisposable).FullName) is { } asyncDisposableType &&
            namedTypeSymbol.AllInterfaces.Contains(asyncDisposableType) &&
            context.SemanticModel.GetEnclosingSymbol(context.Node.SpanStart) is IMethodSymbol methodSymbol &&
            methodSymbol.IsAsync)
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, usingStatement.GetLocation()));
        }
    }
}
