namespace EcoCode.Analyzers;

/// <summary>Analyzer for use struct layout.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseStructLayout : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SymbolKind> SymbolKinds = [SymbolKind.NamedType];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = new(
        Rule.Ids.EC81_UseStructLayout,
        title: "Use struct layout",
        messageFormat: "Use struct layout",
        Rule.Categories.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: string.Empty,
        helpLinkUri: Rule.GetHelpUri(Rule.Ids.EC81_UseStructLayout));

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(static context => Analyze(context), SymbolKinds);
    }

    private static void Analyze(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol symbol || !symbol.IsValueType || symbol.EnumUnderlyingType is not null)
            return;

        var structLayoutAttributeType = context.Compilation.GetBestTypeByMetadataName("System.Runtime.InteropServices.StructLayoutAttribute");
        if (structLayoutAttributeType is null) return;

        foreach (var attr in symbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(structLayoutAttributeType, attr.AttributeClass))
                return;
        }

        int memberCount = 0;
        foreach (var member in symbol.GetMembers())
        {
            if (member is not IFieldSymbol fieldSymbol || fieldSymbol.IsConst || fieldSymbol.IsStatic)
                continue;

            if (fieldSymbol.Type.IsReferenceType) return; // A struct containing a reference type is always in auto layout
            memberCount++;
        }

        if (memberCount < 2) return;
        foreach (var location in symbol.Locations)
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
    }
}
