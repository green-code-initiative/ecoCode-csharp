namespace EcoCode.Shared;

/// <summary>Extensions methods for <see cref="SyntaxNode"/>.</summary>
public static class SyntaxNodeExtensions
{
    /// <summary>Returns whether the node is inside a loop.</summary>
    /// <param name="node">The node.</param>
    /// <returns>True if the node is inside a loop, false otherwise.</returns>
    public static bool IsInsideLoop(this SyntaxNode node)
    {
        for (var parent = node.Parent; parent is not null; parent = parent.Parent)
        {
            if (parent is ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax)
                return true;
        }
        return false;
    }

    /// <summary>Returns the loop statements of a given node, if applicable.</summary>
    /// <param name="node">The node.</param>
    /// <returns>The loop statements, empty if not applicable.</returns>
    public static SyntaxList<StatementSyntax> GetLoopStatements(this SyntaxNode node)
    {
        var contentSyntax = node switch
        {
            ForStatementSyntax syntax => syntax.Statement,
            ForEachStatementSyntax syntax => syntax.Statement,
            WhileStatementSyntax syntax => syntax.Statement,
            DoStatementSyntax syntax => syntax.Statement,
            _ => null,
        };

        return contentSyntax switch
        {
            BlockSyntax blockSyntax => blockSyntax.Statements,
            ExpressionStatementSyntax expressionSyntax => new(expressionSyntax), // Happens when the loop has only one statement
            _ => default,
        };
    }

    /// <summary>Returns whether a node has a given using directive, use with a document root.</summary>
    /// <param name="node">The node.</param>
    /// <param name="namespace">The namespace of the using directive.</param>
    /// <returns>True if the node has the given using, false otherwise.</returns>
    public static bool HasUsingDirective(this SyntaxNode node, string @namespace)
    {
        foreach (var descendant in node.DescendantNodes())
        {
            if ((descendant as UsingDirectiveSyntax)?.Name?.ToString() == @namespace)
                return true;
        }
        return false;
    }

    /// <summary>Adds a using directive to the given node, use with a document root.</summary>
    /// <param name="node">The node.</param>
    /// <param name="namespace">The namespace of the using directive.</param>
    /// <returns>The updated node with the using directive.</returns>
    public static SyntaxNode AddUsingDirective(this SyntaxNode node, string @namespace)
    {
        var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(@namespace)).NormalizeWhitespace();

        foreach (var descendant in node.DescendantNodes())
        {
            if (descendant is NamespaceDeclarationSyntax namespaceDeclaration)
                return node.ReplaceNode(namespaceDeclaration, namespaceDeclaration.AddUsings(usingDirective));
        }
        return ((CompilationUnitSyntax)node).AddUsings(usingDirective); // Add the using directive at the top of the file
    }

    /// <summary>Returns the node with a given leading trivia, or the node directly if the trivia is the same.</summary>
    /// <param name="node">The node.</param>
    /// <param name="trivia">The trivia.</param>
    /// <returns>The node with the leading trivia.</returns>
    public static TSyntaxNode WithLeadingTriviaIfDifferent<TSyntaxNode>(this TSyntaxNode node, SyntaxTriviaList trivia)
        where TSyntaxNode : SyntaxNode =>
        node.GetLeadingTrivia() == trivia ? node : node.WithLeadingTrivia(trivia);

    /// <summary>Returns the node with a given trailing trivia, or the node directly if the trivia is the same.</summary>
    /// <param name="node">The node.</param>
    /// <param name="trivia">The trivia.</param>
    /// <returns>The node with the trailing trivia.</returns>
    public static TSyntaxNode WithTrailingTriviaIfDifferent<TSyntaxNode>(this TSyntaxNode node, SyntaxTriviaList trivia)
        where TSyntaxNode : SyntaxNode =>
        node.GetTrailingTrivia() == trivia ? node : node.WithTrailingTrivia(trivia);
}
