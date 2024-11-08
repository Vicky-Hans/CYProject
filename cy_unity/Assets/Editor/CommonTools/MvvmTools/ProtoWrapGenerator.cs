using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DH.Data;
using Google.Protobuf;
using UnityEditor;
using UnityEngine;

public static class ProtoWrapGenerator
{
    private static readonly Type ProtoMessageType = typeof(IMessage);
    private static readonly Type BaseDataType = typeof(BaseData);
    private static readonly Dictionary<Type, Type> Wrap2ProtoMap = new ();
    private static readonly Dictionary<Type, Type> Proto2WrapTypeMap = new ();

    //[MenuItem("DH Tools/生成ProtoWrap代码")]
    private static void GenerateProtoWrapCode()
    {
        Wrap2ProtoMap.Clear();
        Proto2WrapTypeMap.Clear();
        const string codeFilePath = "Assets/Scripts/Data/Generated/ProtoWrapCode.cs";
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("using DH.Proto;");
        stringBuilder.AppendLine("using DH.UIFramework.Observables;");
        stringBuilder.AppendLine("using Google.Protobuf.Collections;");
        stringBuilder.AppendLine("using Google.Protobuf;");
        stringBuilder.AppendLine("namespace DH.Data");
        stringBuilder.AppendLine("{");
        
        var types = Assembly.GetAssembly(BaseDataType).GetTypes().Where(x =>
            BaseDataType.IsAssignableFrom(x) && x != BaseDataType && x.GetCustomAttribute<ProtoWrapAttribute>() != null);
        foreach (var item in types)
        {
            var protoType = item.GetCustomAttribute<ProtoWrapAttribute>().type;
            Wrap2ProtoMap.Add(item,protoType);
            Proto2WrapTypeMap.Add(protoType,item);
        }

        foreach (var item in Wrap2ProtoMap)
        {
            var properties = item.Value.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            WriteContent(1,$"public partial class {item.Key.Name}",stringBuilder);
            WriteContent(1,"{",stringBuilder);
            WriteContent(2,$"private readonly {item.Value.Name} nestMessage;",stringBuilder);
            
            foreach (var prop in properties)
            {
                GenerateCollection(prop, 2, stringBuilder);
            }
            
            foreach (var prop in properties)
            {
                GenerateProtoWrap(prop, 2, stringBuilder);
            }
            
            WriteContent(0,"",stringBuilder);
            GenerateConstructor(item.Key, item.Value,properties, 2, stringBuilder);
            GenerateUpdateMessage(item.Key, item.Value, properties, 2, stringBuilder);
            foreach (var prop in properties)
            {
                GenerateProperty(prop, 2, stringBuilder);
            }
            WriteContent(1,"}",stringBuilder);
        }

        stringBuilder.AppendLine("}");
        
        File.WriteAllText(codeFilePath, stringBuilder.ToString());
        
        Debug.Log("Gen Proto Wrap code finish");
    }

    private static void RecursionCheckProtoType(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
    }

    private static bool IsWrapProtoMessage(Type type)
    {
        return ProtoMessageType.IsAssignableFrom(type) && Proto2WrapTypeMap.ContainsKey(type);
    }

    private static void GenerateProtoWrap(PropertyInfo property, int intent, StringBuilder stringBuilder)
    {
        var propertyType = property.PropertyType;
        if (!propertyType.IsGenericType && IsWrapProtoMessage(propertyType))
        {
            if (Proto2WrapTypeMap.ContainsKey(propertyType))
            {
                WriteContent(intent,$"private {GetReadableTypeName(property.PropertyType)} {LowerCaseCamelCase(property.Name)};",stringBuilder);
            }
        }
    }

    private static void GenerateCollection(PropertyInfo property, int intent, StringBuilder stringBuilder)
    {
        if (!property.PropertyType.IsGenericType)
        {
            return;
        }
        
        WriteContent(intent,$"private readonly {GetReadableTypeName(property.PropertyType)} {LowerCaseCamelCase(property.Name)} = new ();",stringBuilder);
    }
    
    private static void GenerateProperty(PropertyInfo property, int intent, StringBuilder stringBuilder)
    {
        WriteContent(intent,$"public {GetReadableTypeName(property.PropertyType)} {property.Name}",stringBuilder);
        WriteContent(intent,"{",stringBuilder);
        
        var propertyType = property.PropertyType;
        if (!propertyType.IsGenericType)
        {
            if (IsWrapProtoMessage(propertyType))
            {
                var propertyName = LowerCaseCamelCase(property.Name);
                
                WriteContent(intent+1,$"get => {propertyName};",stringBuilder);
                WriteContent(intent+1,$"set => Set(ref {propertyName}, value);",stringBuilder);
            }
            else
            {
                WriteContent(intent+1,$"get => nestMessage.{property.Name};",stringBuilder);
                WriteContent(intent+1,$"set => Set(nestMessage.{property.Name},value,nestMessage,(msg,newValue)=>msg.{property.Name} = newValue);",stringBuilder);
            }  
        }
        else
        {
            WriteContent(intent+1,$"get => {LowerCaseCamelCase(property.Name)};",stringBuilder);
        }
        WriteContent(intent,"}",stringBuilder);
        WriteContent(0,"",stringBuilder);
    }

