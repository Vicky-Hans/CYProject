using System;
using System.Collections.Generic;
using DHFramework;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.TextCore.Text;
using AtlasPopulationMode = TMPro.AtlasPopulationMode;

namespace DH.UIFramework
{
    public class TMP_SystemFontAsset : MonoBehaviour
    {
        public class FontItem
        {
            public string familyName;
            public string styleName;
            public bool invalid;
        }
        public TMP_FontAsset fontAsset;
        public bool usePresetFont;

        private List<FontItem> fontItems = new List<FontItem>()
        {
            new FontItem() {familyName = "NotoSansSC", styleName = "Regular"},
            new FontItem() {familyName = "Noto Sans SC", styleName = "Regular"},
            new FontItem() {familyName = "NotoSansJP", styleName = "Regular"},
            new FontItem() {familyName = "Noto Sans JP", styleName = "Regular"},
            new FontItem() {familyName = "NotoSansKR", styleName = "Regular"},
            new FontItem() {familyName = "Noto Sans KR", styleName = "Regular"},
            new FontItem() {familyName = "NotoSansThai", styleName = "Regular"},
            new FontItem() {familyName = "Noto Sans Thai", styleName = "Regular"},
            new FontItem() {familyName = "NotoSansMyanmar", styleName = "Regular"}, //缅甸
            new FontItem() {familyName = "Noto Sans Myanmar", styleName = "Regular"}, //缅甸
            new FontItem() {familyName = "Core", styleName = "AppleSDGothicNeo"}, //ios韩文
            new FontItem() {familyName = "LanguageSupport", styleName = "Thonburi"}, //ios泰语
            new FontItem() {familyName = "Core", styleName = "NotoSansMyanmar"}, //ios缅甸
            new FontItem() {familyName = "Microsoft YaHei", styleName = "Regular"}, //windows
            new FontItem() {familyName = "Fonts", styleName = "AppleSDGothicNeo"}, //editor 韩文等
            new FontItem() {familyName = "Malgun Gothic", styleName = "Regular"}, //windows
            new FontItem() {familyName = "Leelawadee UI", styleName = "Regular"}, //windows
            new FontItem() {familyName = "Myanmar Text", styleName = "Regular"}, //windows
        };

        [ContextMenu("ClearFont")]
        private void ClearFont()
        {
            if (!fontAsset)
            {
                return;
            }
            
            fontAsset.ClearFontAssetData(true);
        }
        
        private void Start()
        {
            try
            {
                fontAsset.getGlyphFunc = GetGlyphIndex;
            }
            catch (Exception e)
            {
               DHLog.Debug(e.ToString());
            }
           
#if UNITY_EDITOR
            fontAsset.fallbackFontAssetTable.Clear();
            fontAsset.ClearFontAssetData(true);
#endif
        }

        private void OnDestroy()
        {
            fontAsset.fallbackFontAssetTable.Clear();
#if UNITY_EDITOR
            fontAsset.ClearFontAssetData(true);
#endif
        }

        public uint GetSingleGlyphIndex(FontItem fontItem, uint unicode, out FontEngineError error)
        {
            uint glyphIndex = 0;
            error = FontEngine.LoadFontFace(fontItem.familyName,fontItem.styleName, fontAsset.faceInfo.pointSize);
            if (error == FontEngineError.Success && FontEngine.TryGetGlyphIndex(unicode, out glyphIndex))
            {
                DHLog.Debug($"Load font {fontItem.familyName} {fontItem.styleName} success");
                return glyphIndex;
            }

            return glyphIndex;
        }
        
        public uint GetGlyphIndex(uint unicode, out FontEngineError error)
        {
            uint glyphIndex = 0;
            if (usePresetFont)
            {
                glyphIndex = fontAsset.GetGlyphIndex(unicode, out error);
                if (glyphIndex != 0)
                {
                    return glyphIndex;
                }

                // 预先检查是否在Fallback字体中包含该文字，若包含则不需要创建该字体
                foreach (var item in fontAsset.fallbackFontAssetTable)
                {
                    if (item.characterLookupTable.ContainsKey(unicode))
                    {
                        return 0;
                    }
                    
                    if (item.GetGlyphIndex(unicode, out error) != 0)
                    {
                        return 0;
                    }
                }
            }

            error = FontEngineError.Success;
            var loadedFontIndex = 0;
            while (loadedFontIndex < fontItems.Count)
            {
                var item = fontItems[loadedFontIndex];
                loadedFontIndex++;
                if (item.invalid)
                {
                    continue;
                }

                error = FontEngine.LoadFontFace(item.familyName, item.styleName, fontAsset.faceInfo.pointSize);
                if (error == FontEngineError.Success && FontEngine.TryGetGlyphIndex(unicode, out glyphIndex))
                {
                    var tempItem = item;
                    var newFont = TMP_FontAsset.CreateFontAsset(fontAsset.sourceFontFile, fontAsset.faceInfo.pointSize,
                        fontAsset.atlasPadding, fontAsset.atlasRenderMode, 512, 512, AtlasPopulationMode.Dynamic, true);
                    newFont.getGlyphFunc = (uint code, out FontEngineError error) =>
                        GetSingleGlyphIndex(tempItem, code, out error);
                    fontAsset.fallbackFontAssetTable.Add(newFont);
                    item.invalid = true;
                    return 0;
                }
                else if(error != FontEngineError.Success)
                {
                    item.invalid = true;
                    DHLog.Debug($"Load font {item.familyName} {item.styleName} failed with error {error}");
                }
            }

            return glyphIndex;
        }
    }
}