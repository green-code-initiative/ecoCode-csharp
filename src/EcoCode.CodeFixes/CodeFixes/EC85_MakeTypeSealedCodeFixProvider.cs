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
                    ? CodeAction.Create( // Non partial declaration
                        title: "Make type sealed",
                        createChangedDocument: token => RefactorNoPartialAsync(context.Document, decl, token),
                        equivalenceKey: "Make type sealed")
                    : CodeAction.Create( // Partial declaration
                        title: "Make type sealed",
                        createChangedSolution: token => RefactorWithPartialAsync(context.Document, decl, token),
                        equivalenceKey: "Make type sealed"),
                    diagnostic);
                break;
            }
        }
    }

    private static async Task<Document> RefactorNoPartialAsync(Document document, TypeDeclarationSyntax decl, CancellationToken token) =>
        await RefactorDocumentRootAsync(document, decl, sealDecl: true, token) is SyntaxNode newRoot
        ? document.WithSyntaxRoot(newRoot)
        : document;

    private static async Task<Solution> RefactorWithPartialAsync(Document document, TypeDeclarationSyntax originalDecl, CancellationToken token)
    {
        var solution = document.Project.Solution;
        if (await document.GetSemanticModelAsync(token).ConfigureAwait(false) is not SemanticModel semanticModel ||
            semanticModel.GetDeclaredSymbol(originalDecl, token) is not INamedTypeSymbol symbol)
        {
            return solution;
        }

        var documents = new Dictionary<DocumentId, List<SyntaxReference>>();
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            if (solution.GetDocument(reference.SyntaxTree) is not Document doc) continue;

            if (documents.TryGetValue(doc.Id, out var references))
                references.Add(reference);
            else
                documents.Add(doc.Id, [reference]);
        }

        var updates = await Task.WhenAll(symbol.DeclaringSyntaxReferences
            .Select(reference => RefactorReference(reference, solution, originalDecl, token)))
            .ConfigureAwait(false);

        foreach (var (documentId, newRoot) in updates)
        {
            if (documentId is not null && newRoot is not null)
                solution = solution.WithDocumentSyntaxRoot(documentId, newRoot);
        }
        return solution;

        static async Task<(DocumentId? DocumentId, SyntaxNode? NewRoot)> RefactorReference(
            SyntaxReference reference, Solution solution, TypeDeclarationSyntax originalDecl, CancellationToken token) =>
            await reference.GetSyntaxAsync(token) is TypeDeclarationSyntax decl &&
            solution.GetDocument(decl.SyntaxTree) is Document declDocument &&
            await declDocument.GetSyntaxRootAsync(token).ConfigureAwait(false) is SyntaxNode currentRoot &&
            await RefactorDocumentRootAsync(declDocument, decl, decl == originalDecl, token) is SyntaxNode newRoot
            ? (declDocument.Id, newRoot)
            : default;
    }

    private static async Task<SyntaxNode?> RefactorDocumentRootAsync(Document document, TypeDeclarationSyntax declaration, bool sealDecl, CancellationToken token)
    {
        if (await document.GetSyntaxRootAsync(token).ConfigureAwait(false) is not SyntaxNode root ||
            await document.GetSemanticModelAsync(token).ConfigureAwait(false) is not SemanticModel semanticModel)
        {
            return null;
        }

        var newModifiers = new List<SyntaxToken>(8); // Pre-allocate to avoid resizing, 8 is enough for modifiers
        var newMembers = new List<MemberDeclarationSyntax>(declaration.Members.Count);
        var privateKeyword = SyntaxFactory.Token(SyntaxKind.PrivateKeyword);
        const int Keep = 0, ReplaceWithPrivate = 1, Remove = 2;

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

                if (handleProtected == ReplaceWithPrivate)
                    newModifiers.Add(privateKeyword);
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

        return root.ReplaceNode(declaration, newDecl);
    }

    private static async Task<Solution> RefactorAsync(Document document, TypeDeclarationSyntax originalDecl, CancellationToken token)
    {
        var semanticModel = await document.GetSemanticModelAsync(token).ConfigureAwait(false);
        var symbol = semanticModel.GetDeclaredSymbol(originalDecl, token);
        if (symbol == null) return document.Project.Solution;

        var solution = document.Project.Solution;
        var documentsToProcess = new Dictionary<DocumentId, List<SyntaxReference>>();

        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            var syntax = await reference.GetSyntaxAsync(token) as TypeDeclarationSyntax;
            if (syntax != null)
            {
                var declDocument = solution.GetDocument(syntax.SyntaxTree);
                if (declDocument != null)
                {
                    if (!documentsToProcess.ContainsKey(declDocument.Id))
                    {
                        documentsToProcess[declDocument.Id] = new List<SyntaxReference>();
                    }
                    documentsToProcess[declDocument.Id].Add(reference);
                }
            }
        }

        var updateTasks = documentsToProcess.Select(async kvp =>
        {
            var doc = solution.GetDocument(kvp.Key);
            if (doc == null) return null;

            var root = await doc.GetSyntaxRootAsync(token).ConfigureAwait(false);
            if (root == null) return null;

            foreach (var reff in kvp.Value)
            {
                var decl = await reff.GetSyntaxAsync(token) as TypeDeclarationSyntax;
                if (decl != null)
                {
                    root = await RefactorDocumentRootAsync(doc, decl, decl == originalDecl, token);
                }
            }

            return root != null ? (kvp.Key, root) : default;
        });

        var updates = await Task.WhenAll(updateTasks);

        foreach (var update in updates.Where(u => u != default))
        {
            solution = solution.WithDocumentSyntaxRoot(update.Key, update.root);
        }

        return solution;
    }
}
