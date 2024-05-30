using System.Linq;

namespace EcoCode.Analyzers;

/// <summary>EC89 : Unnecessary assignment.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnnecessaryAssignment : DiagnosticAnalyzer
{
    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
       id: Rule.Ids.EC89_UnnecessaryAssignment,
       title: "Unused variable assignment",
       message: "Variable '{0}' is assigned unnecessarily.",
       category: Rule.Categories.Usage,
       severity: DiagnosticSeverity.Warning,
       description: "Detects variables assigned in both branches of an if-else statement and used immediately after.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
    }

    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;
        var blockSyntax = ifStatement.Parent as BlockSyntax;

        if (blockSyntax == null) return;
        if (blockSyntax.Kind() is not SyntaxKind.ElseClause && ifStatement.Else is null) return;
        if (blockSyntax.Kind() is not SyntaxKind.Block) return;

        if (ContainsPolymorphism(blockSyntax)) return;
        
        context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
    }

    internal static bool ContainsPolymorphism(BlockSyntax block)
    {
        var tree = block.SyntaxTree;
        var compilation = CSharpCompilation.Create("PolymorphismDetection")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(tree);

        var model = compilation.GetSemanticModel(tree);

        var assignments = block.DescendantNodes().OfType<AssignmentExpressionSyntax>()
            .Where(a => a.Kind() == SyntaxKind.SimpleAssignmentExpression)
            .ToList();

        foreach (var assignment in assignments)
        {
            var leftSymbol = model.GetSymbolInfo(assignment.Left).Symbol as ILocalSymbol;
            if (leftSymbol != null)
            {
                var rightType = model.GetTypeInfo(assignment.Right).Type;

                if (rightType != null)
                {
                    var baseType = rightType.BaseType;
                    var typesAssigned = assignments
                        .Select(a => model.GetTypeInfo(a.Right).Type)
                        .Where(t => t != null && t.BaseType != null && t.BaseType.Equals(baseType))
                        .Distinct();

                    if (typesAssigned.Count() > 1)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
