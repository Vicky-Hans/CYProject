using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace DataBindingGenerator
{
    //[DiagnosticAnalyzer(LanguageNames.CSharp)]
    //internal class ProcedureAnalyzer : DiagnosticAnalyzer
    //{
    //    private const string DiagnosticId = "ProcedureAnalyzer";
    //    private const string Category = "ProcedureSafety";
    //    private static readonly LocalizableString Title = "ProcedureDeep attribute should be defined";
    //    private static readonly LocalizableString MessageFormat = "ProcedureDeep attribute should be defined";
    //    private static readonly LocalizableString Description = "ProcedureDeep attribute should be defined";
    //    private const string HelpLinkUri = "";

    //    private static readonly DiagnosticDescriptor RuleInit = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
    //        Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description,
    //        helpLinkUri: HelpLinkUri);

    //    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleInit);


    //    public override void Initialize(AnalysisContext context)
    //    {
    //        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    //        context.EnableConcurrentExecution();
    //        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
    //    }

    //    private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    //    {
    //        var classDeclaration = (ClassDeclarationSyntax)context.Node;
    //        INamedTypeSymbol typeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
    //        if (typeSymbol.BaseType?.ToDisplayString() == "DH.Game.ProcedureBase")
    //        {
    //            if (typeSymbol.GetAttributes().Any(x => x?.AttributeClass?.Name == "ProcedureDeepAttribute"))
    //            {
    //                return;
    //            }

    //            var diagnostic = Diagnostic.Create(RuleInit, classDeclaration.GetLocation());
    //            context.ReportDiagnostic(diagnostic);
    //        }
    //    }


    //    private static bool HasCalllBindCollection(IMethodSymbol methodSymbol)
    //    {
    //        return methodSymbol != null && methodSymbol.Name == "BindCollection" && methodSymbol.IsStatic;
    //    }
    //}
}
