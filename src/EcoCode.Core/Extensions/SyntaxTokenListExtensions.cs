namespace EcoCode.Extensions;

/// <summary>Extensions methods for <see cref="SyntaxTokenList"/>.</summary>
public static class SyntaxTokenListExtensions
{
    /// <summary>Returns whether the list contains a given token.</summary>
    /// <param name="tokenList">The token list.</param>
    /// <param name="kind">The token kind.</param>
    /// <returns>True if the list contains the token, false otherwise.</returns>
    public static bool Contains(this SyntaxTokenList tokenList, SyntaxKind kind) => tokenList.IndexOf(kind) != -1;
}
