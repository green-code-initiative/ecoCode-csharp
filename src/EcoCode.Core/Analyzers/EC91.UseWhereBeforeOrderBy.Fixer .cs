using System.Linq;

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
                   // break; // Break out of the loop once the code fix is registered
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
}
