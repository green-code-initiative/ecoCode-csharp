using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using System.Threading;

namespace EcoCode.CodeFixes;

/// <summary>The code fix provider for use struct layout.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SpecifyStructLayoutCodeFixProvider)), Shared]
public sealed class SpecifyStructLayoutCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => [SpecifyStructLayoutAnalyzer.Descriptor.Id];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var node = root?.FindNode(context.Span, getInnermostNodeForTie: true);
        if (node is not TypeDeclarationSyntax nodeToFix) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Add Auto StructLayout attribute",
                ct => RefactorAsync(context.Document, nodeToFix, LayoutKind.Auto, ct),
                equivalenceKey: "Add Auto StructLayout attribute"),
            context.Diagnostics);

        context.RegisterCodeFix(
            CodeAction.Create(
                "Add Sequential StructLayout attribute",
                ct => RefactorAsync(context.Document, nodeToFix, LayoutKind.Sequential, ct),
                equivalenceKey: "Add Sequential StructLayout attribute"),
            context.Diagnostics);
    }

    private static async Task<Document> RefactorAsync(Document document, SyntaxNode nodeToFix, LayoutKind layoutKind, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        var structLayoutAttributeType = editor.SemanticModel.Compilation.GetBestTypeByMetadataName("System.Runtime.InteropServices.StructLayoutAttribute");
        if (structLayoutAttributeType is null) return document;

        var layoutKindType = editor.SemanticModel.Compilation.GetBestTypeByMetadataName("System.Runtime.InteropServices.LayoutKind");
        if (layoutKindType is null) return document;

        editor.AddAttribute(nodeToFix, editor.Generator.Attribute(
            editor.Generator.TypeExpression(structLayoutAttributeType).WithAdditionalAnnotations(Simplifier.AddImportsAnnotation),
            [
                editor.Generator.AttributeArgument(editor.Generator.MemberAccessExpression(
                    editor.Generator.TypeExpression(layoutKindType).WithAdditionalAnnotations(Simplifier.AddImportsAnnotation),
                    layoutKind.ToString())),
            ]));
        return editor.GetChangedDocument();
    }
}
