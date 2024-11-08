using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DH.Tool
{
    public class FindComponentAsset : EditorWindow
    {
        
        private MonoScript targetScript;
        private List<GameObject> prefabsWithScript = new List<GameObject>();
        
        [MenuItem("DH Tools/查找预制体使用指定脚本")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(FindComponentAsset));
        }

        private void OnGUI()
        {
            GUILayout.Label("查找预制体使用指定脚本", EditorStyles.boldLabel);

            targetScript = EditorGUILayout.ObjectField("查找预制体使用指定脚本", targetScript, typeof(MonoScript), false) as MonoScript;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("查找"))
            {
                FindPrefabsWithScript();
            }

            if ( GUILayout.Button("选中节点"))
            {
                SelectNodesWithScript();
            }

            EditorGUILayout.EndHorizontal();
           
            EditorGUILayout.BeginVertical();

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Prefabs with Script:", EditorStyles.boldLabel);

            foreach (GameObject prefab in prefabsWithScript)
            {
                EditorGUILayout.ObjectField(prefab, typeof(GameObject), false);
            }
            EditorGUILayout.EndVertical();
        }

        private void FindPrefabsWithScript()
        {
            if (targetScript == null)
            {
                Debug.LogWarning("Target Script is not assigned.");
                return;
            }

            prefabsWithScript.Clear();

            string[] assetGuids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string assetGuid in assetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
                if (prefab != null && PrefabContainsScript(prefab, targetScript))
                {
                    prefabsWithScript.Add(prefab);
                }
            }

            Repaint();
        }

        private bool PrefabContainsScript(GameObject prefab, MonoScript script)
        {
            MonoBehaviour[] components = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour component in components)
            {
                if(component == null) continue;
                if (component.GetType() == script.GetClass())
                {
                    return true;
                }
            }
            return false;
        }
        private void SelectNodesWithScript()
        {
            if (targetScript == null)
            {
                Debug.LogWarning("Target Script is not assigned.");
                return;
            }

            GameObject[] selectedGameObjects = Selection.gameObjects;
            List<Object> selectedObjects = new List<Object>();

            foreach (GameObject selectedObject in selectedGameObjects)
            {
                MonoBehaviour[] components = selectedObject.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (MonoBehaviour component in components)
                {
                    if (component.GetType() == targetScript.GetClass())
                    {
                        selectedObjects.Add(component.gameObject);
                    }
                }
            }

            if (selectedObjects.Count > 0)
            {
                Selection.objects = selectedObjects.ToArray();
            }
            else
            {
                Debug.Log("No nodes with the target script found in the selected prefab.");
            }
        }
        
    }
}