using System.Collections.Concurrent;

namespace EcoCode.Analyzers;

/// <summary>Analyzer for make type sealed.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MakeTypeSealedAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = new(
        Rule.Ids.EC85_MakeTypeSealed,
        title: "Make type sealed",
        messageFormat: "Make type sealed",
        Rule.Categories.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: null,
        helpLinkUri: Rule.GetHelpUri(Rule.Ids.EC85_MakeTypeSealed));

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
            // Concurrent collections as RegisterSymbolAction is called once per symbol and can be parallelized
            var sealableClasses = new ConcurrentBag<INamedTypeSymbol>();
            var inheritedClasses = new ConcurrentDictionary<INamedTypeSymbol, bool>(SymbolEqualityComparer.Default);

            compilationStartContext.RegisterSymbolAction(analysisContext =>
            {
                if (analysisContext.Symbol is not INamedTypeSymbol symbol || symbol.TypeKind is not TypeKind.Class || symbol.IsStatic)
                    return;

                if (symbol.BaseType is INamedTypeSymbol { IsAbstract: false, SpecialType: SpecialType.None, TypeKind: TypeKind.Class } baseType)
                    _ = inheritedClasses.TryAdd(baseType, true);

                if (symbol.IsAbstract || symbol.IsSealed || symbol.IsScriptClass || symbol.IsImplicitlyDeclared || symbol.IsImplicitClass)
                    return;

                // Exclude types that are externally public (ie. inheritable from another assembly)
                // AND have at least one externally public or protected overridable member
                // An externally public type can still be inherited from without the second condition
                // But in that case we'll let the user decide whether to seal it or not, and mute the warning if so
                if (!symbol.IsExternallyPublic() || !symbol.HasAnyExternallyOverridableMember())
                    sealableClasses.Add(symbol);
            }, SymbolKind.NamedType);

            compilationStartContext.RegisterCompilationEndAction(compilationEndContext =>
            {
                foreach (var cls in sealableClasses)
                {
                    if (inheritedClasses.ContainsKey(cls)) continue;
                    // foreach (var location in cls.Locations) compilationEndContext.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
                    // TODO: handle multiple locations when partial, choose one smartly
                    compilationEndContext.ReportDiagnostic(Diagnostic.Create(Descriptor, cls.Locations[0]));
                }
            });
        });
    }
}
