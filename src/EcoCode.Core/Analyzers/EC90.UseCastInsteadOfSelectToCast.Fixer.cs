namespace EcoCode.Core.Analyzers
{
    using System.Linq;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCastInsteadOfSelectToCastFixer)), Shared]
    public sealed class UseCastInsteadOfSelectToCastFixer : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(Rule.Ids.EC90_UseCastInsteadOfSelectToCast); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the Select invocation node identified by the diagnostic.
            var selectInvocation = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Use Cast instead of Select to cast",
                    createChangedDocument: c => RefactorAsync(context.Document, diagnostic, c),
                    equivalenceKey: "UseCastInsteadOfSelectToCast"),
                diagnostic);
        }

        private async Task<Document> RefactorAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            // Find the Select invocation node
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var selectInvocation = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

            // Create a new Cast invocation node
            var memberAccess = selectInvocation.Expression as MemberAccessExpressionSyntax;
            var castInvocation = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    memberAccess.Expression,
                    SyntaxFactory.IdentifierName("Cast")));

            // Replace the old Select invocation with the new Cast invocation
            var newRoot = root.ReplaceNode(selectInvocation, castInvocation);

            // Return the new document
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
