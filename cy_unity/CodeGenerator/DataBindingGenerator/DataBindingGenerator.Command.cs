using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace DataBindingGenerator
{
    public partial class DataBindingCodeGenerator
    {
        private void GenerateCommandMethodCode(DataBindingSyntaxReceiver receiver, GeneratorExecutionContext context)
        {
            foreach (var item in receiver.CommandMethods)
            {
                var classSource = ProcessClass(item.Key, item.Value, receiver);
                context.AddSource($"{item.Key.Name}_CommandCode_g.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }

        private string ProcessClass(INamedTypeSymbol classSymbol, List<MethodPair> methodSymbols,
            DataBindingSyntaxReceiver receiver)
        {
            var stringBuilder = receiver.stringBuilder;
            stringBuilder.Length = 0;
            stringBuilder.AppendLine($"// {receiver.CommandMethods.Count}");
            stringBuilder.AppendLine($"// Update Time:{DateTime.Now}");
            var namespaceName = classSymbol.ToDisplayString().Replace($".{classSymbol.Name}", null);
            if (!string.IsNullOrEmpty(namespaceName))
            {
                stringBuilder.AppendLine($"namespace {namespaceName}");
                stringBuilder.AppendLine("{");
            }
           
            WriteContent(1, $"public partial class {classSymbol.Name}", stringBuilder);
            WriteContent(1, "{", stringBuilder);
            foreach (var methodSymbol in methodSymbols)
            {
                GenerateMethodCommandCode(methodSymbol, 2, stringBuilder);
            }
            WriteContent(1, "}", stringBuilder);
            if (!string.IsNullOrEmpty(namespaceName))
            {
                stringBuilder.AppendLine("}");
            }
            return stringBuilder.ToString();
        }

        private void GenerateMethodCommandCode(MethodPair methodPair, int intent, StringBuilder stringBuilder)
        {
            string commandTypeName = "DH.UIFramework.Commands.SimpleCommand";
            if (methodPair.method.ReturnType.Name == "UniTask")
            {
                if(methodPair.method.Parameters.Length > 0)
                {
                    commandTypeName = $"DH.UIFramework.Commands.AsyncCommand<{methodPair.method.Parameters[0]}>";
                }
                else
                {
                    commandTypeName = "DH.UIFramework.Commands.AsyncCommand";
                }              
            }
            else if(methodPair.canExcuteMethod != null)
            {
                commandTypeName = "DH.UIFramework.Commands.RelayCommand";
            }
            else if (methodPair.method.Parameters.Length > 0)
            {
                commandTypeName = $"DH.UIFramework.Commands.SimpleCommand<{methodPair.method.Parameters[0]}>";
            }

            WriteContent(intent, $"private DH.UIFramework.Commands.ICommand {LowerCaseCamelCase(methodPair.method.Name)}Command;", stringBuilder);
            WriteContent(intent, $"public DH.UIFramework.Commands.ICommand {UpperCaseCamelCase(methodPair.method.Name)}Command", stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            if(methodPair.canExcuteMethod != null)
            {
                WriteContent(intent + 1, $"get => {LowerCaseCamelCase(methodPair.method.Name)}Command ??= new {commandTypeName}({methodPair.method.Name},{methodPair.canExcuteMethod.Name});", stringBuilder);
            }
            else
            {
                WriteContent(intent + 1, $"get => {LowerCaseCamelCase(methodPair.method.Name)}Command ??= new {commandTypeName}({methodPair.method.Name});", stringBuilder);
            }
            WriteContent(intent, "}", stringBuilder);
        }
    }

    public class MethodPair
    {
        public IMethodSymbol method;
        public IMethodSymbol canExcuteMethod;
    }

    public partial class DataBindingSyntaxReceiver
    {
        public Dictionary<INamedTypeSymbol, List<MethodPair>> CommandMethods = new Dictionary<INamedTypeSymbol, List<MethodPair>>(SymbolEqualityComparer.Default);

        public void VisitCommandMethodSyntaxNode(GeneratorSyntaxContext context)
        {
            if (!(context.Node is MethodDeclarationSyntax methodDeclarationSyntax) || methodDeclarationSyntax.AttributeLists.Count == 0)
            {
                return;
            }

            // Get the symbol being declared by the field, and keep it if its annotated
            IMethodSymbol methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax) as IMethodSymbol;
            if (methodSymbol == null || !methodSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == "DH.UIFramework.CommandAttribute"))
            {
                return;
            }

            var parentClass = methodSymbol.ContainingType;
            var canExcuteMethodName = $"Can{methodSymbol.Name}";
            var canExcuteMethod = parentClass.GetMembers().FirstOrDefault(x => x is IMethodSymbol method && method.Name == canExcuteMethodName) as IMethodSymbol;
            if(canExcuteMethod != null && canExcuteMethod.ReturnType.ToDisplayString() != "bool")
            {
                canExcuteMethod = null;
            }

            if (CommandMethods.TryGetValue(parentClass, out var symbols))
            {
                symbols.Add(new MethodPair { method = methodSymbol,canExcuteMethod = canExcuteMethod});
            }
            else
            {
                symbols = new List<MethodPair>() { new MethodPair { method = methodSymbol, canExcuteMethod = canExcuteMethod } };
                CommandMethods.Add(parentClass, symbols);
            }
        }
    }
}
