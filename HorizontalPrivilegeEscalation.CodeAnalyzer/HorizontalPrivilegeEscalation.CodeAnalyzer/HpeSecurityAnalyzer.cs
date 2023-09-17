using System;
using System.Collections.Immutable;
using System.Linq;
using HorizontalPrivilegeEscalation.CodeAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace HorizontalPrivilegeEscalation.CodeAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HpeSecurityAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "HPE001";

    private const string Category = "Security";

    private static readonly LocalizableString Title =
        "The controller method is not calling GetLoggedInUserId. " +
        "It could be dangerous. " +
        "Please suppress this message with a meaningful message if you know this is a false positive.";

    private static readonly LocalizableString MessageFormat =
        "'{0}' should call GetLoggedInUserId method";

    private static readonly LocalizableString Description =
        "Horizontal Privilege Escalation Security Analyzer.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
    }

    private void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
        if (ShouldIgnoreSymbol(context))
        {
            return;
        }

        var methodSymbol = (IMethodSymbol)context.Symbol;
        var syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();

        if (syntaxReference is null)
        {
            return;
        }

        var syntax = syntaxReference.GetSyntax();
        var memberAccesses = new ExpressionCallingCollector();
        memberAccesses.Visit(syntax);

        if (memberAccesses.CallingAnyOfTheseMembers("GetLoggedInUserId"))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(
            descriptor: Rule,
            methodSymbol.Locations[0],
            methodSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }

    /// <summary>
    /// Do the generic check for a method to ignore like is it a Controller method, is it public, etc.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static bool ShouldIgnoreSymbol(SymbolAnalysisContext context)
    {
        if (context.Symbol is not IMethodSymbol methodSymbol)
        {
            return true;
        }

        if (!methodSymbol.DeclaringSyntaxReferences.Any())
        {
            return true;
        }

        var typeSymbol = methodSymbol.ContainingType;
        if (typeSymbol is null)
        {
            return true;
        }

        if (string.Equals(methodSymbol.Name, ".ctor", StringComparison.Ordinal))
        {
            return true;
        }

        // The method or the containing class should be public
        if (typeSymbol.IsAbstract
            || typeSymbol.DeclaredAccessibility != Accessibility.Public
            || methodSymbol.DeclaredAccessibility != Accessibility.Public)
        {
            return true;
        }

        // The containing class should be a controller
        var controllerBaseTypeSymbol =
            context.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.ControllerBase");

        var isInheritingControllerBase = typeSymbol.IsInheritingRecursivelyFrom(controllerBaseTypeSymbol);

        if (!isInheritingControllerBase)
        {
            return true;
        }

        // We should NOT ignore this symbol
        return false;
    }
}