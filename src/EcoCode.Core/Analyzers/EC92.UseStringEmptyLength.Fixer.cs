using Microsoft.CodeAnalysis.Formatting;

namespace EcoCode.Analyzers;

/// <summary>EC92 fixer: Use string.Length instead of comparison with empty string</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseStringLengthCodeFixProvider)), Shared]
public class UseStringLengthCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [UseStringEmptyLength.Descriptor.Id];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;
        // Find the node at the diagnostic location.
        var node = root.FindNode(diagnosticSpan);

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            Microsoft.CodeAnalysis.CodeActions.CodeAction.Create(
                title: "Use string.Length instead of comparison with empty string",
                createChangedDocument: c => ReplaceWithLengthCheckAsync(context.Document, node, c),
                equivalenceKey: "Use string.Length instead of comparison with empty string"),
            diagnostic);
    }

    private async Task<Document> ReplaceWithLengthCheckAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return document;

        if (node is BinaryExpressionSyntax binaryExpression)
        {
            var newExpression = CreateLengthCheckExpression(binaryExpression, semanticModel);
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            return root is null ? document : document.WithSyntaxRoot(root.ReplaceNode(binaryExpression, newExpression));
        }

        return document;
    }

    private ExpressionSyntax CreateLengthCheckExpression(BinaryExpressionSyntax binaryExpression, SemanticModel semanticModel)
    {
        var left = binaryExpression.Left;
        var right = binaryExpression.Right;

        ExpressionSyntax? stringExpression = null;

        // Determine which side is the string literal and which is the string variable.
        if (IsEmptyString(left))
        {
            stringExpression = right;
        }
        else if (IsEmptyString(right))
        {
            stringExpression = left;
        }

        if (stringExpression is null)
        {
            return binaryExpression; // Return the original expression if we can't determine the string expression.
        }

        // Create the new expression: stringExpression.Length == 0 or stringExpression.Length != 0
        var lengthMemberAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            stringExpression,
            SyntaxFactory.IdentifierName("Length"));

        var zeroLiteral = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0));

        var newBinaryExpression = binaryExpression.Kind() == SyntaxKind.EqualsExpression
            ? SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, lengthMemberAccess, zeroLiteral)
            : SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, lengthMemberAccess, zeroLiteral);

        return newBinaryExpression
            .WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
    }

    private static bool IsEmptyString(ExpressionSyntax expression)
    {
        return expression is LiteralExpressionSyntax literal && literal.Token.ValueText.Length == 0;
    }
}
