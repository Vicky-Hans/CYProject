using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using TMPro;

namespace Editor.Menu
{
    public static class UITools
    {
        private static readonly string WorkFolder = Environment.CurrentDirectory;
        private static readonly string ProjectPath = Application.dataPath;
        
        [MenuItem("Tools/UI/Structure/Floor SelectedTransform Position _%g")]
        public static void FloorSelectedTrans()
        {
            //获取选中的Transform
            GameObject selected = Selection.activeGameObject;
            var component = selected.GetComponent<RectTransform>();
            component.sizeDelta = new Vector2(Mathf.Floor(component.sizeDelta.x), Mathf.Floor(component.sizeDelta.y));
            component.anchoredPosition = new Vector2(Mathf.Floor(component.anchoredPosition.x),
                Mathf.Floor(component.anchoredPosition.y));
        }
        
        [MenuItem("Tools/UI/清理字体缓存")]
        public static void ClearFontCache()
        {
            var obj = Selection.objects.Length > 0 ? Selection.objects[0] : null;
            if (!obj)
            {
                return;
            }

            var asset = obj as TMP_FontAsset;
            if (!asset)
            {
                return;
            }
            
            asset.ClearFontAssetData(true);
        }

        [MenuItem("Tools/UI/Check TMP Tag")]
        public static void CheckTMPComponentTag()
        {
            string path = $"{WorkFolder}/Assets/GameAssets/Prefabs"; //预制体路径
            List<string> prefabPaths = GetPrefabPaths(path);
            foreach (var prefabPath in prefabPaths)
            {
                string assetPath = "Assets" + prefabPath.Substring(ProjectPath.Length);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab != null)
                {
                    CheckTMPTag(prefab);
                }
            }
        }

        private static List<string> GetPrefabPaths(string path)
        {
            List<string> prefabPaths = new List<string>();
            string[] files = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                prefabPaths.Add(file);
            }
            return prefabPaths;
        }

        private static void CheckTMPTag(GameObject prefab)
        {
            TextMeshProUGUI[] tmpUGUIComponents =
                prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmpUGUIComponent in tmpUGUIComponents)
            {
                if (!(tmpUGUIComponent.CompareTag("FontText") ||
                      tmpUGUIComponent.CompareTag("FontNum")))
                {
                    Debug.LogError($"prefab:{prefab.name} tmp:{tmpUGUIComponent.name} didn't set font tag.");
                }
            }

            TextMeshPro[] tmpComponents = prefab.GetComponentsInChildren<TextMeshPro>(true);
            foreach (var tmpComponent in tmpComponents)
            {
                if (!(tmpComponent.CompareTag("FontText") ||
                      tmpComponent.CompareTag("FontNum")))
                {
                    Debug.LogError($"prefab:{prefab.name} tmp:{tmpComponent.name} didn't set font tag.");
                }
            }
        }
    }
}