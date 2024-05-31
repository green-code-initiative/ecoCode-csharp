using System.Diagnostics.CodeAnalysis;

namespace EcoCode.Analyzers;

/// <summary>EC88 fixer: Dispose resource asynchronously.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisposeResourceAsynchronouslyFixer)), Shared]
public sealed class DisposeResourceAsynchronouslyFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [DisposeResourceAsynchronously.Descriptor.Id];

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
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
                                await context.Document.GetSyntaxRootAsync(token).ConfigureAwait(false) is { } root
                                ? context.Document.WithSyntaxRoot(root.ReplaceNode(usingStatement, usingStatement
                                    .WithoutLeadingTrivia() // Needs to be removed then re-added to keep everything ordered
                                    .WithoutTrailingTrivia() // Needs to be removed then re-added to keep everything ordered
                                    .WithAwaitKeyword(SyntaxFactory.Token(SyntaxKind.AwaitKeyword))
                                    .WithLeadingTrivia(usingStatement.GetLeadingTrivia())
                                    .WithTrailingTrivia(usingStatement.GetTrailingTrivia())))
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
                                await context.Document.GetSyntaxRootAsync(token).ConfigureAwait(false) is { } root
                                ? context.Document.WithSyntaxRoot(root.ReplaceNode(usingDeclaration, usingDeclaration
                                    .WithoutLeadingTrivia() // Needs to be removed then re-added to keep everything ordered
                                    .WithoutTrailingTrivia() // Needs to be removed then re-added to keep everything ordered
                                    .WithAwaitKeyword(SyntaxFactory.Token(SyntaxKind.AwaitKeyword))
                                    .WithLeadingTrivia(usingDeclaration.GetLeadingTrivia())
                                    .WithTrailingTrivia(usingDeclaration.GetTrailingTrivia())))
                                : context.Document,
                            equivalenceKey: "Dispose resource asynchronously"),
                        diagnostic);
                    break;
                }
            }
        }
    }
}
