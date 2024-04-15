namespace EcoCode.Analyzers;

/// <summary>Analyzer for avoid async void method.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidAsyncVoidMethodAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = new(
        Rule.Ids.EC84_AvoidAsyncVoidMethod,
        title: "Avoid async void method",
        messageFormat: "Avoid async void method",
        Rule.Categories.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: null,
        helpLinkUri: Rule.GetHelpUri(Rule.Ids.EC84_AvoidAsyncVoidMethod));

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(static context => AnalyzeMethod(context), SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        if (methodDeclaration.Modifiers.Any(SyntaxKind.AsyncKeyword) &&
            methodDeclaration.ReturnType is PredefinedTypeSyntax returnType &&
            returnType.Keyword.IsKind(SyntaxKind.VoidKeyword))
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, methodDeclaration.Identifier.GetLocation()));
        }
    }
}
