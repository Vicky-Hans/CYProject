using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DataBindingGenerator
{
    public partial class DataBindingCodeGenerator
    {
        private void GenerateCollectionBindingCode(DataBindingSyntaxReceiver receiver, GeneratorExecutionContext context)
        {
            foreach (var item in receiver.CollectionItems)
            {
                var classSource = ProcessClass(item.Key, item.Value, receiver);
                context.AddSource($"{item.Key.Name}_BindingCode_g.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }

        private string ProcessClass(INamedTypeSymbol classSymbol, List<CollectionItem> collectionItem, DataBindingSyntaxReceiver receiver)
        {
            var stringBuilder = receiver.stringBuilder;
            stringBuilder.Length = 0;
            stringBuilder.AppendLine($"// {receiver.CollectionItems.Count}");
            stringBuilder.AppendLine($"// Update Time:{DateTime.Now}");
            stringBuilder.AppendLine("using System.Collections.Specialized;");
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using DH.Data;");
            stringBuilder.AppendLine("using UnityEngine.Pool;");
            stringBuilder.AppendLine($"namespace {classSymbol.ToDisplayString().Replace($".{classSymbol.Name}", null)}");
            stringBuilder.AppendLine("{");
            WriteContent(1, $"public partial class {classSymbol.Name}", stringBuilder);
            WriteContent(1, "{", stringBuilder);
            GenerateInitCode(classSymbol, collectionItem, 2, stringBuilder);
            GenerateCollectionChangedCode(classSymbol, collectionItem, 2, stringBuilder);
            GenerateDisposeCode(classSymbol, collectionItem, 2, stringBuilder);
            WriteContent(1, "}", stringBuilder);
            stringBuilder.AppendLine("}");
            return stringBuilder.ToString();
        }

        private void GenerateDisposeCode(INamedTypeSymbol classSymbol, List<CollectionItem> collectionItems, int intent, StringBuilder stringBuilder)
        {
            WriteContent(intent, "protected void DisposeCollection()", stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            foreach (var item in collectionItems)
            {
                GenerateDisposeCode(item, intent + 1, stringBuilder);
            }
            WriteContent(intent, "}", stringBuilder);
        }

        private void GenerateCollectionChangedCode(INamedTypeSymbol classSymbol, List<CollectionItem> collectionItems, int intent, StringBuilder stringBuilder)
        {
            foreach (var item in collectionItems)
            {
                if (item.sourceIsDictionary)
                {
                    GenerateDic2DicChangedCode(item, intent, stringBuilder);
                    GenerateDic2DicRemoveOldItemsCode(item, intent, stringBuilder);
                    GenerateDic2DicAddNewItemsCode(item, intent, stringBuilder);
                }
                else if (!item.sourceIsDictionary && !item.targetIsDictionary)
                {
                    GenerateCollectionChangedCode(item, intent, stringBuilder);
                }           
            }
        }

        private void GenerateInitCode(INamedTypeSymbol classSymbol, List<CollectionItem> collectionItems, int intent, StringBuilder stringBuilder)
        {
            WriteContent(intent, "private void InitCollection()", stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            foreach (var item in collectionItems)
            {
                GenerateInitCode(item, intent + 1, stringBuilder);
            }
            WriteContent(intent, "}", stringBuilder);
        }

        private void GenerateCreateDictionaryItemCode(CollectionItem collectionItem, int intent, StringBuilder stringBuilder)
        {
            var memberName = collectionItem.targetAccess.Identifier.Text;
            var keyType = collectionItem.keyType.Display();
            var sourceTypeName = collectionItem.sourceAccessTypeSymbol.Display();
            if (collectionItem.creatorMemberAccessor != null)
            {
                WriteContent(intent, $"var itemData = {collectionItem.creatorMemberAccessor}(({keyType})item.Key,({sourceTypeName})item.Value);", stringBuilder);
            }
            else
            {
                WriteContent(intent, $"var itemData = {collectionItem.creatorAccessor.ToFullString()}(({keyType})item.Key,({sourceTypeName})item.Value);", stringBuilder);
            }
        }

        private void GenerateCreateItemCode(CollectionItem collectionItem, int intent, StringBuilder stringBuilder)
        {
            var memberName = collectionItem.targetAccess.Identifier.Text;
            var sourceTypeName = collectionItem.sourceAccessTypeSymbol.Display();
            if(collectionItem.creatorMemberAccessor != null)
            {
                WriteContent(intent, $"var itemData = {collectionItem.creatorMemberAccessor}(({sourceTypeName})item);", stringBuilder);
            }
            else
            {
                WriteContent(intent, $"var itemData = {collectionItem.creatorAccessor.ToFullString()}(({sourceTypeName})item);", stringBuilder);
            }
        }

        private void GenerateInitCode(CollectionItem collectionItem, int intent, StringBuilder stringBuilder)
        {
            var memberName = collectionItem.targetAccess.Identifier.Text;
            WriteContent(intent, $"{collectionItem.sourceAccess}.CollectionChanged += {collectionItem.funcionPrefix}OnCollectionChanged;", stringBuilder);
            if(collectionItem.sourceIsDictionary)
            {
                CreatDictionary2CollectionInitCode(collectionItem, intent, stringBuilder, memberName);
            }
            else if(!collectionItem.sourceIsDictionary && !collectionItem.targetIsDictionary)
            {
                CreateList2ListInitCode(collectionItem, intent, stringBuilder, memberName);
            }
        }

        private void CreatDictionary2CollectionInitCode(CollectionItem collectionItem, int intent, StringBuilder stringBuilder, string memberName)
        {
            WriteContent(intent, $"foreach (var item in {collectionItem.sourceAccess})", stringBuilder);
            WriteContent(intent, "{", stringBuilder);  
            if (collectionItem.customAddOrRemove)
            {
                WriteContent(intent + 1, $"{collectionItem.CreatorAccessorName}(item.Key,item.Value);", stringBuilder);
            }
            else
            {
                WriteContent(intent + 1, "var key = item.Key;", stringBuilder);
                GenerateCreateDictionaryItemCode(collectionItem, intent + 1, stringBuilder);
                WriteContent(intent + 1, $"if({memberName}.ContainsKey(key) || itemData == null) continue;", stringBuilder);
                WriteContent(intent + 1, $"{memberName}.Add(key,itemData);", stringBuilder);
            }
            
            WriteContent(intent, "}", stringBuilder);
        }

        private void CreateList2ListInitCode(CollectionItem collectionItem, int intent, StringBuilder stringBuilder,string memberName)
        {
            WriteContent(intent, $"foreach (var item in {collectionItem.sourceAccess})", stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            if (collectionItem.customAddOrRemove)
            {
                WriteContent(intent + 1, $"{collectionItem.CreatorAccessorName}(item);", stringBuilder);
            }
            else
            {
                GenerateCreateItemCode(collectionItem, intent + 1, stringBuilder);
                WriteContent(intent + 1, $"{memberName}.Add(itemData);", stringBuilder);
            }
            WriteContent(intent, "}", stringBuilder);
        }


        private void GenerateDisposeCode(CollectionItem collectionItem, int intent, StringBuilder stringBuilder)
        {
            WriteContent(intent, $"if({collectionItem.sourceAccess} != null)", stringBuilder);
            WriteContent(intent+1, $"{collectionItem.sourceAccess}.CollectionChanged -= {collectionItem.funcionPrefix}OnCollectionChanged;", stringBuilder);
        }

        private void GenerateDic2DicAddNewItemsCode(CollectionItem collectionItem, int intent, StringBuilder stringBuilder)
        {
            var keyType = collectionItem.keyType;
            var valueType = collectionItem.sourceAccessTypeSymbol;
            var memberName = collectionItem.targetAccess.Identifier.Text;
            WriteContent(intent, $"private void {collectionItem.funcionPrefix}AddNewItems(System.Collections.IList newItems)", stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            WriteContent(intent+1, $"foreach (var listItem in newItems)", stringBuilder);
            WriteContent(intent+1, "{", stringBuilder);
            WriteContent(intent+2, $"var item = (System.Collections.Generic.KeyValuePair<{keyType.Display()}, {valueType.Display()}>)listItem;", stringBuilder);
            if (collectionItem.customAddOrRemove)
            {
                WriteContent(intent + 2, $"{collectionItem.CreatorAccessorName}(item.Key,item.Value);", stringBuilder);
            }
            else
            {
                WriteContent(intent + 2, "var key = item.Key;", stringBuilder);
                GenerateCreateDictionaryItemCode(collectionItem, intent + 2, stringBuilder);
                WriteContent(intent + 2, $"if({memberName}.ContainsKey(key) || itemData == null) continue;", stringBuilder);
                WriteContent(intent + 2, $"{memberName}.Add(key,itemData);", stringBuilder);
            }
          
            WriteContent(intent+1, "}", stringBuilder);
            WriteContent(intent, "}", stringBuilder);
        }

        private void GenerateDic2DicRemoveOldItemsCode(CollectionItem collectionItem, int intent, StringBuilder stringBuilder)
        {
            var keyType = collectionItem.keyType;
            var valueType = collectionItem.sourceAccessTypeSymbol;
            var memberName = collectionItem.targetAccess.Identifier.Text;
            WriteContent(intent, $"private void {collectionItem.funcionPrefix}RemoveOldItems(System.Collections.IList oldItems)", stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            WriteContent(intent + 1, $"foreach (var listItem in oldItems)", stringBuilder);
            WriteContent(intent + 1, "{", stringBuilder);
            WriteContent(intent + 2, $"var item = (System.Collections.Generic.KeyValuePair<{keyType.Display()}, {valueType.Display()}>)listItem;", stringBuilder);
            if (collectionItem.customAddOrRemove)
            {
                WriteContent(intent + 2, $"{collectionItem.RemoveAccessorName}(item.Key,item.Value);", stringBuilder);
            }
            else
            {
                WriteContent(intent + 2, $"{memberName}.Remove(item.Key);", stringBuilder);
            }
            WriteContent(intent + 1, "}", stringBuilder);
            WriteContent(intent, "}", stringBuilder);
        }

        private void GenerateDic2DicChangedCode(CollectionItem collectionItem, int intent, StringBuilder stringBuilder)
        {
            var memberName = collectionItem.targetAccess.Identifier.Text;
            WriteContent(intent, $"private void {collectionItem.funcionPrefix}OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)", stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            WriteContent(intent + 1, "var newItems = eventArgs.NewItems;", stringBuilder);
            WriteContent(intent + 1, "var oldItems = eventArgs.OldItems;", stringBuilder);
            WriteContent(intent + 1, "switch (eventArgs.Action)", stringBuilder);
            WriteContent(intent + 1, "{", stringBuilder);

            WriteContent(intent + 2, "case NotifyCollectionChangedAction.Add:", stringBuilder);
            WriteContent(intent + 3, $"{collectionItem.funcionPrefix}AddNewItems(newItems);", stringBuilder);
            WriteContent(intent + 3, "break;", stringBuilder);
            WriteContent(0, "", stringBuilder);

            WriteContent(intent + 2, "case NotifyCollectionChangedAction.Remove:", stringBuilder);
            WriteContent(intent + 3, $"{collectionItem.funcionPrefix}RemoveOldItems(oldItems);", stringBuilder);
            WriteContent(intent + 3, "break;", stringBuilder);
            WriteContent(0, "", stringBuilder);

            WriteContent(intent + 2, "case NotifyCollectionChangedAction.Replace:", stringBuilder);
            WriteContent(intent + 3, $"{collectionItem.funcionPrefix}RemoveOldItems(oldItems);", stringBuilder);
            WriteContent(intent + 3, $"{collectionItem.funcionPrefix}AddNewItems(newItems);", stringBuilder);
            WriteContent(intent + 3, "break;", stringBuilder);
            WriteContent(0, "", stringBuilder);

            WriteContent(intent + 2, "case NotifyCollectionChangedAction.Reset:", stringBuilder);
            if (!collectionItem.customAddOrRemove)
            {
                WriteContent(intent + 3, $"{memberName}.Clear();", stringBuilder);
            }
            else
            {
                WriteContent(intent + 3, $"{collectionItem.ClearAccessorName}();", stringBuilder);
            }
            WriteContent(intent + 2, "break;", stringBuilder);
            WriteContent(0, "", stringBuilder);

            WriteContent(intent + 1, "}", stringBuilder);
            WriteContent(intent, "}", stringBuilder);
        }

        private void GenerateCustomListChanged(CollectionItem collectionItem, int intent, StringBuilder stringBuilder,bool onlyAdd)
        {
            WriteContent(intent + 3, $"foreach (var item in eventArgs.NewItems)", stringBuilder);
            WriteContent(intent + 3, "{", stringBuilder);
            var sourceTypeName = collectionItem.sourceAccessTypeSymbol.Display();
            WriteContent(intent + 4, $"{collectionItem.CreatorAccessorName}(({sourceTypeName})item);", stringBuilder);
            WriteContent(intent + 3, "}", stringBuilder);
            if (!onlyAdd)
            {
                WriteContent(intent + 3, $"foreach (var item in eventArgs.OldItems)", stringBuilder);
                WriteContent(intent + 3, "{", stringBuilder);
                WriteContent(intent + 4, $"{collectionItem.RemoveAccessorName}(({sourceTypeName})item);", stringBuilder);
                WriteContent(intent + 3, "}", stringBuilder);
            }
        }

        private void GenerateCollectionChangedCode(CollectionItem collectionItem, int intent, StringBuilder stringBuilder)
        {
            var memberName = collectionItem.targetAccess.Identifier.Text;
            WriteContent(intent, $"private void {collectionItem.funcionPrefix}OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)", stringBuilder);
            WriteContent(intent, "{", stringBuilder);
            WriteContent(intent + 1, "switch (eventArgs.Action)", stringBuilder);
            WriteContent(intent + 1, "{", stringBuilder);

            WriteContent(intent + 2, "case NotifyCollectionChangedAction.Add:", stringBuilder);
            WriteContent(intent + 2, "{", stringBuilder);
            if (collectionItem.customAddOrRemove)
            {
                GenerateCustomListChanged(collectionItem,intent, stringBuilder,true);
            }
            else
            {
                WriteContent(intent + 3, $"var tmpList = ListPool<{collectionItem.targetAccessTypeSymbol.Display()}>.Get();", stringBuilder);
                WriteContent(intent + 3, $"foreach (var item in eventArgs.NewItems)", stringBuilder);
                WriteContent(intent + 3, "{", stringBuilder);
                GenerateCreateItemCode(collectionItem, intent + 4, stringBuilder);
                WriteContent(intent + 4, $"tmpList.Add(itemData);", stringBuilder);
                WriteContent(intent + 3, "}", stringBuilder);
                WriteContent(intent + 3, $"{memberName}.InsertRange(eventArgs.NewStartingIndex, tmpList);", stringBuilder);
                WriteContent(intent + 3, $"ListPool<{collectionItem.targetAccessTypeSymbol.Display()}>.Release(tmpList);", stringBuilder);
            }
            WriteContent(intent + 2, "}", stringBuilder);
            WriteContent(intent + 3, "break;", stringBuilder);
            WriteContent(0, "", stringBuilder);

            WriteContent(intent + 2, "case NotifyCollectionChangedAction.Remove:", stringBuilder);
            if (collectionItem.customAddOrRemove)
            {
                GenerateCustomListChanged(collectionItem, intent, stringBuilder,false);
            }
            else
            {
                WriteContent(intent + 3, $"{memberName}.RemoveAt(eventArgs.OldStartingIndex);", stringBuilder);
            }
           
            WriteContent(intent + 3, "break;", stringBuilder);
            WriteContent(0, "", stringBuilder);

            WriteContent(intent + 2, "case NotifyCollectionChangedAction.Replace:", stringBuilder);
            WriteContent(intent + 2, "{", stringBuilder);
            if (!collectionItem.customAddOrRemove)
            {
                WriteContent(intent + 3, $"var item = eventArgs.NewItems[0];", stringBuilder);
                GenerateCreateItemCode(collectionItem, intent + 3, stringBuilder);
                WriteContent(intent + 3, $"{memberName}[eventArgs.OldStartingIndex] = itemData;", stringBuilder);
            }
            else
            {
                GenerateCustomListChanged(collectionItem, intent, stringBuilder, false);
            }
           
            WriteContent(intent + 2, "}", stringBuilder);
            WriteContent(intent + 3, "break;", stringBuilder);
            WriteContent(0, "", stringBuilder);

            WriteContent(intent + 2, "case NotifyCollectionChangedAction.Reset:", stringBuilder);
            if (!collectionItem.customAddOrRemove)
            {
                WriteContent(intent + 3, $"{memberName}.Clear();", stringBuilder);
            }
            else
            {
                WriteContent(intent + 3, $"{collectionItem.ClearAccessorName}();", stringBuilder);
            }
            WriteContent(intent + 2, "break;", stringBuilder);
            WriteContent(0, "", stringBuilder);

            WriteContent(intent + 2, "case NotifyCollectionChangedAction.Move:", stringBuilder);
            if (!collectionItem.customAddOrRemove)
            {
                WriteContent(intent + 3, $"{memberName}.Move(eventArgs.OldStartingIndex, eventArgs.NewStartingIndex);", stringBuilder);
            }
            else
            {
                GenerateCustomListChanged(collectionItem, intent, stringBuilder, false);
            }
            WriteContent(intent + 3, "break;", stringBuilder);
            WriteContent(0, "", stringBuilder);

            WriteContent(intent + 1, "}", stringBuilder);
            WriteContent(intent, "}", stringBuilder);
        }
    }

    public class CollectionItem
    {
        public MemberAccessExpressionSyntax sourceAccess;
        public INamedTypeSymbol sourceAccessTypeSymbol;
        public bool sourceIsDictionary;
        public IdentifierNameSyntax targetAccess;
        public INamedTypeSymbol targetAccessTypeSymbol;
        public bool targetIsDictionary;
        public INamedTypeSymbol keyType;
        public IdentifierNameSyntax creatorAccessor;
        public MemberAccessExpressionSyntax creatorMemberAccessor;

        public IdentifierNameSyntax removeAccessor;
        public MemberAccessExpressionSyntax removeMemberAccessor;

        public IdentifierNameSyntax clearAccessor;
        public MemberAccessExpressionSyntax clearMemberAccessor;

        public string CreatorAccessorName => creatorAccessor?.ToString() ?? creatorMemberAccessor.ToFullString();
        public string ClearAccessorName => clearAccessor?.ToString() ?? clearMemberAccessor.ToFullString();
        public string RemoveAccessorName => removeAccessor?.ToString() ?? removeMemberAccessor.ToFullString();

        public bool customAddOrRemove;
        public string funcionPrefix;
    }

    public partial class DataBindingSyntaxReceiver
    {
        public Dictionary<INamedTypeSymbol, List<CollectionItem>> CollectionItems = new Dictionary<INamedTypeSymbol, List<CollectionItem>>(SymbolEqualityComparer.Default);

        private bool IsDictionary(INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol.Name == "Dictionary" || namedTypeSymbol.Name == "ObservableDictionary";
        }

        private INamedTypeSymbol GetDataType(INamedTypeSymbol namedTypeSymbol,out bool isDictionary,out INamedTypeSymbol keyType)
        {
            if(IsDictionary(namedTypeSymbol))
            {
                isDictionary = true;
                keyType = namedTypeSymbol.TypeArguments[0] as INamedTypeSymbol;
                return namedTypeSymbol.TypeArguments[1] as INamedTypeSymbol;
            }
            else
            {
                isDictionary = false;
                keyType = null;
                return namedTypeSymbol.TypeArguments[0] as INamedTypeSymbol;
            }
        }

        private void GetBindingSourceAndTargetType(CollectionItem collectionItem, SemanticModel semanticModel)
        {
            var targetSymbol = semanticModel.GetSymbolInfo(collectionItem.targetAccess).Symbol;
            var sourceSymbol = semanticModel.GetSymbolInfo(collectionItem.sourceAccess).Symbol;
            INamedTypeSymbol targetMember = null;
            INamedTypeSymbol sourceMember = null;
            {
                if (targetSymbol is IFieldSymbol fieldSymbol)
                {
                    targetMember = fieldSymbol.Type as INamedTypeSymbol;
                }
                else if (targetSymbol is IPropertySymbol propertySymbol)
                {
                    targetMember = propertySymbol.Type as INamedTypeSymbol;
                }
            }

            {
                if (sourceSymbol is IFieldSymbol fieldSymbol)
                {
                    sourceMember = fieldSymbol.Type as INamedTypeSymbol;
                }
                else if (sourceSymbol is IPropertySymbol propertySymbol)
                {
                    sourceMember = propertySymbol.Type as INamedTypeSymbol;
                }
            }
            collectionItem.sourceAccessTypeSymbol = GetDataType(sourceMember,out var sourceIsDictionary,out var keyType);
            collectionItem.targetAccessTypeSymbol = GetDataType(targetMember,out var targetIsDictionary, out var  targetKeyType);
            collectionItem.sourceIsDictionary = sourceIsDictionary;
            collectionItem.targetIsDictionary = targetIsDictionary;
            collectionItem.keyType = keyType;
        }

        public void VisitCollectionBindingSyntaxNode(GeneratorSyntaxContext context)
        {
            if (!(context.Node is InvocationExpressionSyntax invocationExpression))
            {
                return;
            }

            var methodSymbol = context.SemanticModel.GetSymbolInfo(context.Node).Symbol as IMethodSymbol;
            if (methodSymbol == null || methodSymbol.Name != "BindCollection" || !methodSymbol.IsStatic)
            {
                return;
            }

            if (invocationExpression.ArgumentList?.Arguments.Count < 3)
            {
                return;
            }


            CollectionItem collectionItem = new CollectionItem()
            {
                sourceAccess = (MemberAccessExpressionSyntax)invocationExpression.ArgumentList.Arguments[0].Expression,
                targetAccess = (IdentifierNameSyntax)invocationExpression.ArgumentList.Arguments[1].Expression,
            };

            GetBindingSourceAndTargetType(collectionItem,context.SemanticModel);
            {
                var creatorExpression = invocationExpression.ArgumentList.Arguments[2].Expression;
                if (creatorExpression is MemberAccessExpressionSyntax memberAccessExpression)
                {
                    collectionItem.creatorMemberAccessor = memberAccessExpression;
                }
                else if(creatorExpression is IdentifierNameSyntax identifier)
                {
                    collectionItem.creatorAccessor = identifier;
                }
            }

            // 获取Filter和Predicate函数信息
            {
                var arguments = invocationExpression.ArgumentList.Arguments;
                if (arguments.Count >= 4)
                {
                    ExpressionSyntax removeFuncExpression = arguments[3].Expression;
                    if (removeFuncExpression is MemberAccessExpressionSyntax memberAccess)
                    {
                        collectionItem.removeMemberAccessor = memberAccess;
                        collectionItem.customAddOrRemove = true;
                    }
                    else if (removeFuncExpression is IdentifierNameSyntax identifier)
                    {
                        collectionItem.removeAccessor = identifier;
                        collectionItem.customAddOrRemove = true;
                    }
                }

                if (arguments.Count >= 5)
                {
                    ExpressionSyntax clearFuncExpression = arguments[4].Expression;
                    if (clearFuncExpression is MemberAccessExpressionSyntax memberAccess)
                    {
                        collectionItem.clearMemberAccessor = memberAccess;
                        collectionItem.customAddOrRemove = true;
                    }
                    else if (clearFuncExpression is IdentifierNameSyntax identifier)
                    {
                        collectionItem.clearAccessor = identifier;
                        collectionItem.customAddOrRemove = true;
                    }
                }
            }

            var memberName = collectionItem.targetAccess.Identifier.Text;
            var sourceMemberName = collectionItem.sourceAccess.Name.ToString();
            collectionItem.funcionPrefix = $"{DataBindingCodeGenerator.UpperCaseCamelCase(memberName)}{DataBindingCodeGenerator.UpperCaseCamelCase(sourceMemberName)}";
            var typeSymbol = GetClassSymbol(invocationExpression, context,out var partial);
            if (CollectionItems.TryGetValue(typeSymbol, out var collectionItems))
            {
                collectionItems.Add(collectionItem);
            }
            else
            {
                collectionItems = new List<CollectionItem>() { collectionItem };
                CollectionItems.Add(typeSymbol, collectionItems);
            }
        }
    }
}
