// using System;
// using System.Collections.Generic;
// using System.Text.RegularExpressions;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace DH.UIFramework.Editor
// {
//     public class BaseGenerator
//     {
//         protected const string EndTag = "Build(self.dataContext)";
//         
//         /// <summary>
//         /// 生成的绑定代码
//         /// </summary>
//         protected string binding;
//         protected string inject;
//         protected List<string> needRequireList = new List<string>();
//         protected List<string> needOnDestroyList = new List<string>();
//         protected string path;
//         protected string propertyName;
//         protected string fileName;
//         protected Regex bindingRegex;
//         protected Regex bindingLocateRegex;
//         protected Regex injectLocateRegex;
//         
//         protected Type target;
//
//         public BaseGenerator(Type type,string fileName)
//         {
//             target = type;
//             this.fileName = fileName;
//             bindingRegex = new Regex(".*:To\\((.*)\\):.*",RegexOptions.Singleline);
//             bindingLocateRegex = new Regex(".*:Target\\(self.(.*)\\):For.*Build\\(self.dataContext\\)",RegexOptions.Singleline);
//             injectLocateRegex  = new Regex($".*{fileName}\\['(.*)'\\].*");
//         }
//
//         /// <summary>
//         /// 生成的绑定代码
//         /// </summary>
//         public string Binding => binding;
//
//         public string Inject => inject;
//         public List<string> NeedRequireList => needRequireList;
//         public List<string> NeedOnDestroyList => needOnDestroyList;
//
//         public Type Target => target;
//
//         public void Generate(BindingCodeGenerator.ConfigData configData,string comPath)
//         {
//             path = comPath;
//             
//             GeneratePropertyName();
//             GenerateRequire(configData);
//             
//             bool hasComment = GenerateInject(configData);
//             
//             if (string.IsNullOrEmpty(binding) || !binding.Contains("--"))
//             {
//                 GenerateBinding(configData, hasComment);
//             }
//             
//             GenerateOnDestroy(configData);
//         }
//
//         protected virtual void GenerateRequire(BindingCodeGenerator.ConfigData configData)
//         {
//         }
//
//         protected virtual void GenerateOnDestroy(BindingCodeGenerator.ConfigData configData)
//         {
//         }
//
//         protected virtual void GenerateBinding(BindingCodeGenerator.ConfigData configData, bool hasComment)
//         {
//             string oldBinding = GetBinding(configData);
//             binding = oldBinding;
//             
//             // 注入代码被注释掉的情况下，需要确认绑定代码被注释掉
//             if (hasComment && !binding.Contains("--"))
//             {
//                 binding = $"--[[{binding}--]]";
//             }
//             
//             if (string.IsNullOrEmpty(binding) || !binding.Contains("--"))
//             {
//                 GenerateBinding();
//             }
//         }
//
//         protected virtual void GenerateBinding()
//         {
//         }
//
//         protected virtual string GetInjectTypeName()
//         {
//             return Target.FullName;
//         }
//
//         protected virtual bool GenerateInject(BindingCodeGenerator.ConfigData configData)
//         {
//             string oldInject = GetInject(configData);
//             inject = oldInject;
//
//             if (string.IsNullOrEmpty(inject) || !inject.Contains("--"))
//             {
//                 var typeName = GetInjectTypeName();
//                 var left = '{';
//                 var right = '}'; 
//                 string reference = $"{fileName}['{propertyName}'] = {left} injectType = '{typeName}', value=nil, path='{path}'{right}";
//                 inject = reference;
//             }
//
//             return oldInject.Contains("--");
//         }
//
//         protected virtual void GeneratePropertyName()
//         {
//             var splitedPath = path.Split('/');
//             propertyName = splitedPath[splitedPath.Length - 1];
//         }
//
//         protected virtual string GetBinding(BindingCodeGenerator.ConfigData data)
//         {
//             for (int index = data.BindingStartIndex; index < data.BindingEndIndex; index++)
//             {
//                 var str = data.Content[index];
//                 while (!str.Contains(EndTag) && index < data.BindingEndIndex)
//                 {
//                     index++;
//                     str += '\n';
//                     str += data.Content[index];
//                 }
//                 if (!bindingLocateRegex.IsMatch(str))
//                 {
//                     continue;
//                 }
//                 
//                 var result = bindingLocateRegex.Replace(str, "$1");
//                 if (result == propertyName)
//                 {
//                     return str;
//                 }
//             }
//
//             return string.Empty;
//         }
//         
//         private string GetInject(BindingCodeGenerator.ConfigData data)
//         {
//             for (int index = data.InjectStartIndex; index < data.InjectEndIndex; index++)
//             {
//                 var str = data.Content[index];
//                 if (!injectLocateRegex.IsMatch(str))
//                 {
//                     continue;
//                 }
//                 
//                 var result = injectLocateRegex.Replace(str, "$1");
//                 if (result == propertyName)
//                 {
//                     return str;
//                 }
//             }
//
//             return string.Empty;
//         }
//
//         public override string ToString()
//         {
//             return $"binding {Binding} \r\n inject {Inject}";
//         }
//     }
//
//     public class TextGenerator : BaseGenerator
//     {
//         public TextGenerator(string fileName):base(typeof(Text),fileName)
//         {
//             
//         }
//         
//         public TextGenerator(Type type, string fileName):base(type,fileName)
//         {
//             
//         }
//
//         protected override void GenerateBinding()
//         {
//             string reference = $"\tBinding():Target(self.{propertyName}):For('text'):To('记得填哦'):Build(self.dataContext)";
//             if (string.IsNullOrEmpty(Binding))
//             {
//                 binding = reference;
//                 return;
//             }
//             
//             var result = bindingRegex.Replace(binding, "$1");
//             binding = $"\tBinding():Target(self.{propertyName}):For('text'):To({result}):Build(self.dataContext)";
//         }
//     }
//     
//     public class ButtonGenerator : BaseGenerator
//     {
//         public ButtonGenerator(string fileName):base(typeof(Button),fileName)
//         {
//             
//         }
//
//         protected override void GenerateBinding()
//         {
//             string reference = $"\tBinding():Target(self.{propertyName}):For('onClick'):To('记得填哦'):Build(self.dataContext)";
//             if (string.IsNullOrEmpty(Binding))
//             {
//                 binding = reference;
//                 return;
//             }
//             
//             var result = bindingRegex.Replace(binding, "$1");
//             binding = $"\tBinding():Target(self.{propertyName}):For('onClick'):To({result}):Build(self.dataContext)";
//         }
//     }
//
//     public class InputFieldGenerator : BaseGenerator
//     {
//         public InputFieldGenerator(string fileName):base(typeof(InputField),fileName)
//         {
//             
//         }
//         
//         public InputFieldGenerator(Type type, string fileName):base(type,fileName)
//         {
//             
//         }
//
//         protected override void GenerateBinding()
//         {
//             string reference = $"\tBinding():Target(self.{propertyName}):For('text', 'onEndEdit'):To('记得填哦'):TwoWay():Build(self.dataContext)";
//             if (string.IsNullOrEmpty(Binding))
//             {
//                 binding = reference;
//                 return;
//             }
//             
//             var result = bindingRegex.Replace(binding, "$1");
//             binding = $"\tBinding():Target(self.{propertyName}):For('onClick'):To({result}):Build(self.dataContext)";
//         }
//     }
//     
//     public class TextMeshProUGUIGenerator : TextGenerator
//     {
//         public TextMeshProUGUIGenerator(string fileName):base(typeof(TextMeshProUGUI),fileName)
//         {
//             
//         }
//     }
//     
//     public class TextMeshProGenerator : TextGenerator
//     {
//         public TextMeshProGenerator(string fileName):base(typeof(TextMeshPro),fileName)
//         {
//             
//         }
//     }
//
//     public class SliderGenerator : BaseGenerator
//     {
//         public SliderGenerator(string fileName) : base(typeof(Slider), fileName)
//         {
//             
//         }
//         
//         protected override void GenerateBinding()
//         {
//             string reference = $"\tBinding():Target(self.{propertyName}):For('text','onValueChanged'):To('记得填哦'):TwoWay():Build(self.dataContext)";
//             if (string.IsNullOrEmpty(Binding))
//             {
//                 binding = reference;
//                 return;
//             }
//             
//             var result = bindingRegex.Replace(binding, "$1");
//             binding = $"\tBinding():Target(self.{propertyName}):For('value','onValueChanged'):To({result}):Build(self.dataContext)";
//         }
//     }
//     
//     public class UICircularScrollViewGenerator : BaseGenerator
//     {
//         protected string factoryLuaName;
//         protected string factoryPropertyName;
//         protected string factoryBindStr;
//         
//         protected Regex factoryLuaRequireRegex;
//         protected Regex collectionBindingRequireRegex;
//         protected Regex onDestroyRegex;
//
//         public UICircularScrollViewGenerator(string fileName):base(typeof(UICircularScrollView),fileName)
//         {
//             InitGenerator();
//         }
//         
//         public UICircularScrollViewGenerator(string fileName, Type scrollType):base(scrollType,fileName)
//         {
//             InitGenerator();
//         }
//
//         protected void InitGenerator()
//         {
//             SetFactoryLuaName();
//             factoryLuaRequireRegex = new Regex($"local {factoryLuaName} *= *require\\(\"Support/{factoryLuaName}\"\\) *",RegexOptions.Singleline);
//             collectionBindingRequireRegex  = new Regex("local CircularDynamicCollectionBinding *= *require\\(\"Binding/CircularDynamicCollectionBinding\"\\) *",RegexOptions.Singleline);
//             //override
//             bindingLocateRegex = new Regex($".*self.(.*) *= *{factoryLuaName}\\(self.(\\w+),.*\\).*CircularDynamicCollectionBinding\\(\\):Target\\(.*\\):.*Build\\(self.dataContext\\)", RegexOptions.Singleline);
//         }
//         
//         protected override string GetInjectTypeName()
//         {
//             return "UnityEngine.RectTransform";
//         }
//
//         protected virtual void SetFactoryLuaName()
//         {
//             factoryLuaName = "CircularDynamicViewFactory";
//         }
//
//         protected virtual void SetFactoryBindStr()
//         {
//             factoryBindStr = $"self.{factoryPropertyName} = {factoryLuaName}(self.{propertyName}, “cell的prefab地址”)";
//         }
//
//         protected void SetOnDestroyBindStr()
//         {
//             onDestroyRegex = new Regex($".*self.{factoryPropertyName}:Release\\(\\).*self.{factoryPropertyName} *= *nil.*", RegexOptions.Singleline);
//         }
//
//         protected override void GenerateRequire(BindingCodeGenerator.ConfigData data)
//         {
//             bool hasFactoryRequire = false;
//             bool hasCollectionBinding = false;
//             
//             for (int index = data.RequireStartIndex; index <= data.RequireEndIndex; index++)
//             {
//                 if (hasCollectionBinding && hasFactoryRequire)
//                 {
//                     break;
//                 }
//                 
//                 var str = data.Content[index];
//                 
//                 if (!hasFactoryRequire && factoryLuaRequireRegex.IsMatch(str))
//                 {
//                     hasFactoryRequire = true;
//                 }
//
//                 if (!hasCollectionBinding && collectionBindingRequireRegex.IsMatch(str))
//                 {
//                     hasCollectionBinding = true;
//                 }
//             }
//
//             if (!hasFactoryRequire)
//             {
//                 needRequireList.Add($"local {factoryLuaName} = require(\"Support/{factoryLuaName}\")");
//             }
//
//             if (!hasCollectionBinding)
//             {
//                 needRequireList.Add($"local CircularDynamicCollectionBinding = require(\"Binding/CircularDynamicCollectionBinding\")");
//             }
//         }
//
//         protected override void GenerateOnDestroy(BindingCodeGenerator.ConfigData data)
//         {
//             if (string.IsNullOrEmpty(factoryPropertyName))
//             {
//                 return;
//             }
//             
//             bool hasOnDestroy = false;
//
//             if (onDestroyRegex != null)
//             {
//                 string str = "";
//             
//                 for (int index = data.OnDestroyStartIndex; index <= data.OnDestroyEndIndex; index++)
//                 {
//                     if (index > data.OnDestroyStartIndex)
//                     {
//                         str += '\n';
//                         str += data.Content[index];
//                     }
//                 }
//
//                 hasOnDestroy = onDestroyRegex.IsMatch(str);
//             }
//
//             if (!hasOnDestroy)
//             {
//                 needOnDestroyList.Add($"\tself.{factoryPropertyName}:Release()");
//                 needOnDestroyList.Add($"\tself.{factoryPropertyName} = nil");
//             }
//         }
//
//         protected override string GetBinding(BindingCodeGenerator.ConfigData data)
//         {
//             for (int index = data.BindingStartIndex; index < data.BindingEndIndex; index++)
//             {
//                 var str = data.Content[index];
//                 while (!str.Contains(EndTag) && index < data.BindingEndIndex)
//                 {
//                     index++;
//                     str += '\n';
//                     str += data.Content[index];
//                 }
//                 
//                 if (!bindingLocateRegex.IsMatch(str))
//                 {
//                     continue;
//                 }
//                
//                 var result = bindingLocateRegex.Replace(str, "$2");
//                 
//                 if (result.Equals(propertyName))
//                 {
//                     factoryPropertyName = bindingLocateRegex.Replace(str, "$1").Trim();
//                     SetFactoryBindStr();
//                     SetOnDestroyBindStr();
//                     return str;
//                 }
//             }
//
//             return string.Empty;
//         }
//         
//         protected override void GenerateBinding()
//         {
//             if (string.IsNullOrEmpty(Binding) || string.IsNullOrEmpty(factoryPropertyName))
//             {
//                 factoryPropertyName = $"{propertyName}Factory";
//                 SetFactoryBindStr();
//                 SetOnDestroyBindStr();
//                 binding = $"\t{factoryBindStr}\n\tCircularDynamicCollectionBinding():Target(self.{factoryPropertyName}):To(\"绑定的集合对象\"):WithCollectionView(\"排序的对象(可选)）\"):Build(self.dataContext)";
//             }
//         }
//     }
//
//     public class UIGroupCircularScrollViewGenerator : UICircularScrollViewGenerator
//     {
//         public UIGroupCircularScrollViewGenerator(string fileName):base(fileName, typeof(GroupExpandCircularScrollView))
//         {
//         }
//
//         protected override void SetFactoryLuaName()
//         {
//             factoryLuaName = "CircularGroupFactory";
//         }
//
//         protected override void SetFactoryBindStr()
//         {
//             factoryBindStr = $"self.{factoryPropertyName} = {factoryLuaName}(self.{propertyName}, “cell的prefab地址”, “GroupItem的prefab地址”)";
//         }
//     }
//
//     public class UIFlipCircularScrollViewGenerator : UICircularScrollViewGenerator
//     {
//         public UIFlipCircularScrollViewGenerator(string fileName):base(fileName, typeof(FlipPageCircularScrollView))
//         {
//         }
//
//         protected override void SetFactoryLuaName()
//         {
//             factoryLuaName = "CircularFlipFactory";
//         }
//
//         protected override void SetFactoryBindStr()
//         {
//             factoryBindStr = $"self.{factoryPropertyName} = {factoryLuaName}(self.{propertyName},“cell的prefab地址”,“导航正常的prefab地址，可为nil”,“导航选择的prefab地址，可为nil”)";
//         }
//     }
// }