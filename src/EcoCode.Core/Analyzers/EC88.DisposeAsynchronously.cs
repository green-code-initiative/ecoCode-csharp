namespace EcoCode.Analyzers;

/// <summary>EC88: Dispose asynchronously.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DisposeAsynchronously : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> UsingStatementKinds = [SyntaxKind.UsingStatement];
    private static readonly ImmutableArray<SyntaxKind> UsingDeclarationKinds = [SyntaxKind.LocalDeclarationStatement];

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
        context.RegisterSyntaxNodeAction(static context => AnalyzeUsingStatement(context), UsingStatementKinds);
        context.RegisterSyntaxNodeAction(static context => AnalyzeUsingDeclaration(context), UsingDeclarationKinds);
    }

    private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
    {
        var statement = (UsingStatementSyntax)context.Node;
        if (!statement.AwaitKeyword.IsKind(SyntaxKind.AwaitKeyword) &&
            statement.Declaration is { } declaration &&
            CanBeDisposedAsynchronously(context, declaration.Variables) &&
            IsContainedInAsyncMethod(statement))
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, statement.UsingKeyword.GetLocation()));
        }
    }

    private static void AnalyzeUsingDeclaration(SyntaxNodeAnalysisContext context)
    {
        var statement = (LocalDeclarationStatementSyntax)context.Node;
        if (statement.UsingKeyword.IsKind(SyntaxKind.UsingKeyword) &&
            !statement.AwaitKeyword.IsKind(SyntaxKind.AwaitKeyword) &&
            CanBeDisposedAsynchronously(context, statement.Declaration.Variables) &&
            IsContainedInAsyncMethod(statement))
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, statement.UsingKeyword.GetLocation()));
        }
    }

    private static bool CanBeDisposedAsynchronously(SyntaxNodeAnalysisContext context, SeparatedSyntaxList<VariableDeclaratorSyntax> variables) =>
        variables.Count == 1 && variables[0].Initializer?.Value is { } expr && // Don't handle multiple variables declarations
        context.SemanticModel.GetTypeInfo(expr, context.CancellationToken).Type is INamedTypeSymbol namedTypeSymbol &&
        context.Compilation.GetTypeByMetadataName(typeof(IAsyncDisposable).FullName) is { } asyncDisposableType &&
        namedTypeSymbol.AllInterfaces.Contains(asyncDisposableType, SymbolEqualityComparer.Default);

    private static bool IsContainedInAsyncMethod(StatementSyntax statement)
    {
        for (var node = statement.Parent; node is not null; node = node.Parent)
        {
            switch (node)
            {
                case MemberDeclarationSyntax:
                    return node is MethodDeclarationSyntax methodDeclaration && methodDeclaration.Modifiers.Contains(SyntaxKind.AsyncKeyword);
                case LocalFunctionStatementSyntax localFunction:
                    return localFunction.Modifiers.Contains(SyntaxKind.AsyncKeyword);
                case LambdaExpressionSyntax lambdaExpression:
                    return lambdaExpression.Modifiers.Contains(SyntaxKind.AsyncKeyword);
                case AnonymousMethodExpressionSyntax anonymousMethod:
                    return anonymousMethod.Modifiers.Contains(SyntaxKind.AsyncKeyword);
                case LockStatementSyntax:
                    return false;
            }
        }
        return false;
    }
}
