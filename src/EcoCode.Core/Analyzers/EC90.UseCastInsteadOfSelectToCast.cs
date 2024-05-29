namespace EcoCode.Core.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseCastInsteadOfSelectToCast : DiagnosticAnalyzer
    {
        public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
            id: Rule.Ids.EC90_UseCastInsteadOfSelectToCast,
            title: "Use Cast instead of Select to cast",
            message: "A Select method is used for casting instead of the Cast method",
            category: Rule.Categories.Performance,
            severity: DiagnosticSeverity.Warning,
            description: "The Cast method should be used instead of Select for casting to improve performance.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
        private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(static context => AnalyzeSelectNode(context), SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeSelectNode(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;

            // Check if the method being called is 'Select'
            if (memberAccess?.Name.Identifier.Text != "Select") return;

            // Check if the argument to 'Select' is a cast operation
            if ((invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression as SimpleLambdaExpressionSyntax)?.Body is CastExpressionSyntax)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.GetLocation()));
            }

        }

    }
}
