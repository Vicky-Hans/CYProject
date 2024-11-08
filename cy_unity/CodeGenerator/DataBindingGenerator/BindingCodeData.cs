using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;

namespace DataBindingGenerator
{
    public static class BindingHelper
    {

        public static bool CheckBindingStatement(ExpressionStatementSyntax statementSyntax,List<MemberAccessExpressionSyntax> members)
        {
            GetBindingStatement(statementSyntax.ChildNodes(),members);
            int count = members.Count;
            if(count < 2)
            {
                return false;
            }

            var bindMemberName = members[count-1].Name.ToFullString().Trim();
            var forMemberName = members[count-2].Name.ToFullString().Trim();
            if(bindMemberName== "Bind" && forMemberName == "For")
            {
                return true;
            }
            return false;
        }

        private static void GetBindingStatement(IEnumerable<SyntaxNode> syntaxNodes,List<MemberAccessExpressionSyntax> members)
        {
            foreach (var node in syntaxNodes)
            {
                if ((node is MemberAccessExpressionSyntax member))
                {
                    members.Add(member);
                    GetBindingStatement(member.ChildNodes(), members);
                }
                else if(node is InvocationExpressionSyntax invocation)
                {
                    GetBindingStatement(invocation.ChildNodes(), members);
                }
            }
        }

        public static BindingData CreateBindingData(List<MemberAccessExpressionSyntax> members,SemanticModel semanticModel,DataBindingSyntaxReceiver receiver)
        {
            var bindingData = new BindingData();
            bindingData.semantic = semanticModel;
            foreach (var member in members)
            {
                var invocation = member.Parent as InvocationExpressionSyntax;
                var name = member.Name.Identifier.Text.Trim();
                switch (name)
                {
                    case "Bind":
                        bindingData.bindTarget = new BindTargetData() { invocation= invocation ,memberAccess = member,Owner = bindingData};
                        bindingData.bindTarget.Evaluate();
                        var methodSymbol = semanticModel.GetSymbolInfo(member.Name).Symbol as IMethodSymbol;
                        if(methodSymbol != null)
                        {
                            var returnType = methodSymbol.ReturnType as INamedTypeSymbol;
                            bindingData.targetType = returnType.TypeArguments[0] as INamedTypeSymbol;
                            bindingData.sourceType = returnType.TypeArguments[1] as INamedTypeSymbol;
                            if(bindingData.sourceType == null)
                            {
                                bindingData.sourceType = (returnType.TypeArguments[1] as ITypeParameterSymbol).ConstraintTypes[0] as INamedTypeSymbol;
                            }

                            if(bindingData.targetType == null)
                            {
                                bindingData.targetType = (returnType.TypeArguments[0] as ITypeParameterSymbol).ConstraintTypes[0] as INamedTypeSymbol;
                            }
                        }
                        break;

                    case "For":
                        bindingData.bindFor = new BindForData() { invocation = invocation, memberAccess = member, Owner = bindingData };
                        bindingData.bindFor.Evaluate();
                        break;

                    case "To":
                        bindingData.bindTo = new BindToData() { invocation = invocation, memberAccess = member, Owner = bindingData };
                        bindingData.bindTo.Evaluate();
                        break;

                    case "ToExpression":
                        bindingData.bindTo = new BindToData() { invocation = invocation, memberAccess = member, ExpressionMode = true, Owner = bindingData };
                        bindingData.bindTo.Evaluate();
                        break;

                    case "WithConversion":
                        bindingData.bindConverter = new BindConverterData() { invocation = invocation, memberAccess = member, Owner = bindingData };
                        bindingData.bindConverter.Evaluate();
                        break;

                    case "CommandParameter":
                        bindingData.bindCmdPara = new BindCommandPara() { invocation = invocation, memberAccess = member, Owner = bindingData };
                        bindingData.bindCmdPara.Evaluate();
                        break;

                    default:
                        bindingData.bindMode = name;
                        break;
                }
            }
            bindingData.semantic = null;
            return bindingData;
        }
    }

    public class BindTargetData : BindItemData 
    {
        public string targetAccess;

        public override void Evaluate()
        {
            var arguments = invocation.ArgumentList.Arguments;
            targetAccess = arguments.ToFullString();
        }

        public override void GenerateCode()
        {
            WriteContent(1, $"var builder = bindingSet.CompileCreate({targetAccess});");
        }
    }

    public class PropertyInfo
    {
        public int codeIndex;

        public string name;
        public bool elementIndex;
        public bool constantElement;
        public string elementKeyType;
        public bool isMethod;
        public INamedTypeSymbol returnTypeSymbol;
        public INamedTypeSymbol decalareType;
        public string returnTypeName = "DH.UIFramework.Commands.ICommand";
    }

