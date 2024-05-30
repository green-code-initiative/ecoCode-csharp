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
            var parent = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;
            if (parent is null) continue;
            if (diagnostic is null) continue;

            foreach (var node in parent.AncestorsAndSelf())
            {
                if (node is QueryExpressionSyntax queryExpression)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "With LINQ move 'where' before 'orderby'",
                            createChangedDocument: c => MoveWhereBeforeOrderBy(context.Document, queryExpression, c),
                            equivalenceKey: nameof(MoveWhereBeforeOrderBy)),
                        diagnostic);
                }
                else if (node is InvocationExpressionSyntax invocationExpr)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "With LINQ move 'Where' before 'OrderBy'",
                            createChangedDocument: c => MoveWhereBeforeOrderByWithObject(context.Document, invocationExpr, c),
                            equivalenceKey: nameof(MoveWhereBeforeOrderByWithObject)),
                        diagnostic);
                }
            }
        }
    }

    private async Task<Document> MoveWhereBeforeOrderBy(Document document, QueryExpressionSyntax queryExpression, CancellationToken cancellationToken)
    {
        if (queryExpression.Body.Clauses.OfType<WhereClauseSyntax>().FirstOrDefault() is not { } whereClause ||
        queryExpression.Body.Clauses.OfType<OrderByClauseSyntax>().FirstOrDefault() is not { } orderByClause)
        {
            return document;
        }
        if (whereClause == null || orderByClause == null) return document;

        var whereIndex = queryExpression.Body.Clauses.IndexOf(whereClause);
        var orderByIndex = queryExpression.Body.Clauses.IndexOf(orderByClause);

        if (orderByIndex < whereIndex)
        {
            var clauses = queryExpression.Body.Clauses.ToList();
            clauses.RemoveAt(orderByIndex);
            clauses.Insert(whereIndex, orderByClause);

            var newClauses = SyntaxFactory.List(clauses);
            var newQueryBody = queryExpression.Body.WithClauses(newClauses);
            var newQueryExpression = queryExpression.WithBody(newQueryBody);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            if (root != null)
            {
                var newRoot = root.ReplaceNode(queryExpression, newQueryExpression);

                return document.WithSyntaxRoot(newRoot);
            }
        }
        return document;
    }

    private async Task<Document> MoveWhereBeforeOrderByWithObject(Document document, InvocationExpressionSyntax invocationExpr, CancellationToken cancellationToken)
    {
        var memberAccessExpr = (MemberAccessExpressionSyntax)invocationExpr.Expression;
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
            var methodName = ((MemberAccessExpressionSyntax)c.Expression).Name.Identifier.Text;
            if (methodName == "Where")
            {
                whereInvocation = c;
            }
            if (methodName == "OrderBy" || methodName == "OrderByDescending")
            {
                orderByInvocation = c;
            }
            if (whereInvocation != null && orderByInvocation != null)
            {
                break;
            }
        }

        if (whereInvocation != null && orderByInvocation != null)
        {
            var textModified = Regex.Replace(whereInvocation.GetText().ToString(), @".OrderBy\s*\(.*?\)", "", RegexOptions.IgnoreCase);
            textModified = Regex.Replace(textModified, @".OrderByDescending\s*\(.*?\)", "", RegexOptions.IgnoreCase);
            var whereInvocationOnlyWhere = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression(textModified);

            var whereIndex = chain.IndexOf(whereInvocation);
            var orderByIndex = chain.IndexOf(orderByInvocation);

            if (whereIndex > orderByIndex)
            {
                var newChain = new List<InvocationExpressionSyntax>();
                bool orderByAdded = false;

                foreach (var expr in chain)
                {
                    if (expr == orderByInvocation && !orderByAdded)
                    {
                        continue;
                    }
                    else if (expr == whereInvocation && !orderByAdded)
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

                var newInvocationExpr = newChain.First();
                for (int i = 1; i < newChain.Count; i++)
                {
                    var memberAccess = (MemberAccessExpressionSyntax)newChain[i].Expression;
                    newInvocationExpr = newChain[i].WithExpression(memberAccess.WithExpression(newInvocationExpr));
                }

                var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                if (root != null)
                {
                    var newRoot = root.ReplaceNode(invocationExpr, newInvocationExpr)
                        .WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);

                    return document.WithSyntaxRoot(newRoot);
                }
            }
        }

        return document;
    }
}
