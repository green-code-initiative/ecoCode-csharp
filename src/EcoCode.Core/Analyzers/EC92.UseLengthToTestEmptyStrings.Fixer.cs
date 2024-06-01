﻿using Microsoft.CodeAnalysis.Formatting;

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
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics[0];

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use string.Length instead of comparison with empty string",
                createChangedDocument: token => ReplaceWithLengthCheckAsync(context.Document, root.FindNode(diagnostic.Location.SourceSpan), token),
                equivalenceKey: "Use string.Length instead of comparison with empty string"),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithLengthCheckAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
    {
        if (node is BinaryExpressionSyntax binaryExpression)
        {
            var (left, right) = (binaryExpression.Left, binaryExpression.Right);
            var stringExpr = left.IsEmptyStringLiteral() ? right : right.IsEmptyStringLiteral() ? left : null;
            if (stringExpr is not null && await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false) is { } root)
            {
                return document.WithSyntaxRoot(root.ReplaceNode(binaryExpression, SyntaxFactory.BinaryExpression(
                    binaryExpression.Kind(),
                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, stringExpr, SyntaxFactory.IdentifierName("Length")),
                    SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0)))
                    .WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation)));
            }
        }
        return document;
    }
}