using System.Linq;

namespace EcoCode.CodeFixes;

/// <summary>The code fix provider for make type sealed.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeTypeSealedCodeFixProvider)), Shared]
public sealed class MakeTypeSealedCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [MakeTypeSealedAnalyzer.Descriptor.Id];

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
                if (node is TypeDeclarationSyntax decl &&
                    decl is ClassDeclarationSyntax or RecordDeclarationSyntax &&
                    !decl.Modifiers.Any(SyntaxKind.SealedKeyword))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Make type sealed",
                            createChangedDocument: token => RefactorAsync(context.Document, decl, token),
                            equivalenceKey: "Make type sealed"),
                        diagnostic);
                    break;
                }
            }
        }
    }

    private static async Task<Document> RefactorAsync(Document document, TypeDeclarationSyntax decl, CancellationToken token)
    {
        if (await document.GetSyntaxRootAsync(token).ConfigureAwait(false) is not SyntaxNode root ||
            await document.GetSemanticModelAsync(token).ConfigureAwait(false) is not SemanticModel semanticModel)
        {
            return document;
        }

        var newModifiers = new List<SyntaxToken>(8); // Pre-allocate to avoid resizing, 8 is enough for modifiers
        var newMembers = new List<MemberDeclarationSyntax>(decl.Members.Count);
        var privateKeyword = SyntaxFactory.Token(SyntaxKind.PrivateKeyword);
        const int Keep = 0, ReplaceWithPrivate = 1, Remove = 2;

        foreach (var member in decl.Members)
        {
            // Determine what to do with the protected modifier if present
            int handleProtected = semanticModel.GetDeclaredSymbol(member, token)?.DeclaredAccessibility switch
            {
                Accessibility.Protected => ReplaceWithPrivate,
                Accessibility.ProtectedOrInternal or Accessibility.ProtectedAndInternal => Remove,
                _ => Keep,
            };

            // Build the new modifiers for the member
            newModifiers.Clear();
            foreach (var modifier in member.Modifiers)
            {
                if (modifier.IsKind(SyntaxKind.VirtualKeyword) || modifier.IsKind(SyntaxKind.SealedKeyword))
                    continue; // Skip those modifiers

                if (!modifier.IsKind(SyntaxKind.ProtectedKeyword) || handleProtected == Keep)
                    newModifiers.Add(modifier); // Keep the other modifiers, including protected

                if (handleProtected == ReplaceWithPrivate)
                    newModifiers.Add(privateKeyword);
            }

            newMembers.Add(newModifiers.Count == member.Modifiers.Count && handleProtected != ReplaceWithPrivate
                ? member // Don't allocate if the modifiers haven't changed
                : member.WithModifiers(SyntaxFactory.TokenList(newModifiers))
                    .WithLeadingTriviaIfDifferent(member.GetLeadingTrivia())
                    .WithTrailingTriviaIfDifferent(member.GetTrailingTrivia()));
        }

        return document.WithSyntaxRoot(root.ReplaceNode(decl, decl
            .WithModifiers(decl.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.SealedKeyword))) // Add the sealed keyword to the type
            .WithMembers(SyntaxFactory.List(newMembers))));
    }
}