    public class BindToData : BindItemData
    {
        public bool ExpressionMode;
        public List<List<ExpressionSyntax>> memberAccesses = new List<List<ExpressionSyntax>>();
        public List<string> lambdaBody = new List<string>();
        public string vmName;
        public List<List<PropertyInfo>> properties = new List<List<PropertyInfo>>();
        public List<INamedTypeSymbol> variableNames = new List<INamedTypeSymbol>();
        /// <summary>
        /// Dictionary容器绑定
        /// </summary>
        public INamedTypeSymbol collectionKeyType;
        public INamedTypeSymbol collectionValueType;
        public INamedTypeSymbol expressionReturnType;

        private const string CompiledSource = "DH.UIFramework.Proxy.Sources.Expressions.CompiledExpressionSourceDescription";
        private const string CompiledMemberNode = "DH.UIFramework.Paths.CompiledMemberNode";
        private static readonly DiagnosticDescriptor InvalidExpressionWarning = new DiagnosticDescriptor(id: "DataBinding",
                                                                                             title: "Couldn't parse expression with reason {0}",
                                                                                             messageFormat: "Couldn't parse expression with reason {0}",
                                                                                             category: "DataBindingGenerator",
                                                                                             DiagnosticSeverity.Error,
                                                                                             isEnabledByDefault: true);

        public override void Evaluate()
        {
            vmName = EvaluateLambdaArgument(invocation, memberAccesses, lambdaBody,ExpressionMode, variableNames);

            var symbol = Owner.semantic.GetSymbolInfo(invocation.Expression).Symbol as IMethodSymbol;
            if (symbol != null && symbol.IsGenericMethod && symbol.TypeArguments.Length > 0)
            {
                expressionReturnType = symbol.TypeArguments[0] as INamedTypeSymbol;
            }
        }

        public override void GenerateCode()
        {
            EvaluatePropertyType(Owner.sourceType);
            if(properties.Count == 0)
            {
                Owner.context.ReportDiagnostic(Diagnostic.Create(InvalidExpressionWarning, invocation.GetLocation(),"property parse failed"));
                return;
            }

            if (ExpressionMode)
            {
                if (lambdaBody.Count == 0 || variableNames.Count < properties.Count)
                {
                    Owner.context.ReportDiagnostic(Diagnostic.Create(InvalidExpressionWarning, invocation.GetLocation(),"lamda or vm name parse failed"));
                    return;
                }

                WriteContent(1, $"var pathList = new System.Collections.Generic.List<DH.UIFramework.Paths.Path>({properties.Count});");
                int index = 0;
                foreach (var items in properties)
                {
                    WriteContent(1, "{");
                    WriteContent(2, "var path = new DH.UIFramework.Paths.Path();");
                    GenerateBindPathCode(items, 1, variableNames[index]);
                    WriteContent(2, "pathList.Add(path);");
                    WriteContent(1, "}");
                    index++;
                }
                var returnType = expressionReturnType != null ? expressionReturnType : Owner.bindFor.typeSymbol;
                // 使用了Converter，同时绑定的属性由编译器生成导致类型推断失败，使用非泛型方式
                if(Owner.bindConverter != null && expressionReturnType == null)
                {
                    returnType = null;
                }

                if(returnType == null)
                {
                    var body = lambdaBody[0].Trim().Replace($"{vmName}.", $"(({Owner.sourceType.Display()}){vmName}).");
                    WriteContent(1, $"var sourceDesc = new {CompiledSource}({vmName}=>{body},typeof({Owner.targetType.Display()}),pathList);");
                    WriteContent(1, $"builder.Description.Source = sourceDesc;");
                }
                else if(!Owner.sourceType.IsValueType)
                {
                    var body = lambdaBody[0].Trim().Replace($"{vmName}.", $"(({Owner.sourceType.Display()}){vmName}).");
                    WriteContent(1, $"var sourceDesc = new {CompiledSource}<object,{returnType.Display()}>({vmName}=>{body},pathList);");
                    WriteContent(1, $"builder.Description.Source = sourceDesc;");
                }
                else
                {
                    var body = lambdaBody[0].Trim();
                    WriteContent(1, $"var sourceDesc = new {CompiledSource}<{Owner.sourceType.Display()},{returnType.Display()}>({vmName}=>{body},pathList);");
                    WriteContent(1, $"builder.Description.Source = sourceDesc;");
                }
            }
            else
            {
                if (variableNames.Count < properties.Count)
                {
                    Owner.context.ReportDiagnostic(Diagnostic.Create(InvalidExpressionWarning, invocation.GetLocation(),"invalid vm name count parsed"));
                    return;
                }

                WriteContent(1, "var sourceDesc = new DH.UIFramework.Proxy.Sources.Object.ObjectSourceDescription();");
                WriteContent(1, "var path = new DH.UIFramework.Paths.Path();");
                WriteContent(1, "builder.Description.Source = sourceDesc;");
                WriteContent(1, "sourceDesc.Path = path;");
                GenerateBindPathCode(properties[0], 0, variableNames[0]);
            }

            GeneateCollectWrapCode(1);
        }

        private void GeneateCollectWrapCode(int intent)
        {
            if(collectionValueType == null || collectionKeyType == null)
            {
                return;
            }

            WriteContent(intent, $"{Owner.bindTarget.targetAccess}.BindDictionaryGetValueFunc = (value)=>");
            WriteContent(intent+1, "{");
            WriteContent(intent+2, $"var  pair = (System.Collections.Generic.KeyValuePair<{collectionKeyType.Display()},{collectionValueType.Display()}>)value;");
            WriteContent(intent+2, $"return pair.Value;");
            WriteContent(intent+1, "};");
        }

        private void GenerateBindPathCode(List<PropertyInfo> propertyInfos,int intent, INamedTypeSymbol variableName)
        {
            if(propertyInfos.Count == 0)
            {
                return;
            }

            INamedTypeSymbol sourceType = Owner.sourceType;
            var lastProperty = propertyInfos[propertyInfos.Count - 1];
            int index = 0;
            foreach (var item in propertyInfos)
            {
                var returnTypeName = item.returnTypeSymbol.Display();
                var propertyName = item.name;
                if(returnTypeName == "void")
                {
                    if(expressionReturnType != null)
                    {
                        returnTypeName = $"System.Action<{expressionReturnType.Display()}>";
                    }
                    else
                    {
                        returnTypeName = "System.Action";
                    }        
                }
                else if (item.isMethod)
                {
                    propertyName = $"{item.name}()";
                }

                if (item.elementIndex && Owner.NeedGetter())
                {
                    bool isDictionary = item.decalareType.Display().Contains("ObservableDictionary");
                    if (item.elementKeyType == "int")
                    {
                        string access = item.constantElement ? item.name : $"()=>{item.name}";
                        WriteContent(intent + 1, $"path.Append(new DH.UIFramework.Paths.IntegerIndexedNode<{item.decalareType.Display()},{item.returnTypeSymbol.Display()}>({access},{isDictionary.ToString().ToLowerInvariant()}));");
                    }
                    else if (item.elementKeyType == "long")
                    {
                        string access = item.constantElement ? item.name : $"()=>{item.name}";

                        WriteContent(intent + 1, $"path.Append(new DH.UIFramework.Paths.LongIndexedNode<{item.decalareType.Display()},{item.returnTypeSymbol.Display()}>({access},{isDictionary.ToString().ToLowerInvariant()}));");
                    }
                    else
                    {
                        string access = item.constantElement ? item.name : $"()=>{item.name}";
                        WriteContent(intent + 1, $"path.Append(new DH.UIFramework.Paths.StringIndexedNode<{item.decalareType.Display()},{item.returnTypeSymbol.Display()}>({access}));");
                    }
                }
                else if(variableName != null && index == 0)
                {
                    var setter = Owner.NeedSetter() && (lastProperty == item) ? $"(vm,value)=>{variableName.Display()}.{item.name} = value" : "null";
                    var getter = Owner.NeedGetter() || (lastProperty != item) ? $"vm => {variableName.Display()}.{item.name}" : "null";
                    if (item.returnTypeSymbol.IsValueType)
                    {
                        WriteContent(intent + 1, $"path.Append(new {CompiledMemberNode}<object, {returnTypeName}>(\"{item.name}\", {setter}, {getter}));");
                    }
                    else
                    {
                        WriteContent(intent + 1, $"path.Append(new {CompiledMemberNode}<object, object>(\"{item.name}\", {setter}, {getter}));");
                    }
                }
                else
                {
                    if (item.returnTypeSymbol.IsValueType)
                    {
                        var setter = Owner.NeedSetter() && (lastProperty == item) && !item.isMethod ? $"(vm,value)=>(({sourceType.Display()})vm).{item.name} = value" : "null";
                        var getter = Owner.NeedGetter() || (lastProperty != item) ? $"vm => (({sourceType.Display()})vm).{propertyName}" : "null";
                        WriteContent(intent + 1, $"path.Append(new {CompiledMemberNode}<object, {returnTypeName}>(\"{item.name}\", {setter}, {getter}));");
                    }
                    else
                    {
                        var setter = Owner.NeedSetter() && (lastProperty == item) && !item.isMethod ? $"(vm,value)=>(({sourceType.Display()})vm).{item.name} = ({item.returnTypeSymbol})value" : "null";
                        var getter = Owner.NeedGetter() || (lastProperty != item) ? $"vm => (({sourceType.Display()})vm).{propertyName}" : "null";
                        WriteContent(intent + 1, $"path.Append(new {CompiledMemberNode}<object, object>(\"{item.name}\", {setter}, {getter}));");
                    }
                }
                sourceType = item.returnTypeSymbol;
                index++;
            }
        }

        private void EvaluatePropertyType(INamedTypeSymbol namedTypeSymbol)
        {
            DataBindingSyntaxReceiver receiver = Owner.receiver;
            GeneratorExecutionContext context = Owner.context;
            int index = 0;
            foreach (var expressions in memberAccesses)
            {
                var staticClass = variableNames[index];
                index++;
                var list = new List<PropertyInfo>();
                INamedTypeSymbol type = staticClass == null ? namedTypeSymbol : staticClass;
                foreach (var item in expressions)
                {
                    string name = null;
                    var decalareType = type;
                    if(item is MemberAccessExpressionSyntax member)
                    {
                        name = member.Name.ToFullString().Trim();
                        var lowerName =DataBindingCodeGenerator.LowerCaseCamelCase(name);
                        type = GetFieldType(type, name, lowerName, receiver, context, list);
                    }
                    else if(item is ElementAccessExpressionSyntax element && element.ArgumentList.Arguments.Count == 1)
                    {
                        var argument = element.ArgumentList.Arguments[0].Expression;
                        if(type.TypeArguments.Length > 1)
                        {
                            string keyType = type.TypeArguments[0].ToDisplayString();
                            type = type.TypeArguments[1] as INamedTypeSymbol;
                            list.Add(new PropertyInfo()
                            {
                                elementIndex = true,
                                name = argument.ToString(),
                                constantElement = argument is LiteralExpressionSyntax,
                                elementKeyType = keyType,
                                returnTypeSymbol = type,
                                decalareType = decalareType,
                            }); ;
                        }
                        else
                        {
                            type = type.TypeArguments[0] as INamedTypeSymbol;
                            list.Add(new PropertyInfo()
                            {
                                elementIndex = true,
                                name = argument.ToString(),
                                elementKeyType = "int",
                                constantElement = argument is LiteralExpressionSyntax,
                                returnTypeSymbol = type,
                                decalareType = decalareType,
                            });
                        }
                        continue;
                    }
                    else if(item is ThisExpressionSyntax thisExpression)
                    {
                        // do thing
                    }

                    if(type == null)
                    {
                        break;
                    }
                }

                if (type != null && memberAccesses.Count == 1 && type.IsGenericType && type.Name == "ObservableDictionary" && type.TypeArguments.Length == 2)
                {
                    collectionKeyType = type.TypeArguments[0] as INamedTypeSymbol;
                    collectionValueType = type.TypeArguments[1] as INamedTypeSymbol;
                }

                if(list.Count == 0)
                {
                    continue;
                }

                properties.Add(list);
            }
        }

        private INamedTypeSymbol GetSymbolReturnType(ISymbol symbol)
        {
            if(symbol == null)
            {
                return null;
            }

            if (symbol is IFieldSymbol fieldSymbol)
            {
                return fieldSymbol.Type as INamedTypeSymbol;
            }
            if (symbol is IPropertySymbol propertySymbol)
            {
                return propertySymbol.Type as INamedTypeSymbol;
            }
            if (symbol is IMethodSymbol methodSymbol)
            {
                return methodSymbol.ReturnType as INamedTypeSymbol;
            }

            return null;
        }

        private ISymbol GetFieldSymbol(INamedTypeSymbol namedType,string name,string lowerName)
        {
            while(namedType != null)
            {
                var fieldType = namedType.GetMembers().FirstOrDefault(x => x.Name == lowerName || x.Name == name);
                if(fieldType!= null)
                {
                    return fieldType;
                }

                namedType = namedType.BaseType;
            }
           
            return null;
        }

        private INamedTypeSymbol GetFieldType(INamedTypeSymbol namedTypeSymbol, string name, string lowerName, DataBindingSyntaxReceiver receiver, GeneratorExecutionContext context,List<PropertyInfo> properties)
        {
            try
            {
                if(namedTypeSymbol == null)
                {
                    return null;
                }

                var fieldType = GetFieldSymbol(namedTypeSymbol, name, lowerName);
                if (fieldType == null && name.EndsWith("Command"))
                {
                    if (receiver.CommandType == null)
                    {
                        receiver.CommandType = context.Compilation.GetTypeByMetadataName("DH.UIFramework.Commands.ICommand");
                    }

                    properties.Add(new PropertyInfo()
                    {
                        returnTypeSymbol = receiver.CommandType,
                        name = name,
                        isMethod = false,
                        decalareType = namedTypeSymbol,
                    });
                    return null;
                }

                var type = GetSymbolReturnType(fieldType);
                if (type != null)
                {
                    properties.Add(new PropertyInfo()
                    {
                        returnTypeSymbol = type,
                        name = name,
                        isMethod = fieldType is IMethodSymbol,
                        decalareType = namedTypeSymbol,
                    });
                    return type;
                }

                var attribute = namedTypeSymbol.GetAttributes().FirstOrDefault();
                if (attribute == null || attribute.AttributeClass?.Name != "ProtoWrapAttribute")
                {
                    return null;
                }

                var targetTypeName = attribute.ConstructorArguments[0].Value.ToString();
                var protoType = context.Compilation.GetTypeByMetadataName(targetTypeName);
                if (protoType == null)
                {
                    receiver.logMsg.AppendLine($"Invalid proto type targetTypeName");
                    return null;
                }
                fieldType = GetFieldSymbol(protoType, name, name);
                type = GetSymbolReturnType(fieldType);
                if (type != null)
                {
                    properties.Add(new PropertyInfo()
                    {
                        returnTypeSymbol = type,
                        name = name,
                        isMethod = fieldType is IMethodSymbol,
                        decalareType = namedTypeSymbol,
                    });
                }
                return type;
            }
            catch(Exception e)
            {
                receiver.logMsg.AppendLine($"{memberAccess.ToFullString()} {invocation.ToFullString()} \r\n{namedTypeSymbol} Name {name}\r\n{e}");
                return null;
            }
        }
    }

    public class BindCommandPara : BindItemData
    {
        public string param;
        public bool isFunc;
        public ITypeSymbol returnType;
        private const string NameSpace = "DH.UIFramework.Parameters";

        public override void Evaluate()
        {
            EvaluateLambdaBody(invocation);
        }

        public override void GenerateCode()
        {
            if (isFunc)
            {
                WriteContent(1, $"builder.Description.CommandParameter = (System.Func<{returnType}>)({param});");
                WriteContent(1, $"builder.Description.Converter = new {NameSpace}.ParameterWrapConverter(new {NameSpace}.ExpressionCommandParameter<{returnType}>({param}));");
            }
            else
            {
                WriteContent(1, $"builder.Description.CommandParameter = {param};");
                WriteContent(1, $"builder.Description.Converter = new {NameSpace}.ParameterWrapConverter(new {NameSpace}.ConstantCommandParameter({param}));");
            }
        }

        protected void EvaluateLambdaBody(InvocationExpressionSyntax invocation)
        {
            var methodSymbol = (Owner.semantic.GetSymbolInfo(invocation).Symbol as IMethodSymbol);
            if (methodSymbol?.IsGenericMethod ?? false)
            {
                returnType = methodSymbol.TypeArguments[0];
                isFunc = true;
            }
            
            param = invocation.ArgumentList.Arguments.ToFullString();
        }
    }

    public class BindConverterData : BindItemData
    {
        public string param;

        public override void Evaluate()
        {
            EvaluateLambdaBody(invocation);
        }

        public override void GenerateCode()
        {
            WriteContent(1, $"builder.Description.Converter = {param};");
        }

        protected void EvaluateLambdaBody(InvocationExpressionSyntax invocation)
        {
            var arguments = invocation.ArgumentList;
            param = arguments.ToFullString();
        }
    }

    public class BindForData : BindItemData
    {
        public INamedTypeSymbol typeSymbol;
        public bool isUnityEvent;
        public ImmutableArray<ITypeSymbol> typeArguments;
        public ImmutableArray<ITypeSymbol> triggerTypeArguments;
        public string targetName;
        public string triggerName;

        private const string UnityEventTypeName = "UnityEvent";
        private const string TargetNamespace = "DH.UIFramework.Proxy.Targets";

        private bool IsDerivedFrom(INamedTypeSymbol type, string targetType,out INamedTypeSymbol baseType)
        {
            baseType = null;
            while (type != null)
            {
                if (type.Name == targetType)
                {
                    baseType = type;
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }

        private bool IsDerivedFromFullName(INamedTypeSymbol type, string targetType, out INamedTypeSymbol baseType)
        {
            baseType = null;
            while (type != null)
            {
                if (type.ToDisplayString() == targetType)
                {
                    baseType = type;
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }

        private INamedTypeSymbol GetSymbolReturnType(ISymbol symbol)
        {
            if(symbol is IFieldSymbol fieldSymbol)
            {
                return fieldSymbol.Type as INamedTypeSymbol;
            }

            if (symbol is IMethodSymbol methodSymbol)
            {
                return methodSymbol.ReturnType as INamedTypeSymbol;
            }

            if (symbol is IPropertySymbol propertySymbol)
            {
                return propertySymbol.Type as INamedTypeSymbol;
            }

            return null;
        }

        public override void Evaluate()
        {
            List<List<ExpressionSyntax>> memberAccesses = new List<List<ExpressionSyntax>>();
            EvaluateLambdaArgument(invocation, memberAccesses, null,false,null);
            if (memberAccesses.Count == 0)
            {
                return;
            }

            var symbol = Owner.semantic.GetSymbolInfo(invocation.Expression).Symbol as IMethodSymbol;
            if (symbol != null && symbol.IsGenericMethod && symbol.TypeArguments.Length > 0)
            {
                typeSymbol = symbol.TypeArguments[0] as INamedTypeSymbol;
            }

            if (IsDerivedFrom(typeSymbol, UnityEventTypeName,out var baseType))
            {
                isUnityEvent = true;
                if (baseType.IsGenericType)
                {
                    typeArguments = baseType.TypeArguments;
                }
            }
            targetName = GetMemberName(memberAccesses[0]);

            if (invocation.ArgumentList.Arguments.Count <= 1)
            {
                return;
            }

            triggerName = GetMemberName(memberAccesses[1]);
            var triggerSymbol = Owner.semantic.GetSymbolInfo(invocation.ArgumentList.Arguments[1]).Symbol as INamedTypeSymbol;
            if(triggerSymbol == null)
            {
                return;
            }
            if (IsDerivedFrom(triggerSymbol, UnityEventTypeName, out baseType) && baseType.IsGenericType)
            {
                triggerTypeArguments = baseType.TypeArguments;
            }
        }

        public override void GenerateCode()
        {
            if(Owner.targetType == null || targetName == null)
            {
                return;
            }

            var propertyInfo = Owner.receiver.TryAddProperty(Owner.targetType, targetName,typeSymbol);
            propertyInfo.GetFuncProperty(Owner.classSymbol,out var getter, out var setter);
            if (isUnityEvent)
            {
                setter = "null";
                if(typeArguments == null || typeArguments.Length < 1)
                {
                    WriteContent(1, $"var targetDesc = new {TargetNamespace}.SimpleEventTargetDescription<{Owner.targetType.Display()},{typeSymbol.Display()}>(\"{targetName}\",({Owner.targetType.Display()})builder.Target,{getter},{setter});");
                    WriteContent(1, $"builder.Description.Target = targetDesc;");
                }
                else
                {
                    WriteContent(1, $"var targetDesc = new {TargetNamespace}.UnityEventTargetDescription<{Owner.targetType.Display()},{string.Join(",",typeArguments)}>(\"{targetName}\",({Owner.targetType.Display()})builder.Target,{getter},{setter});");
                    WriteContent(1, $"builder.Description.Target = targetDesc;");
                }
                return;
            }

            if (!string.IsNullOrEmpty(triggerName))
            {
                string trigger = $"t=>t.{triggerName}";
                WriteContent(1, $"var targetDesc = new {TargetNamespace}.TargetDescription<{Owner.targetType.Display()},{typeSymbol.Display()}>(\"{targetName}\",({Owner.targetType.Display()})builder.Target,{getter},{setter},{trigger});");
                WriteContent(1, $"builder.Description.Target = targetDesc;");
            }
            else
            {
                WriteContent(1, $"var targetDesc = new {TargetNamespace}.TargetDescription<{Owner.targetType.Display()},{typeSymbol.Display()}>(\"{targetName}\",({Owner.targetType.Display()})builder.Target,{getter},{setter},null);");
                WriteContent(1, $"builder.Description.Target = targetDesc;");
            }
        }
    }

    public class ForSyntaxData : BindingData
    {
        public SyntaxNode syntaxNode;

        public override void Init()
        {
            
        }

        public override void GenerateCode()
        {
            WriteContent(-startIntent, syntaxNode?.ToFullString(),false);
        }
    }

    public class MultipleBindingData : BindingData
    {
        public List<SyntaxNode> forSyntaxNodes = new List<SyntaxNode>();
        public List<BindingData> blockSyntaxNodes = new List<BindingData>();

        public override void Init()
        {
            foreach(var item in blockSyntaxNodes)
            {
                item.Buffer = Buffer;
                item.Writer = Writer;
                item.receiver = receiver;
                item.context = context;
                item.startIntent = startIntent + 1;
                item.Init();
            }
        }

        public override void GenerateCode()
        {
            WriteContent(0, $"for({forSyntaxNodes[0]};{forSyntaxNodes[1]};{forSyntaxNodes[2]})");
            WriteContent(0, "{");
            foreach (var item in blockSyntaxNodes)
            {
                item.GenerateCode();
            }
            WriteContent(0, "}");
        }
    }

    public class InvocationBindingData : BindingData
    {
        public string invocationExpression;

        public override void GenerateCode()
        {
            WriteContent(0,$"{invocationExpression};");
        }
    }

    public class BindingData
    {
        public INamedTypeSymbol targetType;
        public INamedTypeSymbol sourceType;
        public INamedTypeSymbol classSymbol;
        public BindTargetData bindTarget;
        public BindForData bindFor;
        public BindToData bindTo;
        public string bindMode = "OneWay";
        public BindConverterData bindConverter;
        public BindCommandPara bindCmdPara;

        public List<BindItemData> bindItemDatas = new List<BindItemData>();
        public Action<int, string, StringBuilder, bool> Writer;
        public int startIntent;
        public StringBuilder Buffer;
        public DataBindingSyntaxReceiver receiver;
        public GeneratorExecutionContext context;
        public SemanticModel semantic;

        public bool NeedSetter()
        {
            return bindMode == "TwoWay" || bindMode == "OneWayToSource";
        }

        public bool NeedGetter()
        {
            return bindMode == "TwoWay" || bindMode == "OneTime" || bindMode == "OneWay";
        }

        public virtual void Init()
        {
            bindItemDatas.Clear();
            bindItemDatas.Add(bindTarget);
            bindItemDatas.Add(bindFor);
            bindItemDatas.Add(bindTo);
            bindItemDatas.Add(bindConverter);
            bindItemDatas.Add(bindCmdPara);
            foreach(var item in bindItemDatas)
            {
                if(item == null)
                {
                    continue;
                }
                item.Owner = this;
            }
        }

        public virtual void GenerateCode()
        {
            if(bindTarget == null)
            {
                return;
            }

            var intent = startIntent;
            Writer(intent, $"if({bindTarget.targetAccess} != null)",Buffer, true);
            Writer(intent, "{",Buffer, true);
            foreach(var item in bindItemDatas)
            {
                if (item == null)
                {
                    continue;
                }

                item.GenerateCode();
            }
            if (string.IsNullOrEmpty(bindMode))
            {
                WriteContent(1, "builder.Description.Mode = DH.UIFramework.BindingMode.OneWay;");
            }
            else
            {
                WriteContent(1, $"builder.Description.Mode = DH.UIFramework.BindingMode.{bindMode};");
            }

            Writer(intent, "}",Buffer,true);
        }

        public void WriteContent(int intent,string content,bool newLine = true)
        {
            Writer(startIntent + intent, content, Buffer, newLine);
        }
    }

    public class BindItemData
    {
        public MemberAccessExpressionSyntax memberAccess;
        public InvocationExpressionSyntax invocation;
        public BindingData Owner;

        public virtual void GenerateCode()
        {

        }

        public virtual void Evaluate()
        {

        }

        protected void WriteContent(int intent, string content, bool newLine = true)
        {
            Owner.WriteContent(intent, content, newLine);
        }

        protected string EvaluateLambdaArgument(InvocationExpressionSyntax invocationSyntax,
            List<List<ExpressionSyntax>> memberAccessExpressions, 
            List<string> expressionBody,
            bool expressionMode,
            List<INamedTypeSymbol> variableNames)
        {
            string vmName = null;
            var arguments = invocationSyntax.ArgumentList;
            foreach (var arg in arguments.Arguments)
            {
                var lambdaExpression = arg.Expression as SimpleLambdaExpressionSyntax;
                if (lambdaExpression != null)
                {
                    vmName = lambdaExpression.Parameter.ToString();
                    VisitLambdaExpressionMemberAccess(lambdaExpression.ChildNodes(), memberAccessExpressions, variableNames);
                    if (expressionBody != null)
                    {
                        expressionBody.Add(lambdaExpression.ExpressionBody.ToFullString());
                    }
                }
            }

            return vmName;
        }

        protected string GetMemberName(List<ExpressionSyntax> expressions)
        {
            StringBuilder stringBuilder= new StringBuilder();
            int count = expressions.Count;
            for (int index = 0; index < count; index++)
            {
                var item = expressions[index];
                if (item is MemberAccessExpressionSyntax member)
                {
                    if (index != 0)
                    {
                        stringBuilder.Append(".");
                    }

                    stringBuilder.Append(member.Name.ToFullString().Trim());
                }
                else if (item is ElementAccessExpressionSyntax element)
                {
                    stringBuilder.Append(element.ArgumentList.ToFullString());
                    stringBuilder.Append(element.ArgumentList.Arguments[0].ToString());
                }
                else if (item is ThisExpressionSyntax thisExpression)
                {
                    stringBuilder.Append("this");
                }
            }

            return stringBuilder.ToString();
        }

        protected bool ValidExpression(ExpressionSyntax syntax, out INamedTypeSymbol staticClassName)
        {
            staticClassName = null;
            if (syntax is MemberAccessExpressionSyntax member)
            {
                return true;
            }
            else if (syntax is ElementAccessExpressionSyntax element)
            {
                return true;
            }
            else if (syntax is ThisExpressionSyntax thisExpression)
            {
                return true;
            }
            else if (syntax is IdentifierNameSyntax identifierName)
            {
                staticClassName = Owner.semantic.GetSymbolInfo(identifierName).Symbol as INamedTypeSymbol;
                return false;
            }
            else
            {
                return false;
            }
        }

        protected ExpressionSyntax NextExpression(ExpressionSyntax syntax)
        {
            if (syntax is MemberAccessExpressionSyntax member)
            {
                if(member.ChildNodes().Any(x=>x is PredefinedTypeSyntax))
                {
                    return null;
                }

                return member.Expression;
            }
            else if (syntax is ElementAccessExpressionSyntax element)
            {
                return element.Expression;
            }
            else if (syntax is BinaryExpressionSyntax binaryExpressionSyntax)
            {
                if (binaryExpressionSyntax.Left is MemberAccessExpressionSyntax)
                {
                    return binaryExpressionSyntax.Left;
                }

                if (binaryExpressionSyntax.Right is MemberAccessExpressionSyntax)
                {
                    return binaryExpressionSyntax.Right;
                }

                if (binaryExpressionSyntax.Left is InvocationExpressionSyntax invocation)
                {
                    return invocation.Expression;
                }

                if (binaryExpressionSyntax.Right is InvocationExpressionSyntax rhsInvocation)
                {
                    return rhsInvocation.Expression;
                }

                return null;
            }
            else if (syntax is ConditionalAccessExpressionSyntax conditionalAccess)
            {
                if (conditionalAccess.Expression is MemberAccessExpressionSyntax)
                {
                    return conditionalAccess.Expression;
                }

                if (conditionalAccess.WhenNotNull is MemberAccessExpressionSyntax)
                {
                    return conditionalAccess.WhenNotNull;
                }

                if (conditionalAccess.Expression is InvocationExpressionSyntax invocation)
                {
                    return invocation.Expression;
                }

                if (conditionalAccess.WhenNotNull is InvocationExpressionSyntax notnullInvocation)
                {
                    return notnullInvocation.Expression;
                }

                return null;
            }
            else if (syntax is PrefixUnaryExpressionSyntax unaryExpression)
            {
                if (unaryExpression.Operand is MemberAccessExpressionSyntax)
                {
                    return unaryExpression.Operand;
                }

                if (unaryExpression.Operand is InvocationExpressionSyntax invocation)
                {
                    return invocation.Expression;
                }

                return null;
            }
            else
            {
                return null;
            }
        }

        protected void VisitLambdaExpressionMemberAccess(IEnumerable<SyntaxNode> syntaxNodes, List<List<ExpressionSyntax>> memberAccessExpressions,List<INamedTypeSymbol> variableNames)
        {
            foreach (var item in syntaxNodes)
            {
                var childNodes = item.ChildNodesAndTokens();
                if (item is MemberAccessExpressionSyntax || item is ElementAccessExpressionSyntax)
                {
                    List<ExpressionSyntax> members = new List<ExpressionSyntax>();
                    ExpressionSyntax expression = item as ExpressionSyntax;
                    INamedTypeSymbol variableName = null;
                    while (expression != null)
                    {
                        members.Insert(0, expression);
                        expression = NextExpression(expression);
                        if (!ValidExpression(expression, out variableName))
                        {
                            break;
                        }
                    }
                   
                    if(variableNames != null)
                    {
                        variableNames.Add(variableName);
                    }

                    memberAccessExpressions.Add(members);
                }
                else if (item is BinaryExpressionSyntax binaryExpressionSyntax)
                {
                    VisitLambdaExpressionMemberAccess(new List<SyntaxNode>() { binaryExpressionSyntax.Left,binaryExpressionSyntax.Right}, memberAccessExpressions, variableNames);
                }
                else if (item is ConditionalAccessExpressionSyntax conditionalAccess)
                {
                    VisitLambdaExpressionMemberAccess(new List<SyntaxNode>() { conditionalAccess.Expression, conditionalAccess.WhenNotNull}, memberAccessExpressions, variableNames);
                }
                else if (item is ConditionalExpressionSyntax conditional)
                {
                    VisitLambdaExpressionMemberAccess(new List<SyntaxNode>() { conditional.Condition, conditional.WhenTrue,conditional.WhenFalse}, memberAccessExpressions, variableNames);
                }
                else if(item is InvocationExpressionSyntax invocation && invocation.ArgumentList.Arguments.Count > 0)
                {
                    VisitLambdaExpressionMemberAccess(invocation.ArgumentList.Arguments, memberAccessExpressions,variableNames);
                }
                else if (childNodes == null)
                {
                    continue;
                }
                else if (childNodes.Count == 0)
                {
                    continue;
                }
                else
                {
                    VisitLambdaExpressionMemberAccess(item.ChildNodes(), memberAccessExpressions,variableNames);
                }
            }
        }
    }
}
