using Microsoft.CodeAnalysis.Formatting;
using System.Linq;
using System.Text.RegularExpressions;

namespace EcoCode.Analyzers;

/// <summary>EC91 With LINQ use Where before Order by.</summary>
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
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        foreach (var diagnostic in context.Diagnostics)
        {
            if (root.FindToken(diagnostic.Location.SourceSpan.Start).Parent is not { } parent)
                continue;

            foreach (var node in parent.AncestorsAndSelf())
            {
                if (node is QueryExpressionSyntax queryExpression)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "With LINQ move 'where' before 'orderby'",
                            createChangedDocument: c => MoveWhereBeforeOrderByAsync(context.Document, queryExpression, c),
                            equivalenceKey: "Use Where before Order by"),
                        diagnostic);
                }
                else if (node is InvocationExpressionSyntax invocationExpr)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "With LINQ move 'Where' before 'OrderBy'",
                            createChangedDocument: c => MoveWhereBeforeOrderByWithObjectAsync(context.Document, invocationExpr, c),
                            equivalenceKey: "Use Where before Order by"),
                        diagnostic);
                }
            }
        }
    }

    private static async Task<Document> MoveWhereBeforeOrderByAsync(Document document, QueryExpressionSyntax queryExpression, CancellationToken cancellationToken)
    {
        if (queryExpression.Body.Clauses.OfType<WhereClauseSyntax>().FirstOrDefault() is not { } whereClause ||
            queryExpression.Body.Clauses.OfType<OrderByClauseSyntax>().FirstOrDefault() is not { } orderByClause)
        {
            return document;
        }

        if (whereClause is null || orderByClause is null) return document;

        int whereIndex = queryExpression.Body.Clauses.IndexOf(whereClause);
        int orderByIndex = queryExpression.Body.Clauses.IndexOf(orderByClause);

        if (orderByIndex < whereIndex && await document.GetSyntaxRootAsync(cancellationToken) is { } root)
        {
            var clauses = queryExpression.Body.Clauses.ToList();
            clauses.RemoveAt(orderByIndex);
            clauses.Insert(whereIndex, orderByClause);

            var newClauses = SyntaxFactory.List(clauses);
            var newQueryBody = queryExpression.Body.WithClauses(newClauses);
            var newQueryExpression = queryExpression.WithBody(newQueryBody);

            return document.WithSyntaxRoot(root.ReplaceNode(queryExpression, newQueryExpression));
        }
        return document;
    }

    private static async Task<Document> MoveWhereBeforeOrderByWithObjectAsync(Document document, InvocationExpressionSyntax invocationExpr, CancellationToken cancellationToken)
    {
        var chain = new List<InvocationExpressionSyntax>();
        var currentExpr = invocationExpr;

        while (currentExpr != null)
        {
            chain.Add(currentExpr);
            var currentMemberAccess = currentExpr.Expression as MemberAccessExpressionSyntax;
            currentExpr = currentMemberAccess?.Expression as InvocationExpressionSyntax;
        }

        chain.Reverse();

        InvocationExpressionSyntax? whereInvocation = null;
        InvocationExpressionSyntax? orderByInvocation = null;

        foreach (var c in chain)
        {
            string methodName = ((MemberAccessExpressionSyntax)c.Expression).Name.Identifier.Text;
            if (methodName is "Where")
                whereInvocation = c;
            else if (methodName is "OrderBy" or "OrderByDescending")
                orderByInvocation = c;

            if (whereInvocation is not null && orderByInvocation is not null)
                break;
        }

        if (whereInvocation is not null && orderByInvocation is not null)
        {
            string textModified = Regex.Replace(whereInvocation.GetText().ToString(), @".OrderBy\s*\(.*?\)", "", RegexOptions.IgnoreCase);
            textModified = Regex.Replace(textModified, @".OrderByDescending\s*\(.*?\)", "", RegexOptions.IgnoreCase);
            var whereInvocationOnlyWhere = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression(textModified);

            int whereIndex = chain.IndexOf(whereInvocation);
            int orderByIndex = chain.IndexOf(orderByInvocation);

            if (whereIndex > orderByIndex)
            {
                var newChain = new List<InvocationExpressionSyntax>();
                bool orderByAdded = false;

                foreach (var expr in chain)
                {
                    if (expr == orderByInvocation && !orderByAdded)
                        continue;

                    if (expr == whereInvocation && !orderByAdded)
                    {
                        newChain.Add(whereInvocationOnlyWhere);
                        newChain.Add(orderByInvocation);
                        orderByAdded = true;
                    }
                    else
                    {
                        newChain.Add(expr);
                    }
                }

                var newInvocationExpr = newChain[0];
                for (int i = 1; i < newChain.Count; i++)
                {
                    var memberAccess = (MemberAccessExpressionSyntax)newChain[i].Expression;
                    newInvocationExpr = newChain[i].WithExpression(memberAccess.WithExpression(newInvocationExpr));
                }

                var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                if (root is not null)
                {
                    return document.WithSyntaxRoot(root
                        .ReplaceNode(invocationExpr, newInvocationExpr)
                        .WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation));
                }
            }
        }

        return document;
    }
}
