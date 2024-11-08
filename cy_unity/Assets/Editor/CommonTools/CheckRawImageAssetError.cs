using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Tool
{
    public class CheckRawImageAssetErrorWindows : EditorWindow
    {
        private CheckRawImageAssetError tempComp;
        private Vector2 scrollPos;
        private Vector2 scrollPos1;
        private string tempDesc;
        private bool IsPassSet;

        // 将名为"My Window"的菜单项添加到 Window 菜单
        [MenuItem("DH Tools/RawImage 的资源异常")]
        public static void ShowWindow()
        {
            GetWindow(typeof(CheckRawImageAssetErrorWindows));
        }

        private void OnEnable()
        {
            if (tempComp == null)
            {
                tempComp = new(true);
            }
            tempComp.GetJsonToFile();
            tempComp.CheckRawImageComponentAsset();
        }

        private void OnDestroy()
        {
            tempComp.SaveJsonToFile();
            tempComp = null;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("列表", GUILayout.Height(50)))
            {
                IsPassSet = false;
                tempComp.CheckRawImageComponentAsset();
            }

            if (GUILayout.Button("白名单", GUILayout.Height(50)))
            {
                IsPassSet = true;
                tempComp.CheckRawImageComponentAsset();
            }

            EditorGUILayout.EndHorizontal();
            if (!IsPassSet)
            {
                if (GUILayout.Button("刷新", GUILayout.Height(50))) tempComp.CheckRawImageComponentAsset();

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                var pos = 0;
                foreach (var item in tempComp.ItemList)
                {
                    if (!tempComp.CheckNoneChild(item.Value)) continue;
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.ObjectField(string.Empty, item.Key, typeof(GameObject), true,
                        GUILayout.Width(200), GUILayout.Height(30));
                    GUILayout.Label($"未标记数量 ：{item.Value.Count}", GUILayout.Width(150),
                        GUILayout.Height(30));
                    EditorGUILayout.EndHorizontal();
                    for (var i = 0; i < item.Value.Count; i++)
                    {
                        name = tempComp.GetParentPath(item.Value[i].transform);
                        if (tempComp.IsWhiteList(name)) continue;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(50);

                        if (GUILayout.Button("选中节点", GUILayout.Width(60), GUILayout.Height(30)))
                        {
                            var pos1 = i;
                            SelectGame(item.Value[pos1], name);
                        }

                        if (GUILayout.Button("白名单", GUILayout.Width(50), GUILayout.Height(30)))
                        {
                            tempComp.AddWhiteList(name);
                            tempComp.SaveJsonToFile();
                        }

                        GUILayout.Label($"ChildiName:{name}", GUILayout.Width(800),
                            GUILayout.Height(30));

                        EditorGUILayout.EndHorizontal();
                    }

                    pos++;
                }

                EditorGUILayout.EndScrollView();
            }
            else
            {
                scrollPos1 = EditorGUILayout.BeginScrollView(scrollPos1);
                for (var i = 0; i < tempComp.WhiteList.Count; i++)
                {
                    var key = tempComp.WhiteList.Keys.ToList()[i];
                    if (tempComp.WhiteList[key])
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Del", GUILayout.Width(50), GUILayout.Height(30)))
                        {
                            tempComp.DelWhiteList(key);
                            tempComp.SaveJsonToFile();
                        }

                        GUILayout.Label($"白名单：{key}", GUILayout.Width(800), GUILayout.Height(30));
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void SelectGame(GameObject gameObject, string tempName)
        {
            var index = tempName.IndexOf("/", StringComparison.Ordinal);
            var name1 = tempName.Substring(0, index + 1);
            tempName = tempName.Replace(name1, "");
            if (gameObject == null || Selection.activeGameObject == null) return;

            if (gameObject != null)
            {
                var parent = tempComp.GetTopParent(Selection.activeGameObject);
                var rawImage = parent.Find(tempName)?.GetComponent<RawImage>();
                if(rawImage == null) return;
                string rawImagePath = AssetDatabase.GetAssetPath(rawImage.texture);
                if (rawImagePath.IndexOf(tempComp.targetPath) < 0)
                {
                    Selection.activeGameObject = rawImage.gameObject;
                } 
            }
        }

        
    }

    public class CheckRawImageAssetError
    {
        
        private static readonly string WorkFolder = Environment.CurrentDirectory;
        private static readonly string ProjectPath = Application.dataPath;

        public readonly Dictionary<GameObject, List<GameObject>> ItemList = new();
        private readonly string filePath = $"{WorkFolder}/Tools/RawImageWhiteList.json";
        public readonly string targetPath = "Image/bg/";

        public readonly Dictionary<string, bool> WhiteList = new();

        private bool isWindow;

        public CheckRawImageAssetError(bool isWindow)
        {
            this.isWindow = isWindow;
        }

        public void CheckRawImageComponentAsset()
        {
            ItemList.Clear();
            var path = $"{WorkFolder}/Assets/GameAssets/Prefabs"; //预制体路径
            var prefabPaths = GetPrefabPaths(path);
            foreach (var prefabPath in prefabPaths)
            {
                var assetPath = "Assets" + prefabPath.Substring(ProjectPath.Length);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab != null)
                {
                    var nodeNames = CheckRawImage(prefab);
                    if (nodeNames != null && nodeNames.Count > 0)
                        ItemList.Add(prefab, nodeNames);
                }
            }
        }

        public List<string> GetPrefabPaths(string path)
        {
            var prefabPaths = new List<string>();
            var files = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
            foreach (var file in files) prefabPaths.Add(file);
            return prefabPaths;
        }

        public List<GameObject> CheckRawImage(GameObject prefab)
        {
            List<GameObject> tagErrorList = new();

            var allRawComponents = prefab.GetComponentsInChildren<RawImage>(true);

            foreach (var tempComponent in allRawComponents)
            {
                var rawImagePath = AssetDatabase.GetAssetPath(tempComponent.texture);
                if(rawImagePath is null or "") continue;;
                if (rawImagePath.IndexOf(targetPath) < 0)
                {
                    if (isWindow)
                    {
                        tagErrorList.Add(tempComponent.gameObject);
                    }
                    else
                    {
                        var name = GetParentPath(tempComponent.transform);
                        if (IsWhiteList(name)) continue;
                        Debug.LogError($"rawImage 资源用错了 请检查 预制体 {prefab.name}  节点 {tempComponent.gameObject.name}");
                    }
                }
            }
            return tagErrorList;
        }

        //获得子节点完整路径
        public void GetPath(Transform currentTransform, StringBuilder path = null)
        {
            // 如果当前不是根节点，则添加名称并递归父节点
            if (currentTransform.parent != null)
            {
                GetPath(currentTransform.parent, path);
                path?.Append("/").Append(currentTransform.name);
            }
            else // 如果当前是根节点，则只添加自己的名称
            {
                path?.Append(currentTransform.name);
            }
        }

        //获得子节点完整路径
        public Transform GetTopParent(GameObject currentTransform)
        {
            // 如果当前不是根节点，则添加名称并递归父节点
            if (currentTransform.transform.parent != null &&
                currentTransform.transform.parent.name != "Canvas (Environment)")
                return GetTopParent(currentTransform.transform.parent.gameObject);
            // 如果当前是根节点，则只添加自己的名称
            return currentTransform.transform;
        }

        public string GetParentPath(Transform currentTransform)
        {
            var str = new StringBuilder();
            GetPath(currentTransform, str);
            return str.ToString();
        }

        public void AddWhiteList(string path)
        {
            if (!WhiteList.ContainsKey(path))
                WhiteList.Add(path, true);
            else
                WhiteList[path] = true;
        }

        public void DelWhiteList(string path)
        {
            if (WhiteList.ContainsKey(path)) WhiteList[path] = false;
        }

        public bool IsWhiteList(string path)
        {
            if (WhiteList.ContainsKey(path)) return WhiteList[path];

            return false;
        }

        public bool CheckNoneChild(List<GameObject> childList)
        {
            for (var i = 0; i < childList.Count; i++)
            {
                var name = GetParentPath(childList[i].transform);
                if (!WhiteList.ContainsKey(name) ||
                    (WhiteList.ContainsKey(name) && !WhiteList[name])) return true;
            }

            return false;
        }

        /// <summary>
        ///     保存json为文件
        /// </summary>
        /// <param tempName="jsonData"></param>
        /// <param tempName="filePath"></param>
        public void SaveJsonToFile()
        {
            var stringBuilder = new StringBuilder();
            foreach (var item in WhiteList)
                if (item.Value)
                {
                    if (stringBuilder.Length == 0)
                        stringBuilder.Append(item.Key);
                    else
                        stringBuilder.Append(",").Append(item.Key);
                }

            var json = stringBuilder.ToString();
            File.WriteAllText(filePath, json);
        }

        public void GetJsonToFile()
        {
            WhiteList.Clear();
            if (!File.Exists(filePath)) return;
            var json = File.ReadAllText(filePath);
            var NameList = json.Split(',');
            for (var i = 0; i < NameList.Length; i++)
                if (NameList[i] != "")
                    WhiteList.Add(NameList[i], true);
        }
    }
}