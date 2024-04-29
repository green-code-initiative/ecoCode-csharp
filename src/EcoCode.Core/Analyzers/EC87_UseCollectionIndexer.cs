using System.Linq;

namespace EcoCode.Analyzers;

/// <summary>EC87: Use collection indexer.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseCollectionIndexer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> SyntaxKinds = [SyntaxKind.InvocationExpression];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = new(
        Rule.Ids.EC87_UseCollectionIndexer,
        title: "Use collection indexer",
        messageFormat: "A collection indexer should be used instead of a Linq method",
        Rule.Categories.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "When a collection has an indexer, it should be used instead of some Linq methods for performance reasons.",
        helpLinkUri: Rule.GetHelpUri(Rule.Ids.EC87_UseCollectionIndexer));

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(static context => AnalyzeInvocationExpression(context), SyntaxKinds);
    }

    private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
    {
        var invocationExpr = (InvocationExpressionSyntax)context.Node;
        if (context.SemanticModel.GetSymbolInfo(invocationExpr).Symbol is not IMethodSymbol method || !method.IsLinqMethod(context.Compilation))
            return;

        var containingType = context.ContainingSymbol as INamedTypeSymbol ?? context.ContainingSymbol?.ContainingType;
        if (containingType is null) return;

        var memberAccess = (MemberAccessExpressionSyntax)invocationExpr.Expression;
        if (context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type is not { } collectionType)
            return;

        // TODO: analysis can be improved by including scenarios with method chains
        // For example: myList.Skip(5).First() should be refactored to myList[5]

        bool report = method.Name switch
        {
            nameof(Enumerable.First) =>
                !invocationExpr.ArgumentList.Arguments.Any() &&
                collectionType.GetIndexer() is { } indexer &&
                context.Compilation.IsSymbolAccessibleWithin(indexer, containingType),

            nameof(Enumerable.Last) =>
                !invocationExpr.ArgumentList.Arguments.Any() &&
                collectionType.GetIndexer() is { } indexer &&
                context.Compilation.IsSymbolAccessibleWithin(indexer, containingType) &&
                collectionType.GetCountOrLength(containingType, context.Compilation) is not null,

            nameof(Enumerable.ElementAt) =>
                invocationExpr.ArgumentList.Arguments.Count == 1 &&
                collectionType.GetIndexer() is { } indexer &&
                context.Compilation.IsSymbolAccessibleWithin(indexer, containingType),

            _ => false,
        };

        if (report) context.ReportDiagnostic(Diagnostic.Create(Descriptor, memberAccess.GetLocation()));
    }
}
