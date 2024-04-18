using System.Collections.Generic;

namespace EcoCode.Shared;

/// <summary>Extensions methods for <see cref="INamedTypeSymbol"/>.</summary>
public static class NamedTypeSymbolExtensions
{
    /// <summary>Returns whether the symbol is externally public, ie declared public and contained in public types.</summary>
    /// <param name="symbol">The symbol.</param>
    /// <returns>True if the symbol is externally public, false otherwise.</returns>
    public static bool IsExternallyPublic(this INamedTypeSymbol symbol)
    {
        do
        {
            if (symbol.DeclaredAccessibility is not Accessibility.Public) return false;
            symbol = symbol.ContainingType;
        } while (symbol is not null);
        return true;
    }

    /// <summary>Returns whether a symbol has any overridable member.</summary>
    /// <param name="symbol">The symbol.</param>
    /// <param name="recursive">True to include the inherited members, false to restrict the search to the type declared members.</param>
    /// <param name="excludeImplicits">Whether to excluded implicitly declared members, such as record methods like Equals and GetHashCode.</param>
    /// <returns>True if any member is overridable, false otherwise.</returns>
    public static bool HasAnyOverridableMember(this INamedTypeSymbol symbol, bool recursive, bool excludeImplicits = true)
    {
        var type = symbol;
        var sealedMembers = default(HashSet<ISymbol>);
        do
        {
            foreach (var member in type.GetMembers())
            {
                if (member.IsImplicitlyDeclared && excludeImplicits) continue;

                if (member.IsVirtual && sealedMembers?.Contains(member.OverridenSymbol()!) != true)
                    return true;

                if (member.IsOverride)
                {
                    if (!member.IsSealed) return true;
                    _ = (sealedMembers ??= new HashSet<ISymbol>(SymbolEqualityComparer.Default)).Add(member.OverridenSymbol()!);
                }
            }
            type = type.BaseType;
        } while (recursive && type is not null);

        return false;
    }
}
