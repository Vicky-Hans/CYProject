using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace DataBindingGenerator
{
    [Generator]
    public partial class DataBindingCodeGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new DataBindingSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxContextReceiver is DataBindingSyntaxReceiver receiver))
            {
                return;
            }
            try
            {
                if (!string.IsNullOrEmpty(receiver.ErrorMsg))
                {
                    var source = OutputExceptionMessage(receiver, receiver.ErrorMsg, "DataBindingError");
                    context.AddSource($"DataBindingError_Code_g.cs", SourceText.From(source, Encoding.UTF8));
                    return;
                }

                GenerateDataBindingCode(receiver, context);
                GenerateCollectionBindingCode(receiver, context);
                GenerateAutoNotifyCode(receiver, context);
                GenerateCommandMethodCode(receiver, context);
                {
                    var source = OutputExceptionMessage(receiver, $"{receiver.logMsg}", "DataBindingError");
                    context.AddSource($"DataBindingError_Code_g.cs", SourceText.From(source, Encoding.UTF8));
                }
            }
            catch (Exception ex)
            {
                var source = OutputExceptionMessage(receiver, $"{receiver.ErrorMsg}\r\n{receiver.logMsg.ToString()}\r\n{ex}", "DataBindingError");
                context.AddSource($"DataBindingError_Code_g.cs", SourceText.From(source, Encoding.UTF8));
            }
        }

        public static string OutputExceptionMessage(DataBindingSyntaxReceiver receiver, string ex, string className = "CodeGeneratorError")
        {
            var stringBuilder = receiver.stringBuilder;
            stringBuilder.Length = 0;
            stringBuilder.AppendLine($"/* {DateTime.Now} */");
            stringBuilder.AppendLine($"/* {ex} */");
            stringBuilder.AppendLine($"/* {receiver.IndirectBindingDatas.Count} */");
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

        private void GenerateDataBindingCode(DataBindingSyntaxReceiver receiver, GeneratorExecutionContext context)
        {
            INamedTypeSymbol namedTypeSymbol = null;
            foreach (var item in receiver.BindingDatas)
            {
                var classSource = ProcessClass(item.Key, item.Value, receiver, context);
                if (string.IsNullOrEmpty(classSource))
                {
                    continue;
                }
                namedTypeSymbol = item.Key;
                context.AddSource($"{item.Key.Name}_DataBindingCode_g.cs", SourceText.From(classSource, Encoding.UTF8));
            }

            foreach (var item in receiver.IndirectBindingDatas)
            {
                var classSource = ProcessClass(item.Key, item.Value, receiver, context);
                if (string.IsNullOrEmpty(classSource))
                {
                    continue;
                }
                namedTypeSymbol = item.Key;
                context.AddSource($"{item.Key.Name}_IndirectDataBindingCode_g.cs", SourceText.From(classSource, Encoding.UTF8));
            }

            if (namedTypeSymbol == null || receiver.PropertyCache.Count == 0)
            {
                return;
            }
            {
                var classSource = ProcessPropertyCacheClass(receiver, context,namedTypeSymbol);
                if (string.IsNullOrEmpty(classSource))
                {
                    return;
                }
                context.AddSource($"TargetProxyCache_g.cs", SourceText.From(classSource, Encoding.UTF8));
            }
            
        }

        private string ProcessPropertyCacheClass(DataBindingSyntaxReceiver receiver, GeneratorExecutionContext context,INamedTypeSymbol namedTypeSymbol)
        {
            var stringBuilder = receiver.stringBuilder;
            stringBuilder.Length = 0;
            stringBuilder.AppendLine($"// {receiver.PropertyCache.Count}");
            stringBuilder.AppendLine($"// Update Time:{DateTime.Now}");
            stringBuilder.AppendLine($"namespace {namedTypeSymbol?.ContainingAssembly?.Name}");
            stringBuilder.AppendLine("{");
            WriteContent(1, $"public static class TargetProxyCache", stringBuilder);
            WriteContent(1, "{", stringBuilder);
            foreach(var item in receiver.PropertyCache)
            {
                var classItem = item.Value;
                foreach(var prop in classItem.propertyName)
                {
                    // interactable
                    var returnTypeName = prop.Value.returnType?.Display() ?? "bool";
                    WriteContent(2, $"public static readonly System.Func<{item.Key.Display()},{returnTypeName}> C{classItem.index}Prop{prop.Value.index}Getter = {prop.Value.GenerateGetterCode(prop.Key)};", stringBuilder);
                    WriteContent(2, $"public static readonly System.Action<{item.Key.Display()},{returnTypeName}> C{classItem.index}Prop{prop.Value.index}Setter = {prop.Value.GenerateSetterCode(prop.Key)};", stringBuilder);
                }
            }
            WriteContent(1, "}", stringBuilder);
            stringBuilder.AppendLine("}");
            return stringBuilder.ToString();
        }

        private string ProcessClass(INamedTypeSymbol classSymbol,Dictionary<IMethodSymbol, MethodBinding> bindingDatas,DataBindingSyntaxReceiver receiver, GeneratorExecutionContext context)
        {
            var stringBuilder = receiver.stringBuilder;
            stringBuilder.Length = 0;
            stringBuilder.AppendLine($"// {receiver.IndirectBindingDatas.Count}");
            stringBuilder.AppendLine($"// Update Time:{DateTime.Now}");
            if(receiver.UsingDatas.TryGetValue(classSymbol,out var usingDatas))
            {
                foreach (var item in usingDatas)
                {
                    stringBuilder.Append(item);
                }
            }
            else
            {
                receiver.logMsg.AppendLine($"Missing using data {classSymbol}");
            }
            var namespaceName = classSymbol.ToDisplayString().Replace($".{classSymbol.Name}", null);
            if (!string.IsNullOrEmpty(namespaceName) && namespaceName != classSymbol.Name)
            {
                stringBuilder.AppendLine($"namespace {namespaceName}");
                stringBuilder.AppendLine("{");
            }

            WriteContent(1, $"public partial class {classSymbol.Name} : {classSymbol.BaseType}", stringBuilder);
            WriteContent(1, "{", stringBuilder);
      
            foreach(var method in bindingDatas)
            {
                var type = method.Key.Parameters.FirstOrDefault(x => x.Type.Name == "BindingSet").Type;
                WriteContent(2, $"{method.Value.methodDeclare}", stringBuilder);
                WriteContent(2, "{", stringBuilder);
                foreach (var bindingData in method.Value.datas)
                {
                    bindingData.startIntent = 3;
                    bindingData.Buffer = stringBuilder;
                    bindingData.Writer = WriteContent;
                    bindingData.receiver = receiver;
                    bindingData.context = context;
                    bindingData.Init();
                    bindingData.GenerateCode();
                }
                WriteContent(2, "}", stringBuilder);
            }

            WriteContent(1, "}", stringBuilder);
            if (!string.IsNullOrEmpty(namespaceName) && namespaceName != classSymbol.Name)
            {
                stringBuilder.AppendLine("}");
            }
            return stringBuilder.ToString();
        }

        private string ProcessClass(INamedTypeSymbol classSymbol, List<BindingData> bindingDatas,
        DataBindingSyntaxReceiver receiver, GeneratorExecutionContext context)
        {
            var stringBuilder = receiver.stringBuilder;
            stringBuilder.Length = 0;
            stringBuilder.AppendLine($"// {receiver.IndirectBindingDatas.Count}");
            stringBuilder.AppendLine($"// Update Time:{DateTime.Now}");
            if (receiver.UsingDatas.TryGetValue(classSymbol, out var usingDatas))
            {
                foreach (var item in usingDatas)
                {
                    stringBuilder.Append(item);
                }
            }
            else
            {
                receiver.logMsg.AppendLine($"Missing using data {classSymbol}");
            }
            var namespaceName = classSymbol.ToDisplayString().Replace($".{classSymbol.Name}", null);
            if (!string.IsNullOrEmpty(namespaceName) && namespaceName != classSymbol.Name)
            {
                stringBuilder.AppendLine($"namespace {namespaceName}");
                stringBuilder.AppendLine("{");
            }

            WriteContent(1, $"public partial class {classSymbol.Name} : {classSymbol.BaseType}", stringBuilder);
            WriteContent(1, "{", stringBuilder);
            WriteContent(2, "public override void InitializeBinding()", stringBuilder);
            WriteContent(2, "{", stringBuilder);
            WriteContent(3, $"var bindingSet = (DH.UIFramework.Builder.BindingSet<{classSymbol.Display()}, {bindingDatas[0].sourceType.Display()}>)(this.bindingSetBase);", stringBuilder);
            foreach (var bindingData in bindingDatas)
            {
                bindingData.startIntent = 3;
                bindingData.Buffer = stringBuilder;
                bindingData.Writer = WriteContent;
                bindingData.receiver = receiver;
                bindingData.context = context;
                bindingData.Init();
                bindingData.GenerateCode();
            }
            //WriteContent(3, "bindingSet.CompileBuild();", stringBuilder);
            WriteContent(2, "}", stringBuilder);
            WriteContent(1, "}", stringBuilder);
            if (!string.IsNullOrEmpty(namespaceName) && namespaceName != classSymbol.Name)
            {
                stringBuilder.AppendLine("}");
            }
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

        internal static string UpperCaseCamelCase(string name)
        {
            return Char.ToUpperInvariant(name[0]) + name.Substring(1);
        }

        internal static string LowerCaseCamelCase(string name)
        {
            return Char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
    }

    public class ClassItem
    {
        public int index;
        public Dictionary<string, CachePropertyInfo> propertyName = new Dictionary<string, CachePropertyInfo>();
    }

    public class CachePropertyInfo
    {
        public int index;
        public ClassItem owner;
        public INamedTypeSymbol returnType;
        /// <summary>
        /// string setter = targetName == "activeSelf" ? $"(t, v) =>\r\n {{\r\nif (t.activeSelf == v) return;t.SetActive(v);\r\n}}" : $"(t,val)=>t.{targetName} = val";
        /// string getter = $"t=>t.{targetName}";
        /// </summary>
        /// <param name="getter"></param>
        /// <param name="setter"></param>
        /// 
        public string GenerateSetterCode(string propertyName)
        {
            return propertyName == "activeSelf" ? $"(t, v) =>\r\n\t\t{{\r\n\t\t\tif (t.activeSelf == v) return;t.SetActive(v);\r\n\t\t}}" : $"(t,v)=>t.{propertyName} = v";
        }

        public string GenerateGetterCode(string propertyName)
        {
            return $"t=>t.{propertyName}";
        }

        public void GetFuncProperty(INamedTypeSymbol typeSymbol, out string getter, out string setter)
        {
            getter = $"{typeSymbol?.ContainingAssembly?.Name}.TargetProxyCache.C{owner?.index}Prop{index}Getter";
            setter = $"{typeSymbol?.ContainingAssembly?.Name}.TargetProxyCache.C{owner?.index}Prop{index}Setter";
        }
    }

    public class MethodBinding
    {
        public string methodDeclare;
        public List<BindingData> datas = new List<BindingData>();
     }

    public partial class DataBindingSyntaxReceiver : ISyntaxContextReceiver
    {
        public Dictionary<INamedTypeSymbol, List<BindingData>> BindingDatas = new Dictionary<INamedTypeSymbol, List<BindingData>>(SymbolEqualityComparer.Default);
        public Dictionary<INamedTypeSymbol, Dictionary<IMethodSymbol, MethodBinding>> IndirectBindingDatas = new Dictionary<INamedTypeSymbol, Dictionary<IMethodSymbol, MethodBinding>>(SymbolEqualityComparer.Default);
        public Dictionary<INamedTypeSymbol, List<string>> UsingDatas = new Dictionary<INamedTypeSymbol, List<string>>(SymbolEqualityComparer.Default);
        public Dictionary<INamedTypeSymbol, ClassItem> PropertyCache = new Dictionary<INamedTypeSymbol, ClassItem>(SymbolEqualityComparer.Default);
        public INamedTypeSymbol CommandType;
        public StringBuilder stringBuilder = new StringBuilder();
        private List<MemberAccessExpressionSyntax> cacheMembers = new List<MemberAccessExpressionSyntax>();
        public string ErrorMsg;
        public StringBuilder logMsg = new StringBuilder();

        public CachePropertyInfo TryAddProperty(INamedTypeSymbol typeSymbol, string propertyName,INamedTypeSymbol returnType)
        {
            if (!PropertyCache.TryGetValue(typeSymbol,out var item))
            {
                item = new ClassItem();
                item.index= PropertyCache.Count + 1;
                PropertyCache.Add(typeSymbol, item);    
            }

            if(item.propertyName.TryGetValue(propertyName,out var propertyInfo))
            {
                return propertyInfo;
            }
            else
            {
                propertyInfo = new CachePropertyInfo()
                {
                    index = item.propertyName.Count + 1,
                    owner = item,
                    returnType = returnType,
                };
                item.propertyName.Add(propertyName, propertyInfo);
                return propertyInfo;
            }
        }

        public void OnVisitBindingCreateDeclaration(GeneratorSyntaxContext context)
        {
            if(!(context.Node is MethodDeclarationSyntax methodDeclaration))
            {
                return;
            }

            var method = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
            if (method.Name == "Create" && method.IsOverride && method.ReturnsVoid)
            {
                var block = methodDeclaration.ChildNodes().FirstOrDefault(x => x is BlockSyntax);
                if (block == null)
                {
                    return;
                }

                foreach (var item in block.ChildNodes())
                {
                    VisitBindingSyntaxNode(item, method.ContainingType, context, null,null);
                }
            }
            else if (method.IsGenericMethod && method.Parameters.Length > 0
                && method.Parameters.Any(x => IsBindingSet(x)))
            {
                var block = methodDeclaration.ChildNodes().FirstOrDefault(x => x is BlockSyntax);
                if (block == null)
                {
                    return;
                }

                foreach (var item in block.ChildNodes())
                {                    
                    VisitBindingSyntaxNode(item, method.ContainingType, context, method,methodDeclaration);
                }
            }
        }


        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            try
            {
                VisitCollectionBindingSyntaxNode(context);
                VisitAutoNotifySyntaxNode(context);
                VisitCommandMethodSyntaxNode(context);
                OnVisitBindingCreateDeclaration(context);
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
            }
        }

        public void VisitBindingSyntaxNode(SyntaxNode node, INamedTypeSymbol classSymbol, GeneratorSyntaxContext context, IMethodSymbol methodSymbol,MethodDeclarationSyntax methodDeclaration)
        {
            VisitExpressionStatement(node,classSymbol,context,methodSymbol, methodDeclaration);
            VisitForStatement(node, classSymbol, context, methodSymbol, methodDeclaration);
            VisitIfStatement(node, classSymbol, context, methodSymbol, methodDeclaration);
        }

        public static INamedTypeSymbol GetClassSymbol(SyntaxNode syntax, GeneratorSyntaxContext context, out bool isPartial)
        {
            var parent = syntax.Parent;
            isPartial = false;
            while (parent != null)
            {
                if (parent is ClassDeclarationSyntax declarationSyntax)
                {
                    INamedTypeSymbol typeSymbol = context.SemanticModel.GetDeclaredSymbol(declarationSyntax) as INamedTypeSymbol;
                    isPartial = declarationSyntax.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword);
                    return typeSymbol;
                }

                parent = parent.Parent;
            }

            return null;
        }

        /// <summary>
        /// 分析使用If的绑定语法
        /// </summary>
        /// <param name="context"></param>
        private void VisitIfStatement(SyntaxNode node, INamedTypeSymbol classSymbol, GeneratorSyntaxContext context, IMethodSymbol methodSymbol,MethodDeclarationSyntax methodDeclaration)
        {
            if (!(node is IfStatementSyntax ifStatement))
            {
                return;
            }

            List<SyntaxNode> syntaxNodes = new List<SyntaxNode>();
            foreach (var item in ifStatement.ChildNodes())
            {
                if (item is BlockSyntax block)
                {
                    syntaxNodes.AddRange(block.ChildNodes());
                    continue;
                }

                if (item is ExpressionStatementSyntax statementSyntax)
                {
                    syntaxNodes.Add(statementSyntax);
                    continue;
                }
            }

            foreach(var item in syntaxNodes)
            {
                VisitExpressionStatement(item, classSymbol, context, methodSymbol, methodDeclaration);
            }
        }

        /// <summary>
        /// 分析使用For循环的绑定语法
        /// </summary>
        /// <param name="context"></param>
        private void VisitForStatement(SyntaxNode node, INamedTypeSymbol classSymbol, GeneratorSyntaxContext context, IMethodSymbol methodSymbol,MethodDeclarationSyntax methodDeclaration)
        {
            if (!(node is ForStatementSyntax forStatement) && !(node is ForEachStatementSyntax forEachStatement))
            {
                return;
            }

            var data = new MultipleBindingData();
            var forSyntax = node.ChildNodes();
            List<SyntaxNode> syntaxNodes = new List<SyntaxNode>();
            foreach (var item in forSyntax)
            {
                if (item is BlockSyntax block)
                {
                    syntaxNodes.AddRange(block.ChildNodes());
                    continue;
                }

                if (item is ExpressionStatementSyntax statementSyntax)
                {
                    syntaxNodes.Add(statementSyntax);
                    continue;
                }

                data.forSyntaxNodes.Add(item);
            }

            if (syntaxNodes.Count == 0)
            {
                return;
            }

            data.classSymbol = classSymbol;
            if (!UsingDatas.TryGetValue(classSymbol, out var usings))
            {
                var root = context.SemanticModel.SyntaxTree.GetCompilationUnitRoot();
                usings = new List<string>();
                foreach (var item in root.Usings)
                {
                    usings.Add(item.ToFullString());
                }
                UsingDatas[classSymbol] = usings;
            }

            bool needGenerateCode = false;
            foreach (var syntaxNode in syntaxNodes)
            {
                if (!(syntaxNode is ExpressionStatementSyntax expressionStatement))
                {
                    data.blockSyntaxNodes.Add(new ForSyntaxData() { syntaxNode = syntaxNode });
                    continue;
                }

                cacheMembers.Clear();
                var result = BindingHelper.CheckBindingStatement(expressionStatement, cacheMembers);
                if (!result)
                {
                    continue;
                }

                needGenerateCode = true;
                var subData = BindingHelper.CreateBindingData(cacheMembers, context.SemanticModel,this);
                subData.classSymbol = classSymbol;
                data.blockSyntaxNodes.Add(subData);
                data.sourceType = subData.sourceType;
            }

            if (needGenerateCode)
            {
                AddBindingData(classSymbol, methodSymbol, data, methodDeclaration);
            }
        }

        private bool IsBindingSet(IParameterSymbol parameter)
        {
            if(parameter == null)
            {
                return false;
            }

            if(!(parameter.Type is INamedTypeSymbol namedType) || !namedType.IsGenericType)
            {
                return false;
            }

            if(parameter.Type.Name != "BindingSet")
            {
                return false;
            }

            return true;
        }

        private void VisitExpressionStatement(SyntaxNode node,INamedTypeSymbol classSymbol,GeneratorSyntaxContext context,IMethodSymbol methodSymbol,MethodDeclarationSyntax methodDeclaration)
        {
            if (!(node is ExpressionStatementSyntax expressionStatement))
            {
                return;
            }

            // 函数调用
            if(expressionStatement.Expression is InvocationExpressionSyntax invocation)
            {
                var symbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                if (symbol != null && symbol.IsGenericMethod && symbol.Parameters.Length > 0)
                {
                    var paramType = symbol.Parameters.FirstOrDefault(x => IsBindingSet(x))?.Type as INamedTypeSymbol;
                    if(paramType != null)
                    {
                        if (!UsingDatas.TryGetValue(classSymbol, out var usingDatas))
                        {
                            var root = context.SemanticModel.SyntaxTree.GetCompilationUnitRoot();
                            usingDatas = new List<string>();
                            foreach (var item in root.Usings)
                            {
                                usingDatas.Add(item.ToFullString());
                            }
                            UsingDatas[classSymbol] = usingDatas;
                        }

                        var bindData = new InvocationBindingData()
                        {
                            invocationExpression = $"{symbol.Name}Generated(bindingSet); // {methodSymbol}",
                            sourceType = paramType.TypeArguments[1] as INamedTypeSymbol,
                            classSymbol = classSymbol,
                        };

                        AddBindingData(classSymbol, methodSymbol, bindData,methodDeclaration);
                        return;
                    }
                }           
            }


            cacheMembers.Clear();
            var result = BindingHelper.CheckBindingStatement(expressionStatement, cacheMembers);
            if (!result)
            {
                return;
            }

            if (!UsingDatas.TryGetValue(classSymbol, out var usings))
            {
                var root = context.SemanticModel.SyntaxTree.GetCompilationUnitRoot();
                usings = new List<string>();
                foreach (var item in root.Usings)
                {
                    usings.Add(item.ToFullString());
                }
                UsingDatas[classSymbol] = usings;
            }

            var data = BindingHelper.CreateBindingData(cacheMembers, context.SemanticModel,this);
            data.classSymbol = classSymbol;
            AddBindingData(classSymbol, methodSymbol, data, methodDeclaration);
        }

        private void AddBindingData(INamedTypeSymbol classSymbol,IMethodSymbol methodSymbol,BindingData binding,MethodDeclarationSyntax methodDeclaration)
        {
            binding.classSymbol = classSymbol;
            if (methodSymbol != null)
            {
                if (!IndirectBindingDatas.TryGetValue(classSymbol,out var methodCache))
                {
                    methodCache = new Dictionary<IMethodSymbol, MethodBinding>(SymbolEqualityComparer.Default);
                    IndirectBindingDatas.Add(classSymbol, methodCache);
                }

                if(!methodCache.TryGetValue(methodSymbol,out var methodBinding))
                {
                    methodBinding = new MethodBinding();
                    methodBinding.methodDeclare = $"{methodSymbol.DeclaredAccessibility.ToString().ToLowerInvariant()} {methodDeclaration.ReturnType} {methodSymbol.Name}Generated{methodDeclaration.TypeParameterList}{methodDeclaration.ParameterList}{methodDeclaration.ConstraintClauses}";
                    methodCache.Add(methodSymbol, methodBinding);
                }

                methodBinding.datas.Add(binding);
            }
            else
            {
                if (!BindingDatas.TryGetValue(classSymbol, out var list))
                {
                    list = new List<BindingData>();
                    BindingDatas.Add(classSymbol, list);
                }
                list.Add(binding);
            }
        }
    }
}