    private static void GenerateConstructor(Type type, Type messageType,PropertyInfo[] propertyInfos,int intent,StringBuilder stringBuilder)
    {
        WriteContent(intent,$"public {type.Name}({messageType.Name} message)",stringBuilder);
        WriteContent(intent,"{",stringBuilder);
        WriteContent(intent+1,$"nestMessage = message;",stringBuilder);
        foreach (var property in propertyInfos)
        {
            if (!property.PropertyType.IsGenericType)
            {
                continue;
            }

            GenerateCollectionMerge(property, intent, stringBuilder);
        }
        WriteContent(intent,"}",stringBuilder);
        WriteContent(0,"",stringBuilder);
    }

    private static void GenerateCollectionMerge(PropertyInfo property,int intent,StringBuilder stringBuilder,bool clearCollection = false)
    {
        if (clearCollection)
        {
            WriteContent(intent+1,$"if(clearCollection)",stringBuilder);
            WriteContent(intent+1,"{",stringBuilder);
            WriteContent(intent+2,$"{LowerCaseCamelCase(property.Name)}.Clear();",stringBuilder);
            WriteContent(intent+1,"}",stringBuilder);
            WriteContent(0,"",stringBuilder);
        }
        
        WriteContent(intent+1,$"foreach(var genCodeItem in message.{property.Name})",stringBuilder);
        WriteContent(intent+1,"{",stringBuilder);
        if (GetObservableCollection(property.PropertyType) == "ObservableDictionary")
        {
            var wrapType = GetReadableTypeName(property.PropertyType.GetGenericArguments()[1]);
            WriteContent(intent + 2,
                property.PropertyType.GetGenericArguments()[1].IsPrimitive
                    ? $"{LowerCaseCamelCase(property.Name)}.Add(genCodeItem.Key,genCodeItem.Value);"
                    : $"{LowerCaseCamelCase(property.Name)}.Add(genCodeItem.Key,new {wrapType}(genCodeItem.Value));",
                stringBuilder);
        }
        else
        {
            var wrapType = GetReadableTypeName(property.PropertyType.GetGenericArguments()[0]);
            WriteContent(intent + 2,
                property.PropertyType.GetGenericArguments()[0].IsPrimitive
                    ? $"{LowerCaseCamelCase(property.Name)}.Add(genCodeItem);"
                    : $"{LowerCaseCamelCase(property.Name)}.Add(new {wrapType}(genCodeItem));", stringBuilder);
        }
        WriteContent(intent+1,"}",stringBuilder);
        WriteContent(0,"",stringBuilder);
    }
    
    private static void GenerateUpdateMessage(Type type, Type messageType,PropertyInfo[] propertyInfos,int intent,StringBuilder stringBuilder)
    {
        WriteContent(intent,$"public void MergeFrom({messageType.Name} message,bool clearCollection = false)",stringBuilder);
        WriteContent(intent,"{",stringBuilder);
        foreach (var prop in propertyInfos)
        {
            var propertyType = prop.PropertyType;
            if (propertyType.IsGenericType)
            {
                GenerateCollectionMerge(prop, intent, stringBuilder, true);
            }
            else
            {
                if (IsWrapProtoMessage(propertyType))
                {
                    WriteContent(intent+1,$"{prop.Name} = new {GetReadableTypeName(propertyType)}(message.{prop.Name});",stringBuilder);
                }
                else
                {
                    WriteContent(intent+1,$"{prop.Name} = message.{prop.Name};",stringBuilder);
                }
            }
        }
        WriteContent(intent,"}",stringBuilder);
        WriteContent(0,"",stringBuilder);
    }

    private static void WriteContent(int intent,string content,StringBuilder stringBuilder)
    {
        for (int i = 0; i < intent; i++)
        {
            stringBuilder.Append("\t");
        }

        stringBuilder.Append(content);
        stringBuilder.Append(Environment.NewLine);
    }

    private static string LowerCaseCamelCase(string name)
    {
        return Char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static string GetObservableCollection(Type type)
    {
        int index = type.Name.IndexOf("`", StringComparison.Ordinal);
        var collectionTypeName = type.Name.Substring(0, index);
        switch (collectionTypeName)
        {
            case "RepeatedField":
                return "ObservableList";
            case "MapField":
                return "ObservableDictionary";
            default:
                return collectionTypeName;
        }
    }


    private static string GetReadableTypeName(Type type)
    {
        if (Proto2WrapTypeMap.ContainsKey(type))
        {
            type = Proto2WrapTypeMap[type];
        }
        
        if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            var typeName = GetObservableCollection(type);
            List<string> types = new List<string>();
            foreach (var typeArgument in type.GetGenericArguments())
            {
                var name = GetReadableTypeName(typeArgument);
                types.Add(name);
            }
            typeName = $"{typeName}<{string.Join(',',types)}>";
            return typeName;
        }
        
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean:
                return "bool";
            
            case TypeCode.Byte:
                return "byte";
            
            case TypeCode.Char:
                return "char";
            
            case TypeCode.DateTime:
                return "DateTime";

            case TypeCode.Decimal:
                return "decimal";
            
            case TypeCode.Double:
                return "double";

            case TypeCode.Int16:
                return "short";
            
            case TypeCode.Int32:
                return "int";
            
            case TypeCode.Int64:
                return "long";
            
            case TypeCode.SByte:
                return "sbyte";
            
            case TypeCode.Single:
                return "float";
            
            case TypeCode.String:
                return "string";
            
            case TypeCode.UInt16:
                return "ushort";
            case TypeCode.UInt32:
                return "uint";
            
            case TypeCode.UInt64:
                return "ulong";
            
            default:
                return type.Name;
        }
    }
}