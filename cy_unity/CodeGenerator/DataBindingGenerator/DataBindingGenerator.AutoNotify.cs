using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DataBindingGenerator
{
    public partial class DataBindingCodeGenerator
    {
        private void GenerateAutoNotifyCode(DataBindingSyntaxReceiver receiver, GeneratorExecutionContext context)
        {
            foreach (var item in receiver.AutoNotifyProperties)
            {
                var classSource = ProcessClass(item.Key, item.Value, receiver);
                context.AddSource($"{item.Key.Name}_AutoNotifyCode_g.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }

        private string ProcessClass(INamedTypeSymbol classSymbol, List<NotifyItem> fieldSymbols,
            DataBindingSyntaxReceiver receiver)
        {
            var stringBuilder = receiver.stringBuilder;
            stringBuilder.Length = 0;
            stringBuilder.AppendLine($"// {receiver.AutoNotifyProperties.Count}");
            stringBuilder.AppendLine($"// Update Time:{DateTime.Now}");
            stringBuilder.AppendLine("using System.Collections.Specialized;");
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using DH.Data;");
            stringBuilder.AppendLine("using UnityEngine.Pool;");
            stringBuilder.AppendLine($"namespace {classSymbol.ToDisplayString().Replace($".{classSymbol.Name}", null)}");
            stringBuilder.AppendLine("{");
            WriteContent(1, $"public partial class {classSymbol.Name}", stringBuilder);
            WriteContent(1, "{", stringBuilder);
            foreach (var fieldSymbol in fieldSymbols)
            {
                GeneratePropertyCode(fieldSymbol, 2, stringBuilder);
            }
            WriteContent(1, "}", stringBuilder);
            stringBuilder.AppendLine("}");
            return stringBuilder.ToString();
        }

        private void GeneratePropertyCode(NotifyItem fieldSymbol, int intent, StringBuilder stringBuilder)
        {
            WriteContent(intent, $"[global::System.Diagnostics.DebuggerNonUserCodeAttribute]", stringBuilder);
            WriteContent(intent, $"public {fieldSymbol.field.Type.Display()} {UpperCaseCamelCase(fieldSymbol.field.Name)}", stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            WriteContent(intent+1, $"get => {fieldSymbol.field.Name};", stringBuilder);
            if (!fieldSymbol.field.IsReadOnly)
            {
                if (fieldSymbol.setterAccess == "public")
                {
                    WriteContent(intent + 1, "set", stringBuilder);
                }
                else
                {
                    WriteContent(intent + 1, $"{fieldSymbol.setterAccess} set", stringBuilder);
                }

                WriteContent(intent + 1, "{", stringBuilder);
                if (fieldSymbol.changeForTargets == null)
                {
                    WriteContent(intent + 2, $"Set(ref {fieldSymbol.field.Name}, value);", stringBuilder);
                }
                else
                {
                    WriteContent(intent + 2, $"if(!Set(ref {fieldSymbol.field.Name}, value))", stringBuilder);
                    WriteContent(intent + 3, $"return;", stringBuilder);
                    foreach (var item in fieldSymbol.changeForTargets)
                    {
                        WriteContent(intent + 2, $"RaisePropertyChanged(\"{item}\");", stringBuilder);
                    }
                }
                WriteContent(intent + 1, "}", stringBuilder);
            }
            WriteContent(intent, "}", stringBuilder);
        }
    }
    
    public class NotifyItem
    {
        public IFieldSymbol field;
        public string setterAccess;
        public List<string> changeForTargets;
    }

    public enum NotifyAccess
    {
        Public,
        Private,
        Protected,
        Internal,
    }

    public partial class DataBindingSyntaxReceiver
    {
        public Dictionary<INamedTypeSymbol, List<NotifyItem>> AutoNotifyProperties = new Dictionary<INamedTypeSymbol, List<NotifyItem>>(SymbolEqualityComparer.Default);

        public void VisitAutoNotifySyntaxNode(GeneratorSyntaxContext context)
        {
            if (!(context.Node is FieldDeclarationSyntax fieldDeclarationSyntax) || fieldDeclarationSyntax.AttributeLists.Count == 0)
            {
                return;
            }

            foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables)
            {
                // Get the symbol being declared by the field, and keep it if its annotated
                IFieldSymbol fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
                if (!fieldSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == "DH.UIFramework.AutoNotifyAttribute"))
                {
                    continue;
                }

                var notifyItem = new NotifyItem()
                {
                    field = fieldSymbol,
                };

                var attributes = fieldSymbol.GetAttributes();
                foreach (var attribute in attributes)
                {
                    if(attribute.AttributeClass.ToDisplayString() == "DH.UIFramework.ChangeForAttribute")
                    {
                        if (notifyItem.changeForTargets == null)
                        {
                            notifyItem.changeForTargets = new List<string>();
                        }

                        var targetTypeName = attribute.ConstructorArguments[0].Value.ToString();
                        notifyItem.changeForTargets.Add(targetTypeName);
                    }
                    else if (attribute.AttributeClass.ToDisplayString() == "DH.UIFramework.AutoNotifyAttribute")
                    {
                        if(attribute.ConstructorArguments.Length == 0)
                        {
                            notifyItem.setterAccess = "public";
                        }
                        else
                        {
                            var paraValue = (int)attribute.ConstructorArguments[0].Value;
                            notifyItem.setterAccess = ((NotifyAccess)(paraValue)).ToString().ToLowerInvariant();
                        }                    
                    }
                }
                var parentClass = fieldSymbol.ContainingType;
                if (AutoNotifyProperties.TryGetValue(parentClass, out var symbols))
                {
                    symbols.Add(notifyItem);
                }
                else
                {
                    symbols = new List<NotifyItem>() { notifyItem };
                    AutoNotifyProperties.Add(parentClass, symbols);
                }
            }
        }
    }
}
