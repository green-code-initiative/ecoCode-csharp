namespace EcoCode.Core.Analyzers
{
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Provides a code fix for the UseCastInsteadOfSelectToCast analyzer.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCastInsteadOfSelectToCastFixer)), Shared]
    public sealed class UseCastInsteadOfSelectToCastFixer : CodeFixProvider
    {
        /// <summary>
        /// Gets the diagnostic IDs that this provider can fix.
        /// </summary>
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Rule.Ids.EC90_UseCastInsteadOfSelectToCast);

        /// <summary>
        /// Gets the provider that can fix all occurrences of diagnostics.
        /// </summary>
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <summary>
        /// Registers the code fixes provided by this provider.
        /// </summary>

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (context.Diagnostics.Length == 0) return;

            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root is null) return;

            var nodeToFix = root.FindNode(context.Span, getInnermostNodeForTie: true);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Use nameof",
                    createChangedDocument: token => RefactorAsync(document, context.Diagnostics.First(), token),
                    equivalenceKey: "Use nameof"),
                context.Diagnostics);
        }

        private async Task<Document> RefactorAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            if (root == null)
            {
                return document;
            }

            // Find the Select invocation node
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var selectInvocation = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();

            if (selectInvocation is null)
            {
                return document;
            }

            // Get the lambda expression from the Select method call
            var selectArgument = selectInvocation.ArgumentList.Arguments[0];
            var lambdaExpression = selectArgument.Expression as SimpleLambdaExpressionSyntax;

            if (lambdaExpression is null)
            {
                return document;
            }

            // Get the type from the cast expression within the lambda expression
            var castExpression = lambdaExpression.Body as CastExpressionSyntax;

            if (castExpression is null)
            {
                return document;
            }

            var castType = castExpression.Type;

            // Create a new Cast invocation node
            var memberAccess = selectInvocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess?.Expression == null)
            {
                return document;
            }

            var castInvocation = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    memberAccess.Expression,
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier("Cast"),
                        SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(castType))
                    )
                )
            );

            // Replace only the Select invocation with the new Cast invocation
            var newRoot = root.ReplaceNode(selectInvocation, castInvocation);

            // Return the new document
            return document.WithSyntaxRoot(newRoot);
        }

    }
}
