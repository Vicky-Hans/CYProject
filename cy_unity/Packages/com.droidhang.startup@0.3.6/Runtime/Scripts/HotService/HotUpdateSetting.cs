using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DH.Asset;
using UnityEditor;
using UnityEngine;

namespace DH.HotService
{
    [CreateAssetMenu(menuName = "SettingScriptableObject/HotUpdateSetting",fileName = RESOURCE_PATH)]
    public class HotUpdateSetting : ScriptableObject
    {
        public bool skipHotUpdate = false;
        public const string RESOURCE_PATH = "HotUpdateSetting";

        private static HotUpdateSetting setting;

        /// <summary>
        /// 检查是否需要热更新
        /// </summary>
        /// <returns>true表示需要热更新</returns>
        public static bool CheckNeedHotUpdate()
        {
#if !UNITY_EDITOR
            if (!AssetsManager.UseAssetBundle)
            {
                return false;
            }
#endif
            
            if (!setting)
            {
                setting = Resources.Load<HotUpdateSetting>("HotUpdateSetting");
            }
#if UNITY_EDITOR
            return setting != null && !setting.skipHotUpdate;
#else
            return setting == null || !setting.skipHotUpdate;
#endif
        }

#if UNITY_EDITOR
        public const string HOTUPDATE_SETTING_PATH = "Assets/Resources/HotUpdateSetting.asset";
        
        public static HotUpdateSetting GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<HotUpdateSetting>(HOTUPDATE_SETTING_PATH);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<HotUpdateSetting>();
                FileInfo file = new FileInfo(HOTUPDATE_SETTING_PATH);
                if (!file.Directory.Exists)
                    file.Directory.Create();

                AssetDatabase.CreateAsset(settings, HOTUPDATE_SETTING_PATH);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        [Conditional("UNITY_EDITOR")]
        public void SaveInEditor()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
        
    }
}
