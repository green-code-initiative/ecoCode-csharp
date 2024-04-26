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
        if (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) is not SyntaxNode root ||
            await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false) is not SemanticModel semanticModel)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var parent = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;
            if (parent is null) continue;

            foreach (var node in parent.AncestorsAndSelf())
            {
                if (node is not TypeDeclarationSyntax decl ||
                    decl is not ClassDeclarationSyntax and not RecordDeclarationSyntax ||
                    decl.Modifiers.Any(SyntaxKind.SealedKeyword) ||
                    semanticModel.GetDeclaredSymbol(decl, context.CancellationToken) is not INamedTypeSymbol symbol)
                {
                    continue;
                }

                context.RegisterCodeFix(
                    symbol.DeclaringSyntaxReferences.Length == 1
                    ? CodeAction.Create( // Single declaration
                        title: "Make type sealed",
                        createChangedDocument: token => RefactorSingleAsync(context.Document, decl, token),
                        equivalenceKey: "Make type sealed")
                    : CodeAction.Create( // Multiple declarations, happens with partial
                        title: "Make type sealed",
                        createChangedSolution: token => RefactorMultipleAsync(context.Document, decl, token),
                        equivalenceKey: "Make type sealed"),
                    diagnostic);
                break;
            }
        }
    }

    private static async Task<Document> RefactorSingleAsync(Document document, TypeDeclarationSyntax decl, CancellationToken token)
    {
        if (await document.GetSyntaxRootAsync(token).ConfigureAwait(false) is not SyntaxNode root ||
            await document.GetSemanticModelAsync(token).ConfigureAwait(false) is not SemanticModel semanticModel)
        {
            return document;
        }

        var newModifiers = new List<SyntaxToken>(8); // Pre-allocate to avoid resizing, 8 is enough for modifiers
        var newMembers = new List<MemberDeclarationSyntax>(decl.Members.Count);
        var newDecl = RefactorDeclaration(decl, semanticModel, sealDecl: true, newModifiers, newMembers, token);
        return decl == newDecl ? document : document.WithSyntaxRoot(root.ReplaceNode(decl, newDecl));
    }

    private static async Task<Solution> RefactorMultipleAsync(Document document, TypeDeclarationSyntax originalDecl, CancellationToken token)
    {
        var solution = document.Project.Solution;
        if (await document.GetSemanticModelAsync(token).ConfigureAwait(false) is not SemanticModel semanticModel ||
            semanticModel.GetDeclaredSymbol(originalDecl, token) is not INamedTypeSymbol symbol)
        {
            return solution;
        }

        // Documents can be processed in parallel, but the references within a document must be processed sequentially
        var documents = new Dictionary<Document, List<SyntaxReference>>();
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            if (solution.GetDocument(reference.SyntaxTree) is not Document doc) continue;

            if (documents.TryGetValue(doc, out var references))
                references.Add(reference);
            else
                documents.Add(doc, [reference]);
        }

        var updates = await Task.WhenAll(documents
            .Select(async kvp =>
            {
                var (doc, references) = (kvp.Key, kvp.Value);
                if (await doc.GetSyntaxRootAsync(token).ConfigureAwait(false) is not SyntaxNode root ||
                    await doc.GetSemanticModelAsync(token).ConfigureAwait(false) is not SemanticModel semanticModel)
                {
                    return default;
                }

                // Pre-allocated buffers to work with, to avoid allocations in RefactorDeclaration
                var newModifiers = new List<SyntaxToken>(8);
                var newMembers = new List<MemberDeclarationSyntax>(128);

                var updates = new List<(TypeDeclarationSyntax Current, TypeDeclarationSyntax New)>(references.Count);
                foreach (var reference in references) // Despite the await keyword in it's body, this loop is sequential
                {
                    if (await reference.GetSyntaxAsync(token) is not TypeDeclarationSyntax decl) continue;

                    var newDecl = RefactorDeclaration(decl, semanticModel, decl == originalDecl, newModifiers, newMembers, token);
                    if (decl != newDecl) updates.Add((decl, newDecl));
                }
                return updates.Count == 0 ? default : (doc.Id, root, updates);
            }))
            .ConfigureAwait(false);

        foreach (var (docId, docRoot, docUpdates) in updates)
        {
            if (docId is null) continue;

            var newDocRoot = docRoot;
            foreach (var (current, @new) in docUpdates)
                newDocRoot = newDocRoot.ReplaceNode(current, @new);

            if (newDocRoot != docRoot)
                solution = solution.WithDocumentSyntaxRoot(docId, newDocRoot);
        }
        return solution;
    }

    // Because of the provided buffers, this needs to be called sequentially when reusing them
    private static TypeDeclarationSyntax RefactorDeclaration(
        TypeDeclarationSyntax declaration,
        SemanticModel semanticModel,
        bool sealDecl,
        List<SyntaxToken> newModifiers, // Buffer to work on modifiers
        List<MemberDeclarationSyntax> newMembers, // Buffer to work on members
        CancellationToken token)
    {
        const int Keep = 0, ReplaceWithPrivate = 1, Remove = 2;

        newMembers.Clear();
        foreach (var member in declaration.Members)
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
                else if (handleProtected == ReplaceWithPrivate)
                    newModifiers.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
            }

            newMembers.Add(newModifiers.Count == member.Modifiers.Count && handleProtected != ReplaceWithPrivate
                ? member // Don't allocate if the modifiers haven't changed
                : member.WithModifiers(SyntaxFactory.TokenList(newModifiers))
                    .WithLeadingTriviaIfDifferent(member.GetLeadingTrivia())
                    .WithTrailingTriviaIfDifferent(member.GetTrailingTrivia()));
        }

        var newDecl = newMembers.Count == declaration.Members.Count && newMembers.SequenceEqual(declaration.Members)
            ? declaration // Don't allocate if the members haven't changed
            : declaration.WithMembers(SyntaxFactory.List(newMembers));

        if (sealDecl)
        {
            newModifiers.Clear();
            newModifiers.AddRange(newDecl.Modifiers);
            newModifiers.Add(SyntaxFactory.Token(SyntaxKind.SealedKeyword));
            newModifiers.Sort(static (a, b) => a.GetModifierOrder() - b.GetModifierOrder());
            newDecl = newDecl.WithModifiers(SyntaxFactory.TokenList(newModifiers));
        }

        return newDecl;
    }
}
