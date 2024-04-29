namespace EcoCode.Extensions;

internal static class TypeSymbolExtensions
{
    /// <summary>Returns the type indexer, if any.</summary>
    /// <param name="type">The type.</param>
    /// <returns>The type indexer, null if none.</returns>
    public static IPropertySymbol? GetIndexer(this ITypeSymbol type)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is IPropertySymbol propSymbol && propSymbol.IsIndexer)
                return propSymbol;
        }
        return null;
    }

    /// <summary>Returns the type's Count or Length.</summary>
    /// <param name="type">The type on which to look for the Count or Length.</param>
    /// <returns>The type's Count or Length, null if none.</returns>
    public static IPropertySymbol? GetCountOrLength(this ITypeSymbol type)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is IPropertySymbol propSymbol && propSymbol.Name is "Count" or "Length")
                return propSymbol;
        }
        return null;
    }

    /// <summary>Returns the type's Count or Length, that is accessible from another type.</summary>
    /// <param name="type">The type on which to look for the Count or Length.</param>
    /// <param name="accessedFrom">The type that needs to access the Count or Length.</param>
    /// <param name="compilation">The compilation.</param>
    /// <returns>The type's Count or Length, that is accessible from <paramref name="accessedFrom"/>, null if none.</returns>
    public static IPropertySymbol? GetCountOrLength(this ITypeSymbol type, ISymbol accessedFrom, Compilation compilation)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is IPropertySymbol propSymbol &&
                propSymbol.Name is "Count" or "Length" &&
                compilation.IsSymbolAccessibleWithin(propSymbol, accessedFrom))
            {
                return propSymbol;
            }
        }
        return null;
    }

    /// <summary>Returns the type's Count or Length, that is accessible from a given position.</summary>
    /// <param name="type">The type on which to look for the Count or Length.</param>
    /// <param name="position">The position to access the Count or Length from.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <returns>The type's Count or Length, that is accessible from <paramref name="position"/>, null if none.</returns>
    public static IPropertySymbol? GetCountOrLength(this ITypeSymbol type, int position, SemanticModel semanticModel)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is IPropertySymbol propSymbol &&
                propSymbol.Name is "Count" or "Length" &&
                semanticModel.IsAccessible(position, propSymbol))
            {
                return propSymbol;
            }
        }
        return null;
    }
}
