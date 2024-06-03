namespace EcoCode.Analyzers;

/// <summary>EC93 fixer: Return Task directly.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReturnTaskDirectly)), Shared]
public sealed class ReturnTaskDirectlyFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [ReturnTaskDirectly.Descriptor.Id];

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) is not { } root)
            return;

        foreach (var diagnostic in context.Diagnostics)
        {
            if (root.FindToken(diagnostic.Location.SourceSpan.Start).Parent is not { } parent)
                continue;

            foreach (var node in parent.AncestorsAndSelf())
            {
                if (node is not MethodDeclarationSyntax methodDecl) continue;

                int asyncIndex = methodDecl.Modifiers.IndexOf(SyntaxKind.AsyncKeyword);
                if (asyncIndex == -1) continue;

                if (methodDecl.ExpressionBody is { Expression: AwaitExpressionSyntax awaitExpr1 })
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Dispose resource asynchronously",
                            createChangedDocument: _ => ReturnTaskDirectlyWithExpressionAsync(context.Document, methodDecl, awaitExpr1, asyncIndex),
                            equivalenceKey: "Dispose resource asynchronously"),
                        diagnostic);
                    break;
                }
                if (methodDecl.Body?.Statements.SingleOrDefaultNoThrow() is ExpressionStatementSyntax { Expression: AwaitExpressionSyntax awaitExpr2 })
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Dispose resource asynchronously",
                            createChangedDocument: _ => ReturnTaskDirectlyWithBodyAsync(context.Document, methodDecl, awaitExpr2, asyncIndex),
                            equivalenceKey: "Dispose resource asynchronously"),
                        diagnostic);
                    break;
                }
            }
        }
    }

    private static async Task<Document> ReturnTaskDirectlyWithExpressionAsync(
        Document document,
        MethodDeclarationSyntax methodDecl,
        AwaitExpressionSyntax awaitExpr,
        int asyncIndex)
    {
        var returnStatement = SyntaxFactory.ReturnStatement(awaitExpr.Expression);

        var newBody = SyntaxFactory.ArrowExpressionClause(returnStatement.Expression!.WithTriviaFrom(awaitExpr))
            .WithTriviaFrom(methodDecl.ExpressionBody!);

        return await document.WithUpdatedRoot(methodDecl, methodDecl
            .WithModifiers(methodDecl.Modifiers.RemoveAt(asyncIndex))
            .WithExpressionBody(newBody));
    }

    private static async Task<Document> ReturnTaskDirectlyWithBodyAsync(
        Document document,
        MethodDeclarationSyntax methodDecl,
        AwaitExpressionSyntax awaitExpr,
        int asyncIndex)
    {
        var returnStatement = SyntaxFactory.ReturnStatement(awaitExpr.Expression)
            .WithLeadingTrivia(awaitExpr.GetLeadingTrivia())
            .WithTrailingTrivia(((ExpressionStatementSyntax)awaitExpr.Parent!).SemicolonToken.TrailingTrivia);

        var newBody = SyntaxFactory.Block(returnStatement)
            .WithOpenBraceToken(methodDecl.Body!.OpenBraceToken)
            .WithCloseBraceToken(methodDecl.Body.CloseBraceToken)
            .WithTriviaFrom(methodDecl.Body);

        return await document.WithUpdatedRoot(methodDecl, methodDecl
            .WithModifiers(methodDecl.Modifiers.RemoveAt(asyncIndex))
            .WithBody(newBody));
    }
}
