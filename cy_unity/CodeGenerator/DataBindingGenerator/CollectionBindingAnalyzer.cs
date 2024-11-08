using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Diagnostics;

namespace DataBindingGenerator
{
    //[DiagnosticAnalyzer(LanguageNames.CSharp)]
    //internal class CollectionBindingAnalyzer : DiagnosticAnalyzer
    //{
    //    private const string DiagnosticId = "CollectionBindingInit";
    //    private const string Category = "InitializationSafety";
    //    private static readonly LocalizableString Title = "{0} method should be called";
    //    private static readonly LocalizableString MessageFormat = "{0} method should be called";
    //    private static readonly LocalizableString Description = "{0} method should be called";
    //    private const string HelpLinkUri = "";

    //    private static readonly DiagnosticDescriptor RuleInit = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
    //        Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description,
    //        helpLinkUri: HelpLinkUri);

    //    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleInit);


    //    public override void Initialize(AnalysisContext context)
    //    {
    //        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    //        context.EnableConcurrentExecution();
    //        context.RegisterSyntaxNodeAction(AnalyzeInvocationDeclaration, SyntaxKind.InvocationExpression);
    //    }

    //    private ClassDeclarationSyntax GetClassSymbol(ExpressionSyntax syntax)
    //    {
    //        var parent = syntax.Parent;
    //        while (parent != null)
    //        {
    //            if (parent is ClassDeclarationSyntax declarationSyntax)
    //            {
    //                return declarationSyntax;
    //            }

    //            parent = parent.Parent;
    //        }

    //        return null;
    //    }

    //    private void AnalyzeInvocationDeclaration(SyntaxNodeAnalysisContext context)
    //    {
    //        var methodInvocationSyntax = (InvocationExpressionSyntax)context.Node;
    //        var bindCollectionSymbol = (IMethodSymbol)context.SemanticModel.GetSymbolInfo(methodInvocationSyntax).Symbol;
    //        if (HasCalllBindCollection(bindCollectionSymbol))
    //        {
    //            var classNode = GetClassSymbol(methodInvocationSyntax);
    //            if (classNode == null)
    //            {
    //                return;
    //            }
    //            bool init = false;
    //            bool dispose = false;

    //            foreach (var expressionSyntax in classNode.DescendantNodes()
    //                         .OfType<InvocationExpressionSyntax>())
    //            {
    //                var methodSymbol = context.SemanticModel.GetSymbolInfo(expressionSyntax).Symbol as IMethodSymbol;
    //                if (methodSymbol == null) continue;
    //                if (methodSymbol.Name == "InitCollection")
    //                {
    //                    init = true;
    //                    if (init && dispose)
    //                    {
    //                        return;
    //                    }
    //                    continue;
    //                }

    //                if (methodSymbol.Name == "DisposeCollection")
    //                {
    //                    dispose = true;
    //                    if (init && dispose)
    //                    {
    //                        return;
    //                    }
    //                    continue;
    //                }
    //            }

    //            var diagnostic = Diagnostic.Create(RuleInit, methodInvocationSyntax.GetLocation(),!init ? "InitCollection" : "DisposeCollection");
    //            context.ReportDiagnostic(diagnostic);
    //        }
    //    }


    //    private static bool HasCalllBindCollection(IMethodSymbol methodSymbol)
    //    {
    //        return methodSymbol != null && methodSymbol.Name == "BindCollection" && methodSymbol.IsStatic;
    //    }
    //}
}
