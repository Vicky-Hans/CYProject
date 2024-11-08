using DH.Tool;
using UnityEditor;
using UnityEngine;

namespace Editor.CommonTools
{
    [InitializeOnLoad]
    public class AutoCheck
    {
        static AutoCheck()
        {
            EditorApplication.update += RunScriptOnCompile;
        }
        private static void RunScriptOnCompile()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                //检查RawImage资源
                CheckRawImageAssetError tempRawImg = new(false);
                tempRawImg.GetJsonToFile();
                tempRawImg.CheckRawImageComponentAsset();
                
                //检查Text标签
                CheckTextTagError tempTextTag = new(false);
                tempTextTag.GetJsonToFile();
                tempTextTag.CheckTMPComponentTag();
                // 检查属性绑定
                CheckPropertyNone tempProperty = new(false);
                tempProperty.CheckAllPrefab();
                
                // 是否引用Editor
                // CheckEditorScriptInEditorMode tempEditor = new();
                // tempEditor.CheckScripts();
                
                Debug.Log("脚本运行完成");
                // 运行完脚本后，移除更新事件
                EditorApplication.update -= RunScriptOnCompile;
            }
        }
    }
}