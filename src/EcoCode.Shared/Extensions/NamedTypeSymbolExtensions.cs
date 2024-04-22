using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
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

    /// <summary>Returns whether a symbol has any overridable member, excluding the default ones (Equals, GetHashCode, etc).</summary>
    /// <param name="symbol">The symbol.</param>
    /// <returns>True if any member is overridable, false otherwise.</returns>
    public static bool HasAnyExternallyOverridableMember(this INamedTypeSymbol symbol)
    {
        if (symbol.TypeKind is not TypeKind.Class || symbol.IsSealed)
            return false;

        var (current, sealedMembers) = (symbol, default(HashSet<ISymbol>));
        do
        {
            if (current.SpecialType is SpecialType.System_Object) break;

            foreach (var member in current.GetMembers())
            {
                if (member.IsImplicitlyDeclared || member.DeclaredAccessibility is not Accessibility.Public and not Accessibility.Protected)
                    continue; // IsImplicitlyDeclared is for record methods, ie. Equals, GetHashCode, etc

                if (member.IsVirtual && sealedMembers?.Contains(member) != true) return true;

                // If overridden, skip base object methods, ie. Equals, GetHashCode, etc
                if (member.OverriddenSymbol() is { } overridden && overridden.ContainingType.SpecialType is not SpecialType.System_Object)
                {
                    if (member.IsSealed) // Cache the overriden member to prevent returning true when checking it in the parent
                        _ = (sealedMembers ??= new(SymbolEqualityComparer.Default)).Add(overridden);
                    else if (sealedMembers?.Contains(overridden) != true)
                        return true; // Overriden but not sealed is still overridable
                }
            }
            current = current.BaseType;
        } while (current is not null);

        return false;
    }

    /// <summary>Finds the main declaration of a partial class, using a heuristic.</summary>
    /// <param name="symbol">The class symbol.</param>
    /// <param name="context">The compilation context.</param>
    /// <returns>The main class decalration syntax.</returns>
    public static ClassDeclarationSyntax GetPartialClassMainDeclaration(this INamedTypeSymbol symbol, CompilationAnalysisContext context)
    {
        var bestAnalysis = default(SyntaxReferenceAnalysis);
        foreach (var syntaxRef in symbol.DeclaringSyntaxReferences)
        {
            var curAnalysis = SyntaxReferenceAnalysis.AnalyzeSyntaxReference(syntaxRef, context);
            if (curAnalysis.BetterThan(bestAnalysis)) bestAnalysis = curAnalysis;
        }
        return bestAnalysis.ClassDecl;
    }

    private readonly struct SyntaxReferenceAnalysis(
        ClassDeclarationSyntax classDecl, bool hasVisibility, int baseTypes, int modifiers, int constructors, int memberCount)
    {
        public ClassDeclarationSyntax ClassDecl { get; } = classDecl;
        public bool HasVisibility { get; } = hasVisibility;
        public int BaseTypes { get; } = baseTypes;
        public int Modifiers { get; } = modifiers;
        public int Constructors { get; } = constructors;
        public int MemberCount { get; } = memberCount;

        public static SyntaxReferenceAnalysis AnalyzeSyntaxReference(SyntaxReference syntaxRef, CompilationAnalysisContext context)
        {
            if (syntaxRef.GetSyntax(context.CancellationToken) is not ClassDeclarationSyntax classDecl)
                return default;

            bool hasVisibility = false;
            foreach (var modifier in classDecl.Modifiers)
            {
                if (modifier.IsKind(SyntaxKind.PublicKeyword) ||
                    modifier.IsKind(SyntaxKind.PrivateKeyword) ||
                    modifier.IsKind(SyntaxKind.InternalKeyword) ||
                    modifier.IsKind(SyntaxKind.ProtectedKeyword))
                {
                    hasVisibility = true;
                    break;
                }
            }

            int baseTypes = classDecl.BaseList?.Types.Count ?? 0,
                modifierCount = classDecl.Modifiers.Count,
                constructorCount = 0, memberCount = 0;
            foreach (var member in classDecl.Members)
            {
                if (member is ConstructorDeclarationSyntax)
                    constructorCount++;
                else if (member is MethodDeclarationSyntax or PropertyDeclarationSyntax)
                    memberCount++;
            }
            return new SyntaxReferenceAnalysis(classDecl, hasVisibility, baseTypes, modifierCount, constructorCount, memberCount);
        }

        public bool BetterThan(SyntaxReferenceAnalysis other) =>
            HasVisibility != other.HasVisibility ? HasVisibility
            : BaseTypes != other.BaseTypes ? BaseTypes > other.BaseTypes
            : Modifiers != other.Modifiers ? Modifiers > other.Modifiers
            : Constructors != other.Constructors ? Constructors > other.Constructors
            : MemberCount > other.MemberCount;
    }
}
