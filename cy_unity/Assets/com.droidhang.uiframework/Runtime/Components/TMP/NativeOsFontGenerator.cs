using System.Diagnostics;
using DH.NativeCore.MonoSingleton;
using DHFramework;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.TextCore.LowLevel;
using Debug = UnityEngine.Debug;

namespace DH.UIFramework
{
    public class NativeOsFontGenerator : Singleton<NativeOsFontGenerator>
    {
        public class ConfigDataItem
        {
            public string familyName;
            public string styleName;
        }
        
        public class ConfigData
        {
            public ConfigDataItem[] fontsName;
        }
        
        //the font to add the fallback to.
        private TMP_FontAsset fontAsset = null;
        private string[] allFontsPaths;

        protected override void Initialization()
        {
            // 调用一下避免被IL2Cpp裁剪掉
            UpdateFontConfig(null,null);
            allFontsPaths = Font.GetPathsToOSFonts();
        }

        public void UpdateFontConfig(string configJson,TMP_FontAsset font)
        {
            if (string.IsNullOrEmpty(configJson))
            {
                return;
            }
            
            var config = DHUtility.Json.ToObject<ConfigData>(configJson);
            if (config.fontsName == null || config.fontsName.Length == 0)
            {
                return;
            }
            
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            fontAsset = font;
            fontAsset.fallbackFontAssetTable.Clear();
            for (int i = 0; i < config.fontsName.Length; i++)
            {
                AddFallback(ref fontAsset, allFontsPaths, config.fontsName[i].familyName,config.fontsName[i].styleName);
            }
            fontAsset.ReadFontAssetDefinition();
            stopwatch.Stop();
            Debug.Log($"Generate OS font asset used time {stopwatch.ElapsedMilliseconds}ms");
        }

         public void FallbackClear(TMP_FontAsset FontAsset)
        {
            if (FontAsset)
            {
                FontAsset.fallbackFontAssetTable.Clear();
            }
        }
        private void OnDestroy()
        {
            if (fontAsset)
            {
                fontAsset.fallbackFontAssetTable.Clear();
            }
        }
        
        private bool AddFallback(ref TMP_FontAsset rootFontAsset, string[] allFontsPaths, string familyName,string styleName)
        {
            string path = null;
            familyName = familyName.ToLowerInvariant();
            styleName = styleName.ToLowerInvariant();
            
            for (int i = 0; i < allFontsPaths.Length; i++)
            {
                var fontPath = allFontsPaths[i].ToLowerInvariant();
                if (!fontPath.Contains(familyName.ToLowerInvariant()))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(styleName) && !fontPath.Contains(styleName))
                {
                    continue;
                }
                
                
                path = allFontsPaths[i];
                break;
            }

            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            Font osFont = new Font(path);
            int atlasSize = rootFontAsset.fallbackFontAssetTable.Count == 0 ? 1024 : 512;
            TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(osFont,24,1,GlyphRenderMode.SDFAA,atlasSize,atlasSize);
            if (rootFontAsset)
            {
                rootFontAsset.fallbackFontAssetTable.Add(asset);
            }
            else
            {
                rootFontAsset = asset;
            }

            return true;
        }

        [ContextMenu("ClearCache")]
        private void ClearCache()
        {
            if (fontAsset)
            {
                fontAsset.ClearFontAssetData();
            }
        }
    }
}