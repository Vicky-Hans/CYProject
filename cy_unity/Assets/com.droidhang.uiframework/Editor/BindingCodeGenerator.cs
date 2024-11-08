// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Text;
// using System.Text.RegularExpressions;
// using DH.NativeCore.Platform;
// using DHFramework;
// using UI;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace DH.UIFramework.Editor
// {
//     public static class BindingCodeGenerator
//     {
//         public class ConfigData
//         {
//             public List<string> Content;
//             public int InjectStartIndex;
//             public int InjectEndIndex;
//             public int BindingStartIndex;
//             public int BindingEndIndex;
//             public int RequireStartIndex = -1;
//             public int RequireEndIndex = -1;
//             public int OnDestroyStartIndex = -1;
//             public int OnDestroyEndIndex = -1;
//
//             public void Initialize()
//             {
//                 //0：未开始；1开始；2结束
//                 int requireState = 0;
//                 int onDestroyState = 0;
//                 
//                 for(int index = 0;index < Content.Count;index++)
//                 {
//                     var content = Content[index];
//                     if (content.Contains(InjectStartTag))
//                     {
//                         InjectStartIndex = index + 1;
//                     }
//                     else if (content.Contains(InjectEndTag))
//                     {
//                         InjectEndIndex = index + 1;
//                     }
//                     else if(content.Contains(BindingStartTag))
//                     {
//                         BindingStartIndex = index + 1;
//                     }
//                     else if (content.Contains(BindingEndTag))
//                     {
//                         BindingEndIndex = index + 1;
//                     }
//                     
//                     if (requireState < 2)
//                     {
//                         if (RequireRegex.IsMatch(content))
//                         {
//                             if (requireState == 0)
//                             {
//                                 RequireStartIndex = RequireEndIndex = index;
//                                 requireState = 1;
//                             }
//                             else
//                             {
//                                 RequireEndIndex = index;
//                             }
//                         }else if (requireState == 1)
//                         {
//                             requireState = 2;
//                         }
//                     }
//                     
//                     if (onDestroyState < 2)
//                     {
//                         if (OnDestroyRegex.IsMatch(content) && onDestroyState == 0)
//                         {
//                             OnDestroyStartIndex = OnDestroyEndIndex = index;
//                             onDestroyState = 1;
//                         }
//                         else if (onDestroyState == 1)
//                         {
//                             OnDestroyEndIndex = index;
//                             
//                             if (content.Equals("end"))
//                             {
//                                 onDestroyState = 2;
//                             }
//                         }
//                     }
//                 }
//             }
//
//             public void InsertInject(List<string> injectData)
//             {
//                 int removeCount = InjectEndIndex - InjectStartIndex - 1;
//                 if (removeCount > 0)
//                 {
//                     Content.RemoveRange(InjectStartIndex,removeCount);
//                 }
//
//                 Content.InsertRange(InjectStartIndex,injectData);
//                 Initialize();
//             }
//             
//             public void InsertBinding(List<string> bindingData)
//             {
//                 int removeCount = BindingEndIndex - BindingStartIndex - 1;
//                 if (removeCount > 0)
//                 {
//                     Content.RemoveRange(BindingStartIndex,removeCount);
//                 }
//                 
//                 Content.InsertRange(BindingStartIndex,bindingData);
//             }
//             
//             public void InsertRequireList(List<string> requireList)
//             {
//                 if (requireList.Count == 0)
//                 {
//                     return;
//                 }
//                 
//                 Content.InsertRange(RequireStartIndex,requireList);
//                 Initialize();
//             }
//
//             public void InitOnDestroy(string fileName)
//             {
//                 OnDestroyStartIndex = Content.Count + 1;
//                 OnDestroyEndIndex = Content.Count + 2;
//                 
//                 Content.Add("\n");
//                 Content.Add($"function {fileName}:onDestroy()");
//                 Content.Add("end");
//             }
//             
//             public void InsertOnDestroy(List<string> onDestroyList)
//             {
//                 if (onDestroyList.Count == 0)
//                 {
//                     return;
//                 }
//
//                 if (OnDestroyEndIndex == -1)
//                 {
//                     return;
//                 }
//                 
//                 Content.InsertRange(OnDestroyEndIndex - 1, onDestroyList);
//                 Initialize();
//             }
//         }
//
//         private static readonly string BindingStartTag = "--- generated binding tag---";
//         private static readonly string BindingEndTag = "--- end generated binding tag---";
//         private static readonly string InjectStartTag = "--- generated inject tag---";
//         private static readonly string InjectEndTag = "--- end generated inject tag---";
//         private static readonly Regex RequireRegex = new Regex($".*require\\(\".*\"\\).*");
//         private static readonly Regex OnDestroyRegex = new Regex($".*:onDestroy\\(\\).*");
//         
//         static string GetCurrentFileName([System.Runtime.CompilerServices.CallerFilePath] string fileName = null)
//         {
//             return fileName;
//         }
//
//         [MenuItem("CONTEXT/LuaBehaviour/自动注入引用")]
//         public static void InjectComponent(MenuCommand command)
//         {
//             BaseView component = (BaseView) command.context;
//             if (component.script.Type == ScriptReferenceType.TextAsset)
//             {
//                 Debug.LogError("TextAsset强引用方式不支持自动注入引用");
//                 return;
//             }
//
//             var dirs = PrepareLuaFolder(out var mainLuaRoot);
//             var filePath = GetFilePath(dirs, component.script.Filename);
//             if (string.IsNullOrEmpty(filePath))
//             {
//                 Debug.LogError($"{component.script.Filename}文件不存在，无法自动生成绑定");
//                 return;
//             }
//
//
//             var configData = GenerateConfigData(filePath);
//             var generator = new BaseInjector();
//             foreach (var item in configData.Content)
//             {
//                 generator.Inject(item,component);
//             }
//         }
//
//         [MenuItem("CONTEXT/LuaBehaviour/自动生成绑定")]
//         public static void GenerateComponentBinding(MenuCommand command)
//         {
//             var component = (LuaBehaviour) (command.context);
//             GenerateBinding(component);
//             AssetDatabase.Refresh();
//         }
//         
//         [MenuItem("CONTEXT/Component/生成注入代码")]
//         public static void GenerateInjectCode(MenuCommand command)
//         {
//             var component = (Component) (command.context);
//             var root = component.GetComponentInParent<LuaBehaviour>();
//             if (!root)
//             {
//                 Debug.LogError($"LuaBehaviour组件不存在，无法自动生成注入代码");
//                 return;
//             }
//             
//             var left = '{';
//             var right = '}';
//             var typeName = component.GetType().FullName;
//             var scriptPath = root.script.Filename;
//             var fileNames = scriptPath.Split('/');
//             var fileName = fileNames[fileNames.Length - 1];
//             var path = GeneratePath(root.transform, component.transform);
//             var data = $"{fileName}['{component.name}'] = {left}injectType = '{typeName}', value=nil, path='{path}'{right}";
//             DeviceUtility.CopyToClipboard(data);
//             AssetDatabase.Refresh();
//         }
//         
//         [MenuItem("CONTEXT/Transform/生成注入代码")]
//         public static void GenerateTransformInjectCode(MenuCommand command)
//         {
//             var component = (Transform) (command.context);
//             var root = component.GetComponentInParent<LuaBehaviour>();
//             if (!root)
//             {
//                 Debug.LogError($"LuaBehaviour组件不存在，无法自动生成注入代码");
//                 return;
//             }
//             
//             var left = '{';
//             var right = '}';
//             var typeName = component.GetType().FullName;
//             var path = GeneratePath(root.transform, component);
//             var data = $"Binding['{component.name}'] = {left}injectType = '{typeName}', value=nil, path='{path}'{right}";
//             DeviceUtility.CopyToClipboard(data);
//             AssetDatabase.Refresh();
//         }
//
//         [MenuItem("Assets/Create/Lua Script/View", true)]
//         public static bool ValidateCreateLuaView(MenuCommand command)
//         {
//             if (Selection.objects.Length == 0)
//             {
//                 return false;
//             }
//             
//             var select = Selection.objects[0];
//             var path = AssetDatabase.GetAssetPath(select);
//             if (!path.Contains("LuaScript"))
//             {
//                 return false;
//             }
//
//             return true;
//         }
//         
//         [MenuItem("Assets/Create/Lua Script/View",false,4)]
//         public static void CreateLuaView(MenuCommand command)
//         {
//             var select = Selection.objects[0];
//             var path = AssetDatabase.GetAssetPath(select);
//             var attr = File.GetAttributes(path);
//             var dir = path;
//             if (!attr.HasFlag(FileAttributes.Directory))
//             {
//                 dir = new FileInfo(path).DirectoryName;
//             }
//             LuaFileNameWindow window = ScriptableObject.CreateInstance<LuaFileNameWindow>();
//             window.name = "Lua文件名称";
//             window.position = new Rect(Screen.width/2, Screen.height / 2, 250, 150);
//             window.Show();
//             window.OnConfirm = (fileName) =>
//             {
//                 if (fileName.Contains(".lua"))
//                 {
//                     fileName = fileName.Replace(".lua", string.Empty);
//                 }
//
//                 CreateLuaScript(dir, fileName);
//
//                 AssetDatabase.Refresh();
//             };
//         }
//
//         public static void GenerateBinding(LuaBehaviour component)
//         {
//             if (component.script.Type == ScriptReferenceType.TextAsset)
//             {
//                 Debug.LogError("TextAsset强引用方式不支持自动生成绑定");
//                 return;
//             }
//             
//             var dirs = PrepareLuaFolder(out var mainLuaRoot);
//             var filePath = GetFilePath(dirs, component.script.Filename);
//             if (string.IsNullOrEmpty(filePath) && string.IsNullOrEmpty(mainLuaRoot))
//             {
//                 Debug.LogError($"{component.script.Filename}文件不存在，无法自动生成绑定");
//                 return;
//             }
//             
//             // Lua View文件不存在，使用模版创建文件
//             if (string.IsNullOrEmpty(filePath))
//             {
//                 filePath = CreateLuaScript(mainLuaRoot,component.script.Filename);
//             }
//
//             var configData = GenerateConfigData(filePath);
//             var supportedTypes = GenerateSupportType();
//
//             GenerateAll(supportedTypes, configData, component, filePath);
//         }
//
//         private static string CreateLuaScript(string root,string filePath)
//         {
//             var template = Path.Combine(GetCurrentFileName(), "../ViewTemplate.txt");
//             string nameTag = "${NAME}";
//             string dateTag = "${DATE}";
//             string timeTag = "${TIME}";
//             string userTag = "${USER}";
//             var templateStr = File.ReadAllText(template);
//             var filePathSplitted = filePath.Split('/');
//             string fileName = filePathSplitted[filePathSplitted.Length - 1];
//             string date = DateTime.Now.ToString("yyyy/MM/dd");
//             string time = DateTime.Now.ToString("hh:mm tt");
//             string user = Environment.UserName;
//             templateStr = templateStr.Replace(nameTag, fileName);
//             templateStr = templateStr.Replace(dateTag, date);
//             templateStr = templateStr.Replace(timeTag, time);
//             templateStr = templateStr.Replace(userTag, user);
//
//             var path = Path.Combine(root, filePath + ".lua");
//             var dir = new FileInfo(path).DirectoryName;
//             if (!Directory.Exists(dir))
//             {
//                 Directory.CreateDirectory(dir);
//             }
//             File.WriteAllText(path,templateStr);
//
//             return path;
//         }
//
//         private static void GenerateAll(Dictionary<Type,Type> supportedTypes,ConfigData configData,LuaBehaviour component,string filePath)
//         {
//             string fileName = component.script.Filename.Split('/').Last();
//
//             if (configData.OnDestroyStartIndex == -1 && configData.OnDestroyEndIndex == -1)
//             {
//                 configData.InitOnDestroy(fileName);
//             }
//             
//             List<BaseGenerator> generators = new List<BaseGenerator>();
//             foreach (var typePair in supportedTypes)
//             {
//                 var targets = component.GetComponentsInChildren(typePair.Key);
//                 foreach (var item in targets)
//                 {
//                     if (item.GetType() != typePair.Key)
//                     {
//                         continue;
//                     }
//                     
//                     var generator = Activator.CreateInstance(typePair.Value, fileName) as BaseGenerator;
//                     var itemTrans = item.transform;
//                     
//                     var circularScrollViewType = typeof(UICircularScrollView);
//
//                     if (circularScrollViewType.IsAssignableFrom(typePair.Key))
//                     {
//                         ScrollRect rect = item.GetComponent<ScrollRect>();
//
//                         if (!rect)
//                         {
//                             continue;
//                         }
//
//                         itemTrans = rect.content.transform;
//                     }
//                     
//                     generator.Generate(configData,GeneratePath(component.transform,itemTrans));
//                     generators.Add(generator);
//                 }
//             }
//             
//             List<string> inject = new List<string>();
//             List<string> binding = new List<string>();
//             List<string> requireList = new List<string>();
//             List<string> destroyList = new List<string>();
//             foreach (var item in generators)
//             {
//                 inject.Add(item.Inject);
//                 binding.Add(item.Binding);
//                 requireList.AddRange(item.NeedRequireList);
//                 destroyList.AddRange(item.NeedOnDestroyList);
//             }
//             
//             //大概率从后往前插入
//             configData.InsertOnDestroy(destroyList);
//             configData.InsertBinding(binding);
//             configData.InsertInject(inject);
//             configData.InsertRequireList(requireList);
//             
//             using (var fileStream = File.Open(filePath, FileMode.Create))
//             {
//                 using (var writer = new StreamWriter(fileStream))
//                 {
//                     foreach (var content in configData.Content)
//                     {
//                         writer.WriteLine(content);
//                     }
//                 }
//             }
//         }
//
//         private static ConfigData GenerateConfigData(string filePath)
//         {
//             var fileData = File.ReadAllLines(filePath);
//             ConfigData configData = new ConfigData()
//             {
//                 Content = new List<string>(fileData)
//             };
//
//             configData.Initialize();
//
//             return configData;
//         }
//
//         private static string GetFilePath(List<string> dirs, string fileName)
//         {
//             foreach (var item in dirs)
//             {
//                 var path = Path.Combine(item, fileName + ".lua");
//                 if (File.Exists(path))
//                 {
//                     return path;
//                 }
//             }
//
//             return null;
//         }
//
//         private static List<string> PrepareLuaFolder(out string mainLuaRoot)
//         {
//             List<string> directories = new List<string>();
//             mainLuaRoot = string.Empty;
//             
//             foreach (string dir in Directory.GetDirectories(Application.dataPath, "LuaScript",
//                 SearchOption.AllDirectories))
//             {
//                directories.Add(dir);
//                if (!dir.Contains("com.droidhang") && !dir.Contains("Editor") && !dir.Contains("Gen"))
//                {
//                    mainLuaRoot = dir;
//                }
//             }
//
//             var upmPaths = Path.GetFullPath($"Packages");
//             foreach (string dir in Directory.GetDirectories(upmPaths, "LuaScript",
//                 SearchOption.AllDirectories))
//             {
//                 directories.Add(dir);
//             }
//             return directories;
//         }
//
//         private static string GeneratePath(Transform root, Transform current)
//         {
//             StringBuilder sb = new StringBuilder();
//             sb.Append(current.name);
//             while (true)
//             {
//                 current = current.parent;
//                 if (current == root || !current)
//                 {
//                     break;
//                 }
//                 
//                 sb.Insert(0,'/');
//                 sb.Insert(0, current.name);
//             }
//
//             return sb.ToString();
//         }
//
//         private static Dictionary<Type, Type> GenerateSupportType()
//         {
//             var baseType = typeof(BaseGenerator);
//             var types = DHUtility.Assembly.GetTypes().Where(x=>baseType.IsAssignableFrom(x));
//             Dictionary<Type, Type> supportedTypes = new Dictionary<Type, Type>();
//             foreach (var item in types)
//             {
//                 if (item == baseType)
//                 {
//                     continue;
//                 }
//                 
//                 var instance = Activator.CreateInstance(item,string.Empty) as BaseGenerator;
//                 supportedTypes.Add(instance.Target,item);
//             }
//
//             return supportedTypes;
//         }
//     }
// }