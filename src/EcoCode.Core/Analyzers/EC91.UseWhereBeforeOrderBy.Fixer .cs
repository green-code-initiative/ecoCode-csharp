using System.Linq;

namespace EcoCode.Analyzers;

/// <summary>EC91 fixer: Use Where before OrderBy.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseWhereBeforeOrderByFixer)), Shared]
public sealed class UseWhereBeforeOrderByFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [UseWhereBeforeOrderBy.Descriptor.Id];

    /// <inheritdoc/>
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
                if (node is InvocationExpressionSyntax invocation)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Use Where before OrderBy",
                            createChangedDocument: token => RefactorMethodSyntaxAsync(context.Document, invocation, token),
                            equivalenceKey: "Use Where before OrderBy"),
                        diagnostic);
                    break;
                }
                if (node is QueryExpressionSyntax query)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Use Where before OrderBy",
                            createChangedDocument: token => RefactorQuerySyntaxAsync(context.Document, query, token),
                            equivalenceKey: "Use Where before OrderBy"),
                        diagnostic);
                    break;
                }
            }
        }
    }

    private static async Task<Document> RefactorMethodSyntaxAsync(Document document, InvocationExpressionSyntax whereInvocation, CancellationToken token)
    {
        if (await document.GetSyntaxRootAsync(token).ConfigureAwait(false) is not { } root ||
            whereInvocation.Expression is not MemberAccessExpressionSyntax whereMemberAccess ||
            whereMemberAccess.Expression is not InvocationExpressionSyntax orderByInvocation ||
            orderByInvocation.Expression is not MemberAccessExpressionSyntax orderByMemberAccess)
        {
            return document;
        }

        var newWhereInvocation = whereInvocation
            .WithExpression(whereMemberAccess.WithExpression(orderByInvocation.Expression))
            .WithTriviaFrom(orderByInvocation);

        /*var newOrderByInvocation = orderByInvocation
            .WithExpression(orderByMemberAccess.WithExpression(newWhereInvocation))
            .WithTriviaFrom(whereInvocation);*/

        var newRoot = root
            .ReplaceNode(whereInvocation, newWhereInvocation);
            // .ReplaceNode(orderByInvocation, newOrderByInvocation);

        /*var newOrderByExpression = orderByMemberAccess.WithExpression(whereMemberAccess.Expression);
        var newOrderByInvocation = orderByInvocation.WithExpression(newOrderByExpression);

        var newWhereExpression = whereMemberAccess.WithExpression(orderByInvocation);
        var newWhereInvocation = whereInvocation.WithExpression(newWhereExpression);*/

        /*var newOrderByInvocation = orderByInvocation.WithTriviaFrom(whereInvocation);
        var newWhereInvocation = whereInvocation.WithTriviaFrom(orderByInvocation);

        var newRoot = root
            .ReplaceNode(orderByInvocation, newWhereInvocation)
            .ReplaceNode(whereInvocation, newOrderByInvocation);*/

        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> RefactorQuerySyntaxAsync(Document document, QueryExpressionSyntax query, CancellationToken token)
    {
        if (await document.GetSyntaxRootAsync(token).ConfigureAwait(false) is not { } root)
            return document;

        var clauses = query.Body.Clauses;
        for (int i = 0; i < clauses.Count - 1; i++)
        {
            if (clauses[i] is not OrderByClauseSyntax) continue;

            for (int j = i + 1; j < clauses.Count; j++) // To handle multiple OrderBy followed by a Where
            {
                var nextClause = clauses[j];
                if (nextClause is WhereClauseSyntax whereClause)
                {
                    var newClauses = clauses.ToList();
                    newClauses.RemoveAt(j);
                    newClauses.Insert(i, whereClause);
                    return document.WithSyntaxRoot(root.ReplaceNode(query, query.WithBody(query.Body.WithClauses(SyntaxFactory.List(newClauses)))));
                }
                if (nextClause is not OrderByClauseSyntax)
                {
                    i = j; // Skip processed clauses
                    break;
                }
            }
        }

        return document;
    }
}
