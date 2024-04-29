using System.Reflection;

namespace EcoCode.Analyzers;

/// <summary>EC87 fixer: Use collection indexer.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCollectionIndexerFixer)), Shared]
public sealed class UseCollectionIndexerFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [UseCollectionIndexer.Descriptor.Id];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        foreach (var diagnostic in context.Diagnostics)
        {
            var parent = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;
            if (parent is null) continue;

            foreach (var node in parent.AncestorsAndSelf())
            {
                if (node is not InvocationExpressionSyntax invocation || invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                    continue;

                var refactorFunc = memberAccess.Name.Identifier.ValueText switch
                {
                    nameof(System.Linq.Enumerable.First) => RefactorFirstAsync,
                    nameof(System.Linq.Enumerable.Last) =>
                        context.Document.GetLanguageVersion() >= LanguageVersion.CSharp8
                        ? RefactorLastWithIndexerAsync
                        : RefactorLastWithCountOrLengthAsync,
                    nameof(System.Linq.Enumerable.ElementAt) => RefactorElementAtAsync,
                    _ => default(Func<Document, InvocationExpressionSyntax, CancellationToken, Task<Document>>),
                };

                if (refactorFunc is null) continue;
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "Use collection indexer",
                        createChangedDocument: token => refactorFunc(context.Document, invocation, token),
                        equivalenceKey: "Use collection indexer"),
                    diagnostic);
                break;
            }
        }
    }

    private static async Task<Document> UpdateDocument(Document document, InvocationExpressionSyntax invocation, ExpressionSyntax indexExpr, CancellationToken token)
    {
        if (await document.GetSyntaxRootAsync(token) is not SyntaxNode root)
            return document;

        var elementAccess = SyntaxFactory.ElementAccessExpression(
            ((MemberAccessExpressionSyntax)invocation.Expression).Expression,
            SyntaxFactory.BracketedArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(indexExpr))));

        return document.WithSyntaxRoot(root.ReplaceNode(invocation, elementAccess));
    }

    private static Task<Document> RefactorFirstAsync(Document document, InvocationExpressionSyntax invocationExpr, CancellationToken token) =>
        UpdateDocument(document, invocationExpr, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0)), token);

    private static Task<Document> RefactorLastWithIndexerAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken token) =>
        UpdateDocument(document, invocation,
            SyntaxFactory.PrefixUnaryExpression(SyntaxKind.IndexExpression,
                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1))),
            token);

    private static async Task<Document> RefactorLastWithCountOrLengthAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken token)
    {
        if (await document.GetSemanticModelAsync(token).ConfigureAwait(false) is not SemanticModel semanticModel)
            return document;

        var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
        if (semanticModel.GetTypeInfo(memberAccess.Expression, token).Type?.GetCountOrLength(invocation.SpanStart, semanticModel) is not { } countOrLength)
            return document;

        var propertyAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            memberAccess.Expression,
            SyntaxFactory.IdentifierName(countOrLength.Name));

        var minusOneLiteral = SyntaxFactory.LiteralExpression(
            SyntaxKind.NumericLiteralExpression,
            SyntaxFactory.Literal(1));

        var indexExpression = SyntaxFactory.BinaryExpression(
            SyntaxKind.SubtractExpression,
            propertyAccess,
            minusOneLiteral);

        return await UpdateDocument(document, invocation, indexExpression, token);
    }

    private static Task<Document> RefactorElementAtAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken token) =>
        UpdateDocument(document, invocation, invocation.ArgumentList.Arguments[0].Expression, token);
}
