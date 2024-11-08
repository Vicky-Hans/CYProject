using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor.CommonTools
{
    public class CheckEditorScriptInEditorMode: EditorWindow
    {

        private const int WindowWidth = 400;
        private const int WindowHeight = 600;
        private string functionName;
        private string targetFolderPath = Path.Combine(new string[]{
            "Assets", "Scripts", "Game" });
        private Vector2 scrollPosition;
        
        [MenuItem("DH Tools/检查是否有引用 UnityEditor")]
        public static void ShowWindow()
        {
            
            CheckEditorScriptInEditorMode tempWindow = GetWindow<CheckEditorScriptInEditorMode>();
            tempWindow.position = new Rect(100, 100, WindowWidth, WindowHeight);
        }
        
        void OnGUI()
        {
            GUILayout.Label("Enter the target folder path:");
            targetFolderPath = EditorGUILayout.TextField(targetFolderPath);

            if (GUILayout.Button("Check Scripts"))
            {
                CheckScripts();
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (string scriptPath in foundScripts)
            {
                // MonoScript script = LoadScript(scriptPath);
                EditorGUILayout.ObjectField("Script Object", AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath), typeof(MonoScript), false);
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndScrollView();
        }
        
        private MonoScript LoadScript(string path)
        {
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script != null)
            {
                Debug.Log("Script loaded: " + script.name);
                // 在这里可以对加载的脚本对象进行操作，例如获取类型信息、实例化对象等
                return script;
            }
            
            return null;
        }

        private List<string> foundScripts = new List<string>();

        public void CheckScripts()
        {
            foundScripts.Clear();
            string[] files = Directory.GetFiles(targetFolderPath, "*.cs", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string[] lines = File.ReadAllLines(file);
                foreach (string line in lines)
                {
                    if (line.Contains("using UnityEditor"))
                    {
                        foundScripts.Add(file);
                        break;
                    }
                }
            }
            Repaint();
            foreach (var path in foundScripts)
            {
                Debug.LogError("Found script with reference to UnityEditor: " + path);
            }
        }
    }
}