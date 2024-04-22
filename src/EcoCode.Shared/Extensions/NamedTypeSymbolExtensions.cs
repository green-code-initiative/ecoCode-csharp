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
        var analysis = new List<SyntaxReferenceAnalysis>(symbol.DeclaringSyntaxReferences.Length);
        foreach (var syntaxRef in symbol.DeclaringSyntaxReferences)
            analysis.Add(SyntaxReferenceAnalysis.AnalyzeSyntaxReference(syntaxRef, context));
        analysis.Sort();
        return analysis[0].DeclarationSyntax;
    }

    private readonly struct SyntaxReferenceAnalysis : IEquatable<SyntaxReferenceAnalysis>, IComparable<SyntaxReferenceAnalysis>
    {
        public ClassDeclarationSyntax DeclarationSyntax { get; }
        public bool HasVisibility { get; }
        public int Modifiers { get; }
        public int Constructors { get; }
        public int MemberCount { get; }
        public SyntaxReferenceAnalysis(ClassDeclarationSyntax declarationSyntax, bool hasVisibility, int modifiers, int constructors, int memberCount)
        {
            DeclarationSyntax = declarationSyntax;
            HasVisibility = hasVisibility;
            Modifiers = modifiers;
            Constructors = constructors;
            MemberCount = memberCount;
        }

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

            int modifierCount = classDecl.Modifiers.Count;

            int constructorCount = 0, memberCount = 0;
            foreach (var member in classDecl.Members)
            {
                if (member is ConstructorDeclarationSyntax)
                    constructorCount++;
                else if (member is MethodDeclarationSyntax or PropertyDeclarationSyntax)
                    memberCount++;
            }
            return new SyntaxReferenceAnalysis(classDecl, hasVisibility, modifierCount, constructorCount, memberCount);
        }

        public override int GetHashCode() => DeclarationSyntax.GetHashCode();

        public bool Equals(SyntaxReferenceAnalysis other) =>
            DeclarationSyntax == other.DeclarationSyntax &&
            HasVisibility == other.HasVisibility &&
            Modifiers == other.Modifiers &&
            Constructors == other.Constructors &&
            MemberCount == other.MemberCount;

        public override bool Equals(object obj) => obj is SyntaxReferenceAnalysis other && Equals(other);

        public int CompareTo(SyntaxReferenceAnalysis other)
        {
            int comp = Comparer<bool>.Default.Compare(other.HasVisibility, HasVisibility); // True first
            if (comp != 0) return comp;

            comp = Comparer<int>.Default.Compare(other.Modifiers, Modifiers); // Highest first
            if (comp != 0) return comp;

            comp = Comparer<int>.Default.Compare(other.Constructors, Constructors); // Highest first
            return comp == 0 ? MemberCount.CompareTo(other.MemberCount) : comp;
        }
    }
}
