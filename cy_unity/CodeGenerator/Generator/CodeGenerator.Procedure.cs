using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace UnitySourceGenerator
{
    public partial class CodeGenerator
    {
        private void GenerateProcedureConfigCode(SyntaxReceiver receiver, GeneratorExecutionContext context)
        {
            if (receiver.ProcedureClasses.Count == 0)
            {
                return;
            }
            
            var classSource = ProcessClass(receiver);
            context.AddSource($"ProcedureConfig_Code_g.cs", SourceText.From(classSource, Encoding.UTF8));
        }

        private string ProcessClass(SyntaxReceiver receiver)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"// {receiver.ProcedureClasses.Count}");
            stringBuilder.AppendLine("using System.Collections.Generic;");
            stringBuilder.AppendLine("using System;");
            WriteContent(0, $"public partial class ProcedureConfig", stringBuilder);
            WriteContent(0, "{", stringBuilder);
            GenerateDeepConfigsCode(receiver.ProcedureClasses, 1, stringBuilder);
            WriteContent(1, "public static Dictionary<string, int> DeepConfigs => deepConfigs;", stringBuilder);
            GenerateProcedureCreatorCode(receiver.ProcedureClasses, 1, stringBuilder);
            GenerateConfigGetterCode(receiver.ProcedureClasses, 1, stringBuilder);
            WriteContent(0, "}", stringBuilder);
            return stringBuilder.ToString();
        }

        private void GenerateConfigGetterCode(Dictionary<INamedTypeSymbol, string> procedureClasses, int intent,
            StringBuilder stringBuilder)
        {
            WriteContent(intent, "public static int GetProcedureDeep(string procedureConfigKey)", stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            WriteContent(intent + 1, "return deepConfigs[procedureConfigKey];", stringBuilder);
            WriteContent(intent, "}", stringBuilder);
        }

        private void GenerateDeepConfigsCode(Dictionary<INamedTypeSymbol, string> procedureClasses, int intent,
            StringBuilder stringBuilder)
        {
            WriteContent(intent, "private static Dictionary<string, int> deepConfigs = new Dictionary<string, int>",
                stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            foreach (var item in procedureClasses)
            {
                WriteContent(intent + 1, $"{{nameof({item.Key.ToDisplayString()}), {item.Value}}},", stringBuilder);
            }

            WriteContent(intent, "};", stringBuilder);
        }

        private void GenerateProcedureCreatorCode(Dictionary<INamedTypeSymbol, string> procedureClasses, int intent,
            StringBuilder stringBuilder)
        {
            WriteContent(intent, "public static DH.Game.ProcedureBase GetProcedureClass(string procedureConfigKey)",
                stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            WriteContent(intent + 1, "switch (procedureConfigKey)", stringBuilder);
            WriteContent(intent + 1, "{", stringBuilder);
            foreach (var item in procedureClasses)
            {
                var fullClassName = item.Key.ToDisplayString();
                WriteContent(intent + 2, $"case nameof({fullClassName}):", stringBuilder);
                WriteContent(intent + 3, $" return new {fullClassName}();", stringBuilder);
            }

            WriteContent(intent + 2, $"default:", stringBuilder);
            WriteContent(intent + 3, @"throw new Exception($""Don't find procedure {procedureConfigKey}"");", stringBuilder);
            WriteContent(intent + 1, "}", stringBuilder);
            WriteContent(intent, "}", stringBuilder);
        }
    }


    public partial class SyntaxReceiver
    {
        public Dictionary<INamedTypeSymbol, string> ProcedureClasses = new Dictionary<INamedTypeSymbol, string>(SymbolEqualityComparer.Default);

        public void VisitProcedureClassSyntaxNode(GeneratorSyntaxContext context)
        {
            if (!(context.Node is ClassDeclarationSyntax classSyntax) ||
                classSyntax.AttributeLists.Count == 0)
            {
                return;
            }

            INamedTypeSymbol typeSymbol = context.SemanticModel.GetDeclaredSymbol(classSyntax);
            if (typeSymbol != null && classSyntax.AttributeLists.Count > 0)
            {
                var attribute = typeSymbol.GetAttributes().First();
                if (attribute?.AttributeClass?.Name != "ProcedureDeepAttribute")
                {
                    return;
                }

                if (attribute.ConstructorArguments.Length < 1)
                {
                    return;
                }

                var value = attribute.ConstructorArguments[0].Value;
                if (value != null)
                {
                    var deepValue = value.ToString();
                    ProcedureClasses.Add(typeSymbol, deepValue);
                }
            }
        }
    }
}