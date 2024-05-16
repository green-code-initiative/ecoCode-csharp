namespace EcoCode.Analyzers;

/// <summary>EC88 fixer: Dispose resource asynchronously.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisposeResourceAsynchronouslyFixer)), Shared]
public sealed class DisposeResourceAsynchronouslyFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [DisposeResourceAsynchronously.Descriptor.Id];

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
                if (node is UsingStatementSyntax usingStatement)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Dispose resource asynchronously",
                            createChangedDocument: async token =>
                                await context.Document.GetSyntaxRootAsync(token) is { } root
                                ? context.Document.WithSyntaxRoot(root.ReplaceNode(usingStatement, usingStatement.WithAwaitKeyword(
                                    SyntaxFactory.Token(SyntaxKind.AwaitKeyword).WithTrailingTrivia(SyntaxFactory.Whitespace(" ")))))
                                : context.Document,
                            equivalenceKey: "Dispose resource asynchronously"),
                        diagnostic);
                    break;
                }
                if (node is LocalDeclarationStatementSyntax usingDeclaration)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Dispose resource asynchronously",
                            createChangedDocument: async token =>
                                await context.Document.GetSyntaxRootAsync(token) is { } root
                                ? context.Document.WithSyntaxRoot(root.ReplaceNode(usingDeclaration, usingDeclaration
                                    .WithAwaitKeyword(SyntaxFactory.Token(SyntaxKind.AwaitKeyword))
                                    .WithLeadingTrivia(usingDeclaration.GetLeadingTrivia())))
                                : context.Document,
                            equivalenceKey: "Dispose resource asynchronously"),
                        diagnostic);
                    break;
                }
            }
        }
    }
}
