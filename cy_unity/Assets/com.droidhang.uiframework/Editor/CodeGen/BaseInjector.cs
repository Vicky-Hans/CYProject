// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;
// using System.Text.RegularExpressions;
// using UI;
// using UnityEditor;
// using UnityEngine;
//
// namespace DH.UIFramework.Editor
// {
//     public class BaseInjector
//     {
//         /// <summary>
//         /// 生成的绑定代码
//         /// </summary>
//         protected string binding;
//
//         protected string inject;
//         protected Regex pathRegex;
//         protected Regex typeRegex;
//         protected Regex noValueTypeRegex;
//         protected Regex injectLocateRegex;
//         protected string path;
//         protected string type;
//         protected string propertyName;
//         protected bool preDefineType;
//
//         private Dictionary<string, string> PreDefinedTypes = new Dictionary<string, string>()
//         {
//             { "GameObject", "UnityEngine.GameObject" },
//             { "RectTransform", "UnityEngine.RectTransform" },
//             { "Transform", "UnityEngine.Transform" },
//             { "Button", "UnityEngine.UI.Button" },
//             { "Dropdown", "UnityEngine.UI.Dropdown" },
//             { "Text", "UnityEngine.UI.Text" },
//             { "InputField", "UnityEngine.UI.InputField" },
//             { "Image", "UnityEngine.UI.Image" },
//             { "ToggleGroup", "UnityEngine.UI.ToggleGroup" },
//             { "Toggle", "UnityEngine.UI.Toggle" },
//             { "TMPText", "TMPro.TMP_Text" },
//             { "TMPInputField", "TMPro.TMP_InputField" },
//             { "UICircularScrollView", "DH.UIFramework.UICircularScrollView" },
//             { "HoldButton", "DH.UIFramework.HoldButton" },
//             { "Slider", "UnityEngine.UI.Slider" },
//             { "TMPTextEventHandler", "DH.UIFramework.TMP_TextEventHandler" }
//         };
//
//         public BaseInjector()
//         {
//             pathRegex = new Regex(".*path.*=(.*)}");
//             typeRegex = new Regex(".*injectType.*=(.*),.*value.*=.*");
//             noValueTypeRegex = new Regex(".*injectType.*=(.*),.*path.*=.*");
//             injectLocateRegex = new Regex($".*\\['(.*)'\\].*");
//         }
//
//         public void Inject(string content, BaseView owner)
//         {
//             // 已注释代码
//             if (content.StartsWith("--"))
//             {
//                 return;
//             }
//
//             if (pathRegex.IsMatch(content))
//             {
//                 path = pathRegex.Replace(content, "$1");
//             }
//             else
//             {
//                 return;
//             }
//
//             if (typeRegex.IsMatch(content))
//             {
//                 type = typeRegex.Replace(content, "$1");
//             }
//             else if (noValueTypeRegex.IsMatch(content))
//             {
//                 type = noValueTypeRegex.Replace(content, "$1");
//             }
//             else
//             {
//                 return;
//             }
//
//             if (injectLocateRegex.IsMatch(content))
//             {
//                 propertyName = injectLocateRegex.Replace(content, "$1");
//             }
//             else
//             {
//                 return;
//             }
//
//             if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(type))
//             {
//                 return;
//             }
//
//             BeautyPath();
//             BeautyType();
//
//             Transform target = owner.transform.Find(path);
//             if (!target)
//             {
//                 Debug.LogError($"Inject failed with error type {type} path {path}");
//                 return;
//             }
//
//             if (type == typeof(GameObject).FullName)
//             {
//                 var gameObject = target.gameObject;
//                 owner.variables.AddVariable(propertyName, gameObject);
//             }
//             else if (type == typeof(Transform).FullName)
//             {
//                 owner.variables.AddVariable(propertyName, target);
//             }
//             else if (type == typeof(RectTransform).FullName)
//             {
//                 owner.variables.AddVariable(propertyName, target as RectTransform);
//             }
//             else
//             {
//                 var component = target.GetComponent(type);
//                 if (!component)
//                 {
//                     Debug.LogError($"Inject failed with error type {type} path {path}");
//                     return;
//                 }
//
//                 owner.variables.AddVariable(propertyName, component);
//             }
//
//             EditorUtility.SetDirty(owner);
//         }
//
//         private void BeautyPath()
//         {
//             path = path.Trim();
//             path = path.Replace("\'", string.Empty);
//             path = path.Replace("\"", string.Empty);
//         }
//
//         private void BeautyType()
//         {
//             type = type.Trim();
//             preDefineType = !type.Contains("\'") && !type.Contains("\"");
//             type = type.Replace("\'", string.Empty);
//             type = type.Replace("\"", string.Empty);
//             if (preDefineType)
//             {
//                 var splited = type.Split('.');
//                 type = splited[splited.Length - 1];
//                 type = PreDefinedTypes[type];
//             }
//         }
//     }
// }