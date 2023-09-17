using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace HorizontalPrivilegeEscalation.CodeAnalyzer.Extensions;

public static class SymbolExtensions
{
    /// <summary>
    /// Checks if a given symbol inherited from another symbol (recursively)
    /// </summary>
    /// <param name="typeInheriting"></param>
    /// <param name="typeInherited"></param>
    /// <returns></returns>
    public static bool IsInheritingRecursivelyFrom(this INamedTypeSymbol? typeInheriting,
        INamedTypeSymbol? typeInherited)
    {
        if (typeInheriting is null || typeInherited is null)
        {
            return false;
        }

        if (SymbolEqualityComparer.Default.Equals(typeInheriting, typeInherited))
        {
            return true;
        }

        var currentType = typeInheriting.BaseType;
        while (currentType != null)
        {
            if (SymbolEqualityComparer.Default.Equals(currentType, typeInherited))
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }

    public static ISet<string> FlattenAllPublicPropertyNamesOfMethod(this IMethodSymbol methodSymbol)
    {
        var propNamesSet = new HashSet<string>(System.StringComparer.Ordinal);
        foreach (var parameterSymbol in methodSymbol.Parameters)
        {
            propNamesSet.UnionWith(parameterSymbol.FlattenAllPublicPropertyNamesOfParameter());
        }
    
        return propNamesSet;
    }

    private static ISet<string> FlattenAllPublicPropertyNamesOfParameter(this IParameterSymbol parameterSymbol)
    {
        var propNamesSet = parameterSymbol.Type.FlattenAllPublicPropertyNames();
        propNamesSet.Add(parameterSymbol.Name);

        return propNamesSet;
    }

    private static ISet<string> FlattenAllPublicPropertyNames(this ITypeSymbol typeSymbol)
    {
        var dictToAdd = new Dictionary<string, List<string>>(System.StringComparer.Ordinal);
        FillDictionaryWithPropertyNamesRecursive(dictToAdd, typeSymbol);

        var propNames = dictToAdd.Values.SelectMany(v => v);
        var propNamesSet = new HashSet<string>(propNames, System.StringComparer.Ordinal);

        return propNamesSet;
    }

    private static void FillDictionaryWithPropertyNamesRecursive(
        IDictionary<string, List<string>> dictToAdd,
        ITypeSymbol typeSymbol)
    {
        var typeSymbolFullName = typeSymbol.ToDisplayString();

        if (dictToAdd.ContainsKey(typeSymbolFullName))
        {
            return;
        }

        var propertySymbols = typeSymbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public)
            .ToList();

        dictToAdd.Add(typeSymbolFullName, propertySymbols.Select(p => p.Name).ToList());

        foreach (var propertySymbol in propertySymbols)
        {
            FillDictionaryWithPropertyNamesRecursive(dictToAdd, propertySymbol.Type);
        }
    }
}