using System;
using UnityEditor;
using UnityEngine;

namespace DH.UIFramework.Editor
{
    public class LuaFileNameWindow : EditorWindow
    {
        private string fileName = "LuaFile";
        public Action<string> OnConfirm;
        void OnGUI()
        {
            GUILayout.Space(10);
            fileName = EditorGUILayout.TextField("文件名称", fileName);
            GUILayout.Space(10);
            if (GUILayout.Button("确认"))
            {
                OnConfirm?.Invoke(fileName);
                this.Close();
            }
        }
    }
}