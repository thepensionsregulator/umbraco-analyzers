using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ThePensionsRegulator.Analyzers.Umbraco;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PublishedValueFallbackAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "TPRUMB0001";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Pass IPublishedValueFallback to Umbraco value access methods",
        messageFormat: "'.{0}()' is called without IPublishedValueFallback. " +
                       "Use the overload that accepts IPublishedValueFallback as the first argument.",
        category: "Umbraco.Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Umbraco's .Value<T>() and .GetCropUrl() methods have overloads that accept " +
                     "IPublishedValueFallback. Always use these overloads to ensure correct " +
                     "fallback behaviour and testability.",
        helpLinkUri: "https://github.com/thepensionsregulator/umbraco-analyzers/blob/develop/docs/TPRUMB0001.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Handle both `x.Method(...)` (MemberAccessExpression)
        // and `x?.Method(...)` (MemberBindingExpression inside a conditional access)
        SimpleNameSyntax? methodName = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
            MemberBindingExpressionSyntax memberBinding => memberBinding.Name,
            _ => null
        };

        if (methodName is null)
            return;

        var methodIdentifier = methodName.Identifier.Text;
        if (methodIdentifier is not ("Value" or "GetCropUrl"))
            return;

        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
            return;

        // Determine the type that .Value<T>() is being called on.
        // For extension methods this is the ReceiverType; for instance methods it is the ContainingType.
        ITypeSymbol? targetType = methodSymbol.IsExtensionMethod
            ? methodSymbol.ReceiverType
            : methodSymbol.ContainingType;

        if (targetType is null || !ImplementsPublishedElementOrContent(targetType))
            return;

        bool hasFallbackParameter = methodSymbol.Parameters.Any(
            p => p.Type.ToDisplayString().Contains("IPublishedValueFallback"));

        if (hasFallbackParameter)
            return;

        var invokedMethod = methodName is GenericNameSyntax genericMethod
            ? $"{methodIdentifier}<{string.Join(", ", genericMethod.TypeArgumentList.Arguments)}>"
            : methodIdentifier;
        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), invokedMethod));
    }

    private static bool ImplementsPublishedElementOrContent(ITypeSymbol type)
    {
        if (IsPublishedElementOrContent(type))
            return true;

        foreach (var iface in type.AllInterfaces)
        {
            if (IsPublishedElementOrContent(iface))
                return true;
        }

        // Walk base types for classes
        var baseType = type.BaseType;
        while (baseType is not null)
        {
            if (IsPublishedElementOrContent(baseType))
                return true;

            foreach (var iface in baseType.AllInterfaces)
            {
                if (IsPublishedElementOrContent(iface))
                    return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    private static bool IsPublishedElementOrContent(ITypeSymbol type)
    {
        var name = type.Name;
        return (name == "IPublishedElement" || name == "IPublishedContent")
               && type.ContainingNamespace?.ToDisplayString() == "Umbraco.Cms.Core.Models.PublishedContent";
    }
}
