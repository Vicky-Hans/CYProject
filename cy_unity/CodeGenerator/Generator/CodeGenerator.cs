using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace UnitySourceGenerator
{
    [Generator]
    public partial class CodeGenerator : ISourceGenerator
    {
        private GeneratorExecutionContext Context;

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
            {
                return;
            }
            Context = context;

            try
            {
                if (!string.IsNullOrEmpty(receiver.ErrorMsg))
                {
                    var source = OutputExceptionMessage(receiver, receiver.ErrorMsg);
                    context.AddSource($"CodeGeneratorError_Code_g.cs", SourceText.From(source, Encoding.UTF8));
                    return;
                }
                GenerateProcedureConfigCode(receiver, context);
                GenerateAssetPathCode(receiver, context);
                {
                    var source = OutputExceptionMessage(receiver, $"{receiver.logMsg}");
                    context.AddSource($"CodeGeneratorError_Code_g.cs", SourceText.From(source, Encoding.UTF8));
                }
            }
            catch (Exception ex)
            {
                var source = OutputExceptionMessage(receiver, $"{receiver.ErrorMsg}\r\n{receiver.logMsg.ToString()}\r\n{ex}");
                context.AddSource($"CodeGeneratorError_Code_g.cs", SourceText.From(source, Encoding.UTF8));
            }        
        }

        public static string OutputExceptionMessage(ISyntaxContextReceiver receiver, string ex,string className = "CodeGeneratorError")
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"/* {ex} */");
            stringBuilder.AppendLine("using System.Collections.Specialized;");
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine($"namespace DH.Game");
            stringBuilder.AppendLine("{");
            WriteContent(1, $"public partial class {className}", stringBuilder);
            WriteContent(1, "{", stringBuilder);
            WriteContent(2, "public int testCode;", stringBuilder);
            WriteContent(1, "}", stringBuilder);
            stringBuilder.AppendLine("}");
            return stringBuilder.ToString();
        }

        public static void WriteContent(int intent, string content, StringBuilder stringBuilder,bool newLine = true)
        {
            for (int i = 0; i < intent; i++)
            {
                stringBuilder.Append("\t");
            }

            stringBuilder.Append(content);
            if(newLine)
            {
                stringBuilder.Append(Environment.NewLine);
            }
        }

        internal static string LowerCaseCamelCase(string name)
        {
            return Char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
    }

    public partial class SyntaxReceiver : ISyntaxContextReceiver
    {
        public string ErrorMsg;
        public StringBuilder logMsg = new StringBuilder();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            try
            {
                VisitProcedureClassSyntaxNode(context);
                VisitAssetPathSyntaxNode(context);
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
            }
        }

        private bool IsDerivedFrom(INamedTypeSymbol baseType, string targetType)
        {
            if (GetFullTypeName(baseType) == targetType)
                return true;

            var interfaces = baseType.AllInterfaces;
            foreach (var iface in interfaces)
            {
                if (GetFullTypeName(iface) == targetType) return true;
            }

            return false;
        }

        public static string GetFullNamespace(INamedTypeSymbol symbol)
        {
            return string.Join(".", GetNamespaces(symbol).Reverse());
        }

        public static string GetFullTypeName(INamedTypeSymbol symbol)
        {
            return string.Join(".", GetNamespaces(symbol).Reverse().Concat(new[] { symbol.Name }));
        }

        public static IEnumerable<string> GetNamespaces(INamedTypeSymbol symbol)
        {
            var current = symbol.ContainingNamespace;
            while (current != null)
            {
                if (current.IsGlobalNamespace)
                    break;
                yield return current.Name;
                current = current.ContainingNamespace;
            }
        }
    }
}
