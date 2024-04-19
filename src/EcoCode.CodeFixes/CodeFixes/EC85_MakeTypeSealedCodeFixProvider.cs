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

    private static async Task<Document> RefactorAsync(Document document, TypeDeclarationSyntax decl, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null) return document;

        const int virtualKeyword = (int)SyntaxKind.VirtualKeyword;
        const int sealedKeyword = (int)SyntaxKind.SealedKeyword;

        bool updated = false;
        var newModifiers = new List<SyntaxToken>(8); // Pre-allocate to avoid resizing, 8 is more than enough for modifiers
        var newMembers = new List<MemberDeclarationSyntax>(decl.Members.Count);
        foreach (var member in decl.Members)
        {
            newModifiers.Clear();
            foreach (var modifier in member.Modifiers)
            {
                if (modifier.RawKind is not virtualKeyword and not sealedKeyword)
                    newModifiers.Add(modifier);
            }

            if (newModifiers.Count == member.Modifiers.Count)
            {
                newMembers.Add(member); // Don't allocate for nothing
            }
            else
            {
                updated = true;
                newMembers.Add(member.WithModifiers(SyntaxFactory.TokenList(newModifiers)));
            }
        }

        var updatedDecl = decl.WithModifiers(decl.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.SealedKeyword)));
        if (updated) updatedDecl = updatedDecl.WithMembers(SyntaxFactory.List(newMembers));

        return document.WithSyntaxRoot(root.ReplaceNode(decl, updatedDecl));
    }
}
