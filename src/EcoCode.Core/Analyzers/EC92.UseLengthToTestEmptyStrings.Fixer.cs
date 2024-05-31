using Microsoft.CodeAnalysis.Formatting;

namespace EcoCode.Analyzers;

/// <summary>EC92 fixer: Use Length to test empty strings.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseLengthToTestEmptyStringsFixer)), Shared]
public sealed class UseLengthToTestEmptyStringsFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [UseLengthToTestEmptyStrings.Descriptor.Id];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use string.Length instead of comparison with empty string",
                createChangedDocument: token => ReplaceWithLengthCheckAsync(context.Document, root.FindNode(diagnosticSpan), token),
                equivalenceKey: "Use string.Length instead of comparison with empty string"),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithLengthCheckAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
    {
        if (node is BinaryExpressionSyntax binaryExpression)
        {
            var newExpression = CreateLengthCheckExpression(binaryExpression);
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            return root is null ? document : document.WithSyntaxRoot(root.ReplaceNode(binaryExpression, newExpression));
        }
        return document;
    }

    private static BinaryExpressionSyntax CreateLengthCheckExpression(BinaryExpressionSyntax binaryExpression)
    {
        var left = binaryExpression.Left;
        var right = binaryExpression.Right;

        ExpressionSyntax? stringExpression = null;

        // Determine which side is the string literal and which is the string variable.
        if (IsEmptyString(left))
            stringExpression = right;
        else if (IsEmptyString(right))
            stringExpression = left;

        if (stringExpression is null)
            return binaryExpression; // Return the original expression if we can't determine the string expression.

        // Create the new expression: stringExpression.Length == 0 or stringExpression.Length != 0
        var lengthMemberAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            stringExpression,
            SyntaxFactory.IdentifierName("Length"));

        var zeroLiteral = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0));

        var newBinaryExpression = binaryExpression.Kind() == SyntaxKind.EqualsExpression
            ? SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, lengthMemberAccess, zeroLiteral)
            : SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, lengthMemberAccess, zeroLiteral);

        return newBinaryExpression.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
    }

    private static bool IsEmptyString(ExpressionSyntax expression) =>
        expression is LiteralExpressionSyntax { Token.ValueText.Length: 0 };
}
