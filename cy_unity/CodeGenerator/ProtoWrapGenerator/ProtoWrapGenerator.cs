using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceGenerator
{
    [Generator]
    public class ProtoWrapGenerator : ISourceGenerator
    {
        GeneratorExecutionContext Context;
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
                    var source = OutputExceptionMessage(receiver, receiver.ErrorMsg, "ProtoWrapError");
                    context.AddSource($"CodeGeneratorError_Code_g.cs", SourceText.From(source, Encoding.UTF8));
                    return;
                }

                GenerateProtoWrapCode(receiver, context);
                {
                    var source = OutputExceptionMessage(receiver, $"{receiver.logMsg}", "ProtoWrapError");
                    context.AddSource($"CodeGeneratorError_Code_g.cs", SourceText.From(source, Encoding.UTF8));
                }
            }
            catch (Exception ex)
            {
                var source = OutputExceptionMessage(receiver, $"{receiver.ErrorMsg}\r\n{receiver.logMsg.ToString()}\r\n{ex}", "ProtoWrapError");
                context.AddSource($"CodeGeneratorError_Code_g.cs", SourceText.From(source, Encoding.UTF8));
            }
        }

        public static string OutputExceptionMessage(ISyntaxContextReceiver receiver, string ex, string className = "CodeGeneratorError")
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

        public static void WriteContent(int intent, string content, StringBuilder stringBuilder, bool newLine = true)
        {
            for (int i = 0; i < intent; i++)
            {
                stringBuilder.Append("\t");
            }

            stringBuilder.Append(content);
            if (newLine)
            {
                stringBuilder.Append(Environment.NewLine);
            }
        }

        private void GenerateProtoWrapCode(SyntaxReceiver receiver, GeneratorExecutionContext context)
        {
            foreach (var keyValuePair in receiver.Classes)
            {
                var classType = keyValuePair.Key;
                if (!receiver.ProtoClasses.TryGetValue(keyValuePair.Value, out var wrapItem))
                {
                    wrapItem = TryAddClass(keyValuePair.Value);
                    if (wrapItem == null)
                    {
                        continue;
                    }
                }
                wrapItem.wrapTypeSymbol = classType;
            }

            foreach (var keyValuePair in receiver.Classes)
            {
                var classType = keyValuePair.Key;
                if (!receiver.ProtoClasses.TryGetValue(keyValuePair.Value, out var wrapItem))
                {
                    continue;
                }

                var classSource = ProcessClass(classType, wrapItem, receiver);
                context.AddSource($"{classType.Name}_WrapCode_g.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }

        private WrapItem TryAddClass(string className)
        {
            var context = Context;
            var classSymbol = context.Compilation.GetTypeByMetadataName(className);
            if (classSymbol == null)
            {
                return null;
            }

            var wrapItem = new WrapItem()
            {
                protoTypeSymbol = classSymbol,
            };

            (context.SyntaxContextReceiver as SyntaxReceiver).ProtoClasses.Add(classSymbol.ToDisplayString(), wrapItem);
            foreach (var member in classSymbol.GetMembers())
            {
                if (!(member is IPropertySymbol propertySymbol))
                {
                    continue;
                }

                if (propertySymbol.IsStatic || propertySymbol.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                wrapItem.properties.Add(propertySymbol);
            }

            return wrapItem;
        }

        private string ProcessClass(INamedTypeSymbol classSymbol, WrapItem wrapItem, SyntaxReceiver receiver)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"// {receiver.ProtoClasses.Count}");
            stringBuilder.AppendLine($"// {DateTime.Now}");
            stringBuilder.AppendLine("using DH.UIFramework.Observables;");
            stringBuilder.AppendLine("using Google.Protobuf;");
            stringBuilder.AppendLine("namespace DH.Data");
            stringBuilder.AppendLine("{");
            WriteContent(1, $"public partial class {classSymbol.Name}", stringBuilder);
            WriteContent(1, "{", stringBuilder);
            WriteContent(2, $"private readonly {wrapItem.protoTypeSymbol.ToDisplayString()} nestMessage;", stringBuilder);

            foreach (var prop in wrapItem.properties)
            {
                GenerateCollection(prop, 2, stringBuilder, receiver);
            }

            foreach (var prop in wrapItem.properties)
            {
                GenerateProtoWrap(prop, 2, stringBuilder, receiver);
            }

            WriteContent(0, "", stringBuilder);
            GenerateConstructor(classSymbol, wrapItem.protoTypeSymbol, wrapItem.properties, 2, stringBuilder, receiver);

            if(GetSyncFlagSymbole(wrapItem.properties) != null)
            {
                GenerateUpdateMessageWithSyncFlag(classSymbol, wrapItem.protoTypeSymbol, wrapItem.properties, 2, stringBuilder, receiver);
            }
            else
            {
                GenerateUpdateMessage(classSymbol, wrapItem.protoTypeSymbol, wrapItem.properties, 2, stringBuilder, receiver);
            }

            foreach (var property in wrapItem.properties)
            {
                GenerateProperty(property, 2, stringBuilder, receiver);
            }

            WriteContent(1, "}", stringBuilder);
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        private void GenerateProtoWrap(IPropertySymbol property, int intent, StringBuilder stringBuilder, SyntaxReceiver receiver)
        {
            var propertyType = property.Type as INamedTypeSymbol;
            if (!propertyType.IsGenericType && IsWrapProtoMessage(propertyType.ToDisplayString(), receiver))
            {
                WriteContent(intent, $"private {GetReadableTypeName(propertyType, receiver, out var wrapped)} {LowerCaseCamelCase(property.Name)};", stringBuilder);
            }
        }

        private void GenerateCollection(IPropertySymbol property, int intent, StringBuilder stringBuilder, SyntaxReceiver receiver)
        {
            var propertyType = property.Type as INamedTypeSymbol;
            if (!propertyType.IsGenericType)
            {
                return;
            }

            WriteContent(intent, $"private readonly {GetReadableTypeName(propertyType, receiver, out var wrapped)} {LowerCaseCamelCase(property.Name)} = new ();", stringBuilder);
        }

        private void GenerateProperty(IPropertySymbol property, int intent, StringBuilder stringBuilder, SyntaxReceiver receiver)
        {
            WriteContent(intent, $"public {GetReadableTypeName(property.Type as INamedTypeSymbol, receiver, out var wrapped)} {property.Name}", stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            var propertyType = property.Type as INamedTypeSymbol;
            if (!propertyType.IsGenericType)
            {
                if (IsWrapProtoMessage(propertyType.ToDisplayString(), receiver))
                {
                    var propertyName = LowerCaseCamelCase(property.Name);

                    WriteContent(intent + 1, $"get => {propertyName};", stringBuilder);
                }
                else
                {
                    WriteContent(intent + 1, $"get => nestMessage.{property.Name};", stringBuilder);
                    WriteContent(intent + 1, $"set => Set(nestMessage.{property.Name},value,nestMessage,(msg,newValue)=>msg.{property.Name} = newValue);", stringBuilder);
                }
            }
            else
            {
                WriteContent(intent + 1, $"get => {LowerCaseCamelCase(property.Name)};", stringBuilder);
            }

            WriteContent(intent, "}", stringBuilder);
            WriteContent(0, "", stringBuilder);
        }

        private void GenerateConstructor(INamedTypeSymbol type, INamedTypeSymbol messageType, List<IPropertySymbol> propertyInfos, int intent, StringBuilder stringBuilder, SyntaxReceiver receiver)
        {
            WriteContent(intent, $"//用于手动构造本地数据模式", stringBuilder);
            WriteContent(intent, $"public {type.Name}() : this(new {messageType.ToDisplayString()}())", stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            WriteContent(intent, "}", stringBuilder);
            WriteContent(0, "", stringBuilder);

            WriteContent(intent, $"public {messageType.ToDisplayString()} Clone()", stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            WriteContent(intent + 1, $"return nestMessage.Clone();", stringBuilder);
            WriteContent(intent, "}", stringBuilder);
            WriteContent(0, "", stringBuilder);

            WriteContent(intent, $"public {type.Name}({messageType.ToDisplayString()} message)", stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            WriteContent(intent + 1, $"nestMessage = message;", stringBuilder);
            foreach (var property in propertyInfos)
            {
                var propertyType = property.Type as INamedTypeSymbol;
                if (!propertyType.IsGenericType)
                {
                    var proptyTypeName = propertyType.ToDisplayString();
                    if (IsWrapProtoMessage(proptyTypeName, receiver))
                    {
                        WriteContent(intent + 1, $"message.{property.Name} ??= new global::{receiver.ProtoClasses[proptyTypeName].protoTypeSymbol}();", stringBuilder);
                        WriteContent(intent + 1, $"{LowerCaseCamelCase(property.Name)} = new {GetReadableTypeName(propertyType, receiver, out var wrapped)}(message.{property.Name});", stringBuilder);
                        WriteContent(intent + 1, $"{LowerCaseCamelCase(property.Name)}.Init();", stringBuilder);
                    }
                    continue;
                }

                GenerateCollectionMerge(property, intent, stringBuilder, receiver);
            }
            WriteContent(intent, "}", stringBuilder);
            WriteContent(0, "", stringBuilder);
        }

        private IPropertySymbol GetSyncFlagSymbole(List<IPropertySymbol> propertySymbols)
        {
            const string SyncFlagClassName = "DH.Proto.SyncFlag";
            var item = propertySymbols.Find(x => x.Type.ToDisplayString() == SyncFlagClassName);
            return item;
        }

        private void GenerateUpdateMessageWithSyncFlag(INamedTypeSymbol type, INamedTypeSymbol messageType, List<IPropertySymbol> propertyInfos, int intent, StringBuilder stringBuilder, SyntaxReceiver receiver)
        {
            WriteContent(intent, $"public void MergeFrom({messageType.ToDisplayString()} message,bool clearCollection = false)", stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            GenerateCollectionWithSyncFlag(intent, stringBuilder);
            foreach (var property in propertyInfos)
            {
                var propertyType = property.Type as INamedTypeSymbol;
                if (propertyType.IsGenericType)
                {
                    GenerateCollectionMerge(property, intent, stringBuilder, receiver, true);
                }
                else
                {
                    if (IsWrapProtoMessage(propertyType.ToDisplayString(), receiver))
                    {
                        WriteContent(intent + 1, $"{LowerCaseCamelCase(property.Name)}.MergeFrom(message.{property.Name},clearCollection);", stringBuilder);
                    }
                    else
                    {
                        WriteContent(intent + 1, $"{property.Name} = message.{property.Name};", stringBuilder);
                    }
                }
            }
            WriteContent(intent, "}", stringBuilder);
            WriteContent(0, "", stringBuilder);
        }

        private void GenerateUpdateMessage(INamedTypeSymbol type, INamedTypeSymbol messageType, List<IPropertySymbol> propertyInfos, int intent, StringBuilder stringBuilder, SyntaxReceiver receiver)
        {
            WriteContent(intent, $"public void MergeFrom({messageType.ToDisplayString()} message,bool clearCollection = false)", stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            foreach (var property in propertyInfos)
            {
                var propertyType = property.Type as INamedTypeSymbol;
                if (propertyType.IsGenericType)
                {
                    GenerateCollectionMerge(property, intent, stringBuilder, receiver, true);
                }
                else
                {
                    if (IsWrapProtoMessage(propertyType.ToDisplayString(), receiver))
                    {
                        WriteContent(intent + 1, $"{LowerCaseCamelCase(property.Name)}.MergeFrom(message.{property.Name},clearCollection);", stringBuilder);
                    }
                    else
                    {
                        WriteContent(intent + 1, $"{property.Name} = message.{property.Name};", stringBuilder);
                    }
                }
            }
            WriteContent(intent, "}", stringBuilder);
            WriteContent(0, "", stringBuilder);
        }

        private void GenerateCollectionWithSyncFlag(int intent, StringBuilder stringBuilder)
        {
            WriteContent(intent + 1, $"if (message == null) return;", stringBuilder);
            WriteContent(intent + 1, $"switch (message.Flag)", stringBuilder);
            WriteContent(intent + 1, "{", stringBuilder);
            WriteContent(intent + 2, "case DH.Proto.SyncFlag.All:", stringBuilder);
            WriteContent(intent + 3, "clearCollection = true;", stringBuilder);
            WriteContent(intent + 3, "break;", stringBuilder);

            WriteContent(intent + 2, "case DH.Proto.SyncFlag.Remove:", stringBuilder);
            WriteContent(intent + 3, "return;", stringBuilder);

            WriteContent(intent + 2, "case DH.Proto.SyncFlag.Flag:", stringBuilder);
            WriteContent(intent + 3, "return;", stringBuilder);

            WriteContent(intent + 2, "case DH.Proto.SyncFlag.Update:", stringBuilder);
            WriteContent(intent + 3, "clearCollection = false;", stringBuilder);
            WriteContent(intent + 3, "break;", stringBuilder);


            WriteContent(intent + 2, "default:", stringBuilder);
            WriteContent(intent + 3, "break;", stringBuilder);

            WriteContent(intent + 1, "}", stringBuilder);
        }

        private void GenerateCollectionMerge(IPropertySymbol property, int intent, StringBuilder stringBuilder, SyntaxReceiver receiver, bool clearCollection = false)
        {
            var propertyType = property.Type as INamedTypeSymbol;
            var propertyName = propertyType.Name;
            bool isDictionary = propertyName == "ObservableDictionary" || propertyName == "Dictionary" || propertyName == "MapField";
            INamedTypeSymbol paramType;
            string wrapType;
            bool wrapped;
            if (isDictionary)
            {
                paramType = propertyType.TypeArguments[1] as INamedTypeSymbol;
                wrapType = GetReadableTypeName(paramType, receiver, out wrapped);
            }
            else
            {
                paramType = propertyType.TypeArguments[0] as INamedTypeSymbol;
                wrapType = GetReadableTypeName(paramType, receiver, out wrapped);
            }

            if (clearCollection)
            {
                WriteContent(intent + 1, $"if(clearCollection)", stringBuilder);
                WriteContent(intent + 1, "{", stringBuilder);
                if (!propertyType.IsValueType && wrapped)
                {
                    WriteContent(intent + 2, $"foreach(var genCodeItem in {LowerCaseCamelCase(property.Name)})", stringBuilder);
                    WriteContent(intent + 2, "{", stringBuilder);
                    if (isDictionary)
                    {
                        WriteContent(intent + 3, "genCodeItem.Value.Clear();", stringBuilder);
                    }
                    else
                    {
                        WriteContent(intent + 3, "genCodeItem.Clear();", stringBuilder);
                    }
                    WriteContent(intent + 2, "}", stringBuilder);
                }
                WriteContent(intent + 2, $"{LowerCaseCamelCase(property.Name)}.Clear();", stringBuilder);
                WriteContent(intent + 1, "}", stringBuilder);
                WriteContent(0, "", stringBuilder);
            }

            WriteContent(intent + 1, $"foreach(var genCodeItem in message.{property.Name})", stringBuilder);
            WriteContent(intent + 1, "{", stringBuilder);
            if (isDictionary)
            {
                if (wrapped && !paramType.IsValueType)
                {
                    WriteContent(intent + 2, $"if(!{LowerCaseCamelCase(property.Name)}.TryGetValue(genCodeItem.Key,out var item))", stringBuilder);
                    WriteContent(intent + 2, "{", stringBuilder);
                    WriteContent(intent + 3, $"item = new {wrapType}(genCodeItem.Value);", stringBuilder);
                    WriteContent(intent + 3, $"item.Init();", stringBuilder);
                    WriteContent(intent + 3, $"{LowerCaseCamelCase(property.Name)}.Add(genCodeItem.Key,item);", stringBuilder);
                    WriteContent(intent + 2, "}", stringBuilder);
                    WriteContent(intent + 2, "else", stringBuilder);
                    WriteContent(intent + 2, "{", stringBuilder);
                    WriteContent(intent + 3, "item.MergeFrom(genCodeItem.Value,true);", stringBuilder);
                    WriteContent(intent + 2, "}", stringBuilder);
                }
                else
                {
                    WriteContent(intent + 2,
                    paramType.IsValueType
                        ? $"{LowerCaseCamelCase(property.Name)}.Add(genCodeItem.Key,genCodeItem.Value);"
                        : $"{LowerCaseCamelCase(property.Name)}.Add(genCodeItem.Key,new {wrapType}(genCodeItem.Value));",
                    stringBuilder);
                }
            }
            else
            {
                if (wrapped && !paramType.IsValueType)
                {
                    WriteContent(intent + 2, $"var item = new {wrapType}(genCodeItem);", stringBuilder);
                    WriteContent(intent + 2, $"item.Init();", stringBuilder);
                    WriteContent(intent + 2, $"{LowerCaseCamelCase(property.Name)}.Add(item);", stringBuilder);
                }
                else
                {
                    WriteContent(intent + 2,
                    paramType.IsValueType
                        ? $"{LowerCaseCamelCase(property.Name)}.Add(genCodeItem);"
                        : $"{LowerCaseCamelCase(property.Name)}.Add(new {wrapType}(genCodeItem));", stringBuilder);
                }
            }
            WriteContent(intent + 1, "}", stringBuilder);
            WriteContent(0, "", stringBuilder);
        }

        private string GetReadableTypeName(INamedTypeSymbol type, SyntaxReceiver receiver, out bool wrapped)
        {
            if (receiver.ProtoClasses.TryGetValue(type.ToDisplayString(), out var wrapItem))
            {
                wrapped = true;
                return wrapItem.wrapTypeSymbol.ToDisplayString();
            }
            wrapped = false;
            return GetTypeName(type, receiver);
        }

        private static bool IsWrapProtoMessage(string typeName, SyntaxReceiver receiver)
        {
            return receiver.ProtoClasses.ContainsKey(typeName);
        }

        internal static string LowerCaseCamelCase(string name)
        {
            return Char.ToLowerInvariant(name[0]) + name.Substring(1);
        }

        private string GetTypeName(ITypeSymbol typeSymbol, SyntaxReceiver receiver)
        {
            if (!(typeSymbol is INamedTypeSymbol namedTypeSymbol))
            {
                return typeSymbol.Name;
            }

            if (namedTypeSymbol.IsGenericType)
            {
                var displayName = namedTypeSymbol.ToDisplayString();
                if (namedTypeSymbol.Name == "RepeatedField")
                {
                    displayName = displayName.Replace("Google.Protobuf.Collections.RepeatedField", "DH.UIFramework.Observables.ObservableList");
                }

                if (namedTypeSymbol.Name == "MapField")
                {
                    displayName = displayName.Replace("Google.Protobuf.Collections.MapField", "DH.UIFramework.Observables.ObservableDictionary");
                }

                foreach (var item in namedTypeSymbol.TypeArguments)
                {
                    if (receiver.ProtoClasses.TryGetValue(item.ToDisplayString(), out var type))
                    {
                        displayName = displayName.Replace(item.ToDisplayString(), type.wrapTypeSymbol.ToDisplayString());
                    }
                    else
                    {
                        // do nothing
                    }
                }

                return displayName;
            }

            return namedTypeSymbol.ToDisplayString();
        }
    }

    public class WrapItem
    {
        public INamedTypeSymbol protoTypeSymbol;
        public INamedTypeSymbol wrapTypeSymbol;
        public List<IPropertySymbol> properties = new List<IPropertySymbol>();
    }

    public class SyntaxReceiver : ISyntaxContextReceiver
    {
        public string ErrorMsg;
        public StringBuilder logMsg = new StringBuilder();
        public Dictionary<INamedTypeSymbol, string> Classes { get; } = new Dictionary<INamedTypeSymbol, string>(SymbolEqualityComparer.Default);
        public Dictionary<string, WrapItem> ProtoClasses { get; } = new Dictionary<string, WrapItem>();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            try
            {
                VisitProtoWrapSyntaxNode(context);
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
            }
        }

        public void VisitProtoWrapSyntaxNode(GeneratorSyntaxContext context)
        {
            if (!(context.Node is ClassDeclarationSyntax classSyntax))
            {
                return;
            }

            INamedTypeSymbol typeSymbol = context.SemanticModel.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;
            if (IsDerivedFrom(typeSymbol, "Google.Protobuf.IBufferMessage"))
            {
                WrapItem item = new WrapItem()
                {
                    protoTypeSymbol = typeSymbol,
                };
                ProtoClasses.Add(typeSymbol.ToDisplayString(), item);

                foreach (var member in typeSymbol.GetMembers())
                {
                    if (!(member is IPropertySymbol propertySymbol))
                    {
                        continue;
                    }

                    if (propertySymbol.IsStatic || propertySymbol.DeclaredAccessibility != Accessibility.Public)
                    {
                        continue;
                    }

                    item.properties.Add(propertySymbol);
                }

                return;
            }

            if (classSyntax.Modifiers.Any(SyntaxKind.PartialKeyword) && classSyntax.AttributeLists.Count > 0)
            {
                if (typeSymbol.BaseType.ToDisplayString() != "DH.Data.BaseData")
                {
                    return;
                }

                var attribute = typeSymbol.GetAttributes().First();
                if (attribute?.AttributeClass.Name != "ProtoWrapAttribute")
                {
                    return;
                }

                var targetTypeName = attribute.ConstructorArguments[0].Value.ToString();
                Classes.Add(typeSymbol, targetTypeName);
            }
        }


        private bool IsDerivedFrom(INamedTypeSymbol baseType, string targetType)
        {
            if (baseType.ToDisplayString() == targetType)
                return true;

            var interfaces = baseType.AllInterfaces;
            foreach (var iface in interfaces)
            {
                if (iface.ToDisplayString() == targetType) return true;
            }

            return false;
        }
    }
}
