using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace UnitySourceGenerator
{
    public partial class CodeGenerator
    {
        private void GenerateAssetPathCode(SyntaxReceiver receiver, GeneratorExecutionContext context)
        {
            foreach (var item in receiver.SyncAssetPathConfig)
            {
                var classSource = ProcessSyncAssetPathClass(item.Key, item.Value);
                context.AddSource($"{item.Key.Name}_SyncAssetPathCode_g.cs", SourceText.From(classSource, Encoding.UTF8));
            }

            foreach (var item in receiver.AsyncAssetPathConfig)
            {
                var classSource = ProcessAsyncAssetPathClass(item.Key, item.Value);
                context.AddSource($"{item.Key.Name}_AsyncAssetPathCode_g.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }

        private string ProcessSyncAssetPathClass(INamedTypeSymbol classSymbol,List<IFieldSymbol> fieldSymbols)
        {
            var stringBuilder = new StringBuilder();
            var namespaceName = classSymbol.ToDisplayString().Replace($".{classSymbol.Name}", null);
            if (!string.IsNullOrEmpty(namespaceName))
            {
                stringBuilder.AppendLine($"namespace {namespaceName}");
                stringBuilder.AppendLine("{");
            }

            WriteContent(1, $"public partial class {classSymbol.Name}", stringBuilder);
            WriteContent(1, "{", stringBuilder);
            WriteContent(2, "protected override void ProduceSyncPath(System.Collections.Generic.HashSet<string> syncPath)", stringBuilder);
            WriteContent(2, "{", stringBuilder);
            WriteContent(3, "base.ProduceSyncPath(syncPath);", stringBuilder);
            foreach (var filedSymbol in fieldSymbols)
            {
                WriteContent(3, $"syncPath.Add({filedSymbol.Name});", stringBuilder);
            }
            WriteContent(2, "}", stringBuilder);
            WriteContent(1, "}", stringBuilder);
            if (!string.IsNullOrEmpty(namespaceName))
            {
                stringBuilder.AppendLine("}");
            }
            return stringBuilder.ToString();
        }

        private string ProcessAsyncAssetPathClass(INamedTypeSymbol classSymbol, List<IFieldSymbol> fieldSymbols)
        {
            var stringBuilder = new StringBuilder();
            var namespaceName = classSymbol.ToDisplayString().Replace($".{classSymbol.Name}", null);
            if (!string.IsNullOrEmpty(namespaceName))
            {
                stringBuilder.AppendLine($"namespace {namespaceName}");
                stringBuilder.AppendLine("{");
            }

            WriteContent(1, $"public partial class {classSymbol.Name}", stringBuilder);
            WriteContent(1, "{", stringBuilder);
            WriteContent(2, "protected override void ProduceAsyncPath(System.Collections.Generic.HashSet<string> asyncPath)", stringBuilder);
            WriteContent(2, "{", stringBuilder);
            WriteContent(3, "base.ProduceAsyncPath(asyncPath);", stringBuilder);
            foreach (var filedSymbol in fieldSymbols)
            {
                WriteContent(3, $"if(!string.IsNullOrEmpty({filedSymbol.Name}))", stringBuilder);
                WriteContent(4, $"asyncPath.Add({filedSymbol.Name});", stringBuilder);
            }
            WriteContent(2, "}", stringBuilder);
            WriteContent(1, "}", stringBuilder);
            if (!string.IsNullOrEmpty(namespaceName))
            {
                stringBuilder.AppendLine("}");
            }
            return stringBuilder.ToString();
        }
    }


    public partial class SyntaxReceiver
    {
        public Dictionary<INamedTypeSymbol, List<IFieldSymbol>> SyncAssetPathConfig = new Dictionary<INamedTypeSymbol, List<IFieldSymbol>>(SymbolEqualityComparer.Default);
        public Dictionary<INamedTypeSymbol, List<IFieldSymbol>> AsyncAssetPathConfig = new Dictionary<INamedTypeSymbol, List<IFieldSymbol>>(SymbolEqualityComparer.Default);

        private bool CheckBaseType(INamedTypeSymbol type,string typeSimpleName)
        {
            while(type != null)
            {
                if (type.Name == typeSimpleName)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }

        public void VisitAssetPathSyntaxNode(GeneratorSyntaxContext context)
        {
            if (!(context.Node is FieldDeclarationSyntax filedDeclare) ||
                filedDeclare.AttributeLists.Count == 0)
            {
                return;
            }

            foreach (VariableDeclaratorSyntax variable in filedDeclare.Declaration.Variables)
            {
                IFieldSymbol typeSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
                if (typeSymbol != null && filedDeclare.AttributeLists.Count > 0)
                {
                    if (!CheckBaseType(typeSymbol.ContainingType, "BaseAssetEntity"))
                    {
                        continue;
                    }

                    var attribute = typeSymbol.GetAttributes();
                    if (attribute.Any(x => x.AttributeClass.ToDisplayString() == "DH.Game.AssetPathAttribute"))
                    {
                        if (AsyncAssetPathConfig.TryGetValue(typeSymbol.ContainingType, out var asyncAssetPath))
                        {
                            asyncAssetPath.Add(typeSymbol);
                        }
                        else
                        {
                            asyncAssetPath = new List<IFieldSymbol> { typeSymbol };
                            AsyncAssetPathConfig.Add(typeSymbol.ContainingType, asyncAssetPath);
                        }
                        return;
                    }

                    if (attribute.Any(x => x.AttributeClass.ToDisplayString() == "DH.Game.SyncAssetPathAttribute"))
                    {
                        if (SyncAssetPathConfig.TryGetValue(typeSymbol.ContainingType, out var syncAssetPath))
                        {
                            syncAssetPath.Add(typeSymbol);
                        }
                        else
                        {
                            syncAssetPath = new List<IFieldSymbol> { typeSymbol };
                            SyncAssetPathConfig.Add(typeSymbol.ContainingType, syncAssetPath);
                        }
                        return;
                    }
                }
            }
        }
    }
}
