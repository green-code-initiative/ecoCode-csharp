using Microsoft.CodeAnalysis.Formatting;
using System.Linq;
using System.Text.RegularExpressions;

namespace EcoCode.Analyzers;

/// <summary>EC91 fixer: Where before Order By.</summary>
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

                if (node is InvocationExpressionSyntax invocationExpr)
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
        var whereClause = queryExpression.Body.Clauses.OfType<WhereClauseSyntax>().FirstOrDefault();
        var orderByClause = queryExpression.Body.Clauses.OfType<OrderByClauseSyntax>().FirstOrDefault();

        if (whereClause != null && orderByClause != null)
        {
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

        var whereInvocation = chain.FirstOrDefault(inv => ((MemberAccessExpressionSyntax)inv.Expression).Name.Identifier.Text == "Where");

        var orderByInvocation = chain.FirstOrDefault(inv =>
        {
            var methodName = ((MemberAccessExpressionSyntax)inv.Expression).Name.Identifier.Text;
            return methodName == "OrderBy" || methodName == "OrderByDescending";
        });

        var textModified = Regex.Replace(whereInvocation.GetText().ToString(), @".OrderBy\s*\(.*?\)", "", RegexOptions.IgnoreCase);
        textModified = Regex.Replace(textModified, @".OrderByDescending\s*\(.*?\)", "", RegexOptions.IgnoreCase);
        var whereInvocationOnlyWhere = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression(textModified);


        if (whereInvocation != null && orderByInvocation != null)
        {
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
