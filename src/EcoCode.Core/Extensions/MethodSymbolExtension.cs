namespace EcoCode.Extensions;

/// <summary>Extensions methods for <see cref="IMethodSymbol"/>.</summary>
public static class MethodSymbolExtensions
{
    /// <summary>Returns whether the method is a LINQ extension method.</summary>
    /// <param name="methodSymbol">The method symbol.</param>
    /// <param name="compilation">The compilation.</param>
    /// <returns>True if the method is a LINQ extension method, false otherwise.</returns>
    public static bool IsLinqMethod(this IMethodSymbol methodSymbol, Compilation compilation) =>
        methodSymbol.IsExtensionMethod &&
        SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, compilation.GetLinqEnumerableSymbol());

    /// <summary>Returns whether the method is a LINQ extension method.</summary>
    /// <param name="methodSymbol">The method symbol.</param>
    /// <param name="linqEnumerableSymbol">The LINQ Enumerable symbol.</param>
    /// <returns>True if the method is a LINQ extension method, false otherwise.</returns>
    public static bool IsLinqMethod(this IMethodSymbol methodSymbol, INamedTypeSymbol? linqEnumerableSymbol) =>
        methodSymbol.IsExtensionMethod &&
        SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, linqEnumerableSymbol);
}
