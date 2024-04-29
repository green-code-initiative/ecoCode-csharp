namespace EcoCode.Analyzers;

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
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        context.RegisterCodeFix(CodeAction.Create(
            title: "Make type sealed",
            createChangedSolution: token => SealDeclaration(context, token),
            equivalenceKey: "Make type sealed"),
            context.Diagnostics);
        return Task.CompletedTask;
    }

    private static async Task<Solution> SealDeclaration(CodeFixContext context, CancellationToken token)
    {
        var solutionEditor = new SolutionEditor(context.Document.Project.Solution);
        var location = context.Diagnostics[0].Location;

        if (solutionEditor.OriginalSolution.GetDocument(location.SourceTree) is not Document document ||
            await document.GetSyntaxRootAsync(token).ConfigureAwait(false) is not SyntaxNode root ||
            root.FindNode(location.SourceSpan) is not TypeDeclarationSyntax declaration)
        {
            return solutionEditor.OriginalSolution;
        }

        var documentEditor = await solutionEditor.GetDocumentEditorAsync(document.Id, token).ConfigureAwait(false);
        var newModifiers = documentEditor.Generator.GetModifiers(declaration).WithIsSealed(true);
        documentEditor.ReplaceNode(declaration, documentEditor.Generator.WithModifiers(declaration, newModifiers));

        return solutionEditor.GetChangedSolution();
    }
}
