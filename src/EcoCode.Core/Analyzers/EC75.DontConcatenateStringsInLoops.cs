namespace EcoCode.Analyzers;

/// <summary>EC75: Don't concatenate strings in loops.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DontConcatenateStringsInLoops : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> SyntaxKinds = [
        SyntaxKind.ForStatement,
        SyntaxKind.ForEachStatement,
        SyntaxKind.WhileStatement,
        SyntaxKind.DoStatement];
    private static readonly ImmutableArray<OperationKind> Invocations = [OperationKind.Invocation];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.EC75_DontConcatenateStringsInLoops,
        title: "Don't concatenate strings in loops",
        message: "A string is concatenated in a loop",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "Strings should not be concatenated in loops, use a more optimized way, such as a StringBuilder.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(static context => AnalyzeLoopNode(context), SyntaxKinds);
        context.RegisterOperationAction(static context => AnalyzeForEach(context), Invocations);
    }

    private static void AnalyzeLoopNode(SyntaxNodeAnalysisContext context)
    {
        foreach (var loopStatement in context.Node.GetLoopStatements())
        {
            if (loopStatement is ExpressionStatementSyntax expressionStatement &&
                expressionStatement.Expression is AssignmentExpressionSyntax assignment &&
                assignment.IsKind(SyntaxKind.AddAssignmentExpression) &&
                assignment.Left is IdentifierNameSyntax identifierName &&
                context.SemanticModel.GetSymbolInfo(identifierName).Symbol is ISymbol symbol &&
                symbol.IsVariableOfType(SpecialType.System_String) &&
                symbol.IsDeclaredOutsideLoop(context.Node))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, assignment.GetLocation()));
            }
        }
    }

    private static void AnalyzeForEach(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation { TargetMethod.Name: "ForEach" } operation ||
            GetDelegateArgument(operation, context.Compilation)?.Value is not IDelegateCreationOperation { Target: { } body })
        {
            return;
        }

        foreach (var op in body.Descendants())
        {
            var target = default(IOperation);

            if (op is ICompoundAssignmentOperation compoundAssignment && compoundAssignment.OperatorKind is BinaryOperatorKind.Add)
                target = compoundAssignment.Target;

            if (target?.Type?.SpecialType is not SpecialType.System_String) continue;

            bool shouldReport = false;

            if (target is IFieldReferenceOperation or IPropertyReferenceOperation)
            {
                shouldReport = true;
            }
            else if (target is ILocalReferenceOperation localReference)
            {
                var declaringSyntax = localReference.Local.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                if (declaringSyntax is not null && !body.Syntax.Span.Contains(declaringSyntax.Span))
                    shouldReport = true;
            }
            else if (target is IParameterReferenceOperation parameterReference)
            {
                var parameterSymbol = parameterReference.Parameter;
                if (!SymbolEqualityComparer.Default.Equals(parameterSymbol.ContainingSymbol, body as ISymbol))
                    shouldReport = true;
            }

            if (shouldReport)
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, op.Syntax.GetLocation()));
        }

        static IArgumentOperation? GetDelegateArgument(IInvocationOperation operation, Compilation compilation)
        {
            var symbol = operation.TargetMethod.ContainingType.OriginalDefinition;
            if (operation.TargetMethod.ContainingType.IsStatic) // Parallel.ForEach<T>, the delegate to analyze is always called 'body'
            {
                if (SymbolEqualityComparer.Default.Equals(symbol, compilation.GetTypeByMetadataName("System.Threading.Tasks.Parallel")))
                    return operation.Arguments.FirstOrDefault(a => a.Parameter?.Name == "body");
            }
            else if (operation.TargetMethod.IsStatic) // Array : static void ForEach<T>(T[] array, Action<T> action)
            {
                if (SymbolEqualityComparer.Default.Equals(symbol, compilation.GetTypeByMetadataName("System.Array")))
                    return operation.Arguments[1];
            }
            else if ( // List<T> and ImmutableList<T> : void ForEach(Action<T> action)
                SymbolEqualityComparer.Default.Equals(symbol, compilation.GetTypeByMetadataName("System.Collections.Generic.List`1")) ||
                SymbolEqualityComparer.Default.Equals(symbol, compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableList`1")))
            {
                return operation.Arguments[0];
            }
            return null;
        }
    }
}
