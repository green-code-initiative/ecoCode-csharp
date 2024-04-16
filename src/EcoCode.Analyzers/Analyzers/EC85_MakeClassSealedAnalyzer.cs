namespace EcoCode.Analyzers;

/// <summary>Analyzer for make class sealed.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MakeClassSealedAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = new(
        Rule.Ids.EC85_MakeClassSealed,
        title: "Make class sealed",
        messageFormat: "Make class sealed",
        Rule.Categories.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: null,
        helpLinkUri: Rule.GetHelpUri(Rule.Ids.EC85_MakeClassSealed));

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            var sealableClasses = new List<INamedTypeSymbol>();
            var inheritedClasses = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            compilationStartContext.RegisterSymbolAction(context =>
            {
                if (context.Symbol is not INamedTypeSymbol symbol || symbol.TypeKind is not TypeKind.Class || symbol.IsStatic)
                    return;

                var baseType = symbol.BaseType;
                if (baseType?.TypeKind is TypeKind.Class)
                    _ = inheritedClasses.Add(baseType);

                if (symbol.IsAbstract || symbol.IsSealed || symbol.IsImplicitlyDeclared || symbol.IsImplicitClass)
                    return;

                foreach (var member in symbol.GetMembers())
                    if (member.IsVirtual) return;

                sealableClasses.Add(symbol);
            }, SymbolKind.NamedType);

            compilationStartContext.RegisterCompilationEndAction(compilationEndContext =>
            {
                foreach (var cls in sealableClasses)
                {
                    if (inheritedClasses.Contains(cls)) continue;
                    compilationEndContext.ReportDiagnostic(Diagnostic.Create(Descriptor, cls.Locations[0]));
                }
            });
        });
    }
}
