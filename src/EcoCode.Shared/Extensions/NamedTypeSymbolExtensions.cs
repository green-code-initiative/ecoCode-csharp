namespace EcoCode.Shared;

/// <summary>Extensions methods for <see cref="INamedTypeSymbol"/>.</summary>
public static class NamedTypeSymbolExtensions
{
    /// <summary>Returns whether the symbol has inheritance members: either virtual, overriden but not sealed, protected or private protected.</summary>
    /// <param name="symbol">The symbol.</param>
    /// <returns>True if the symbol has inheritance members, false otherwise.</returns>
    public static bool HasInheritanceMembers(this INamedTypeSymbol symbol)
    {
        foreach (var member in symbol.GetMembers())
        {
            if (member.IsImplicitlyDeclared) continue; // Skip record implicit members like Equals, GetHashCode, etc.
            if (member.IsVirtual || member.IsOverride && !member.IsSealed || member.DeclaredAccessibility is Accessibility.Protected or Accessibility.ProtectedAndInternal)
                return true;
        }
        return false;
    }
}
