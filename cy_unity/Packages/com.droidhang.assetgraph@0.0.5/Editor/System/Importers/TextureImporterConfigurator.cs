using UnityEditor;
using System;
using System.Collections.Generic;
using Model = UnityEngine.AssetGraph.DataModel.Version2;

namespace UnityEngine.AssetGraph
{
    [Serializable]
    [CustomAssetImporterConfigurator(typeof(TextureImporter), "Texture", "setting.png")]
    public class TextureImportSettingsConfigurator : IAssetImporterConfigurator
    {
        [SerializeField] private bool m_overwritePackingTag;
        [SerializeField] private bool m_overwriteSpriteSheet;
        [SerializeField] private SerializableMultiTargetString m_customPackingTagTemplate;
        [SerializeField] private bool m_ignoreWrapMode;
        [SerializeField] private bool m_ignorePivotMode;
        [SerializeField] private bool m_setMeshType;

        public void Initialize(ConfigurationOption option)
        {
            m_overwritePackingTag = option.overwritePackingTag;
            m_overwriteSpriteSheet = option.overwriteSpriteSheet;
            m_customPackingTagTemplate = option.customPackingTagTemplate;
            m_ignoreWrapMode = option.ignoreWrapMode;
            m_ignorePivotMode = option.ignorePivotMode;
            m_setMeshType = option.ignorePivotMode;
        }

        public bool IsModified(AssetImporter referenceImporter, AssetImporter importer, BuildTarget target,
            string group)
        {
            var r = referenceImporter as TextureImporter;
            var t = importer as TextureImporter;
            if (t == null || r == null)
            {
                throw new AssetGraphException($"Invalid AssetImporter assigned for {importer.assetPath}");
            }

            return !IsEqual(t, r, GetTagName(target, group));
        }

        public void Configure(AssetImporter referenceImporter, AssetImporter importer, BuildTarget target, string group)
        {
            var r = referenceImporter as TextureImporter;
            var t = importer as TextureImporter;
            if (t == null || r == null)
            {
                throw new AssetGraphException($"Invalid AssetImporter assigned for {importer.assetPath}");
            }

            OverwriteImportSettings(t, r, GetTagName(target, group));
        }

        public void OnInspectorGUI(AssetImporter referenceImporter, BuildTargetGroup target, Action onValueChanged)
        {
            var importer = referenceImporter as TextureImporter;
            if (importer == null)
            {
                return;
            }

            if (importer.textureType == TextureImporterType.Sprite)
            {
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    GUILayout.Label("Sprite Settings");
                    GUILayout.Space(4f);
                    var bSpriteSheet = EditorGUILayout.ToggleLeft("Configure Sprite Mode", m_overwriteSpriteSheet);
                    var bPackingTag = EditorGUILayout.ToggleLeft("Configure Sprite Packing Tag", m_overwritePackingTag);
                    var bIgnorePivotMode = EditorGUILayout.ToggleLeft("Ignore PivotMode", m_ignorePivotMode);
                    bool setMeshType = false;
                    if (!m_overwriteSpriteSheet)
                    {
                        setMeshType = EditorGUILayout.ToggleLeft("Set Mesh Type", m_setMeshType);
                    }

                    if (bSpriteSheet != m_overwriteSpriteSheet ||
                        bPackingTag != m_overwritePackingTag)
                    {
                        m_overwriteSpriteSheet = bSpriteSheet;
                        m_overwritePackingTag = bPackingTag;
                        onValueChanged();
                    }

                    if (m_overwritePackingTag)
                    {
                        if (m_customPackingTagTemplate == null)
                        {
                            m_customPackingTagTemplate = new SerializableMultiTargetString();
                        }

                        var val = m_customPackingTagTemplate.DefaultValue;

                        var newValue = EditorGUILayout.TextField("Packing Tag", val);
                        if (newValue != val)
                        {
                            m_customPackingTagTemplate.DefaultValue = newValue;
                            onValueChanged();
                        }
                    }

                    if (bIgnorePivotMode != m_ignorePivotMode)
                    {
                        m_ignorePivotMode = bIgnorePivotMode;
                        onValueChanged();
                    }
                    
                    if (setMeshType != m_setMeshType)
                    {
                        m_setMeshType = setMeshType;
                        onValueChanged();
                    }

                    EditorGUILayout.HelpBox(
                        "You can configure packing tag name with \"*\" to include group name in your sprite tag.",
                        MessageType.Info);
                }
            }
            else
            {
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    GUILayout.Label("Default Texture Settings");
                    GUILayout.Space(4f);
                    var bIgnoreWrapMode = EditorGUILayout.ToggleLeft("Ignore WarpMode", m_ignoreWrapMode);
                    if (bIgnoreWrapMode != m_ignoreWrapMode)
                    {
                        m_ignoreWrapMode = bIgnoreWrapMode;
                        onValueChanged();
                    }

                    EditorGUILayout.HelpBox(
                        "设置是否忽略WrapMode",
                        MessageType.Info);
                }
            }
        }

        private string GetTagName(BuildTarget target, string groupName)
        {
            return m_customPackingTagTemplate[target].Replace("*", groupName);
        }

        private void ApplySpriteTag(BuildTarget target, IEnumerable<PerformGraph.AssetGroups> incoming)
        {
            foreach (var ag in incoming)
            {
                foreach (var groupKey in ag.assetGroups.Keys)
                {
                    var assets = ag.assetGroups[groupKey];
                    foreach (var asset in assets)
                    {
                        if (asset.importerType == typeof(UnityEditor.TextureImporter))
                        {
                            var importer = AssetImporter.GetAtPath(asset.importFrom) as TextureImporter;

                            importer.spritePackingTag = GetTagName(target, groupKey);
                            importer.SaveAndReimport();
                            asset.TouchImportAsset();
                        }
                    }
                }
            }
        }

        private bool IsEqual(TextureImporter target, TextureImporter reference, string tagName)
        {
            // UnityEditor.TextureImporter.textureFormat' is obsolete: 
            // `textureFormat is not longer accessible at the TextureImporter level
            if (target.textureType != reference.textureType)
                return false;

            TextureImporterSettings targetSetting = new TextureImporterSettings();
            TextureImporterSettings referenceSetting = new TextureImporterSettings();

            target.ReadTextureSettings(targetSetting);
            reference.ReadTextureSettings(referenceSetting);

            // if m_overwriteSpriteSheet is false, following properties
            // should be ignored
            referenceSetting.spriteBorder = targetSetting.spriteBorder;
            if (!m_overwriteSpriteSheet)
            {
                referenceSetting.spriteAlignment = targetSetting.spriteAlignment;
                referenceSetting.spriteExtrude = targetSetting.spriteExtrude;
                referenceSetting.spriteMode = targetSetting.spriteMode;
                if (!m_setMeshType)
                {
                    referenceSetting.spriteMeshType = targetSetting.spriteMeshType;
                }
                referenceSetting.spritePixelsPerUnit = targetSetting.spritePixelsPerUnit;
                referenceSetting.spriteTessellationDetail = targetSetting.spriteTessellationDetail;
            }

            if (m_overwriteSpriteSheet && m_ignorePivotMode)
            {
                referenceSetting.spriteAlignment = targetSetting.spriteAlignment;
                referenceSetting.spritePivot = targetSetting.spritePivot;
            }

            //DHGames Start-----------------------
            if (m_ignoreWrapMode)
            {
                referenceSetting.wrapMode = targetSetting.wrapMode;
                referenceSetting.wrapModeU = targetSetting.wrapModeU;
                referenceSetting.wrapModeV = targetSetting.wrapModeV;
                referenceSetting.wrapModeW = targetSetting.wrapModeW;
            }
            //DHGames End-----------------------

            if (!TextureImporterSettings.Equal(targetSetting, referenceSetting))
            {
                return false;
            }

            if (target.textureType == TextureImporterType.Sprite)
            {
                if (m_overwritePackingTag)
                {
                    if (!string.IsNullOrEmpty(tagName))
                    {
                        if (target.spritePackingTag != tagName)
                            return false;
                    }
                    else
                    {
                        if (target.spritePackingTag != reference.spritePackingTag)
                            return false;
                    }
                }

                if (m_overwriteSpriteSheet)
                {
                    if (target.spriteImportMode != reference.spriteImportMode)
                        return false;
                    if (!m_ignorePivotMode && target.spritePivot != reference.spritePivot)
                        return false;
                    if (target.spritePixelsPerUnit != reference.spritePixelsPerUnit)
                        return false;

                    var s1 = target.spritesheet;
                    var s2 = reference.spritesheet;

                    if (s1.Length != s2.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < s1.Length; ++i)
                    {
                        if (s1[i].alignment != s2[i].alignment)
                            return false;
                        if (s1[i].border != s2[i].border)
                            return false;
                        if (s1[i].name != s2[i].name)
                            return false;
                        if (s1[i].pivot != s2[i].pivot)
                            return false;
                        if (s1[i].rect != s2[i].rect)
                            return false;
                    }
                }
            }

            if (target.allowAlphaSplitting != reference.allowAlphaSplitting)
                return false;
            if (target.alphaIsTransparency != reference.alphaIsTransparency)
                return false;
            if (target.alphaSource != reference.alphaSource)
                return false;
            if (target.alphaTestReferenceValue != reference.alphaTestReferenceValue)
                return false;
            if (target.androidETC2FallbackOverride != reference.androidETC2FallbackOverride)
                return false;
            if (target.anisoLevel != reference.anisoLevel)
                return false;
            if (target.borderMipmap != reference.borderMipmap)
                return false;
            if (target.compressionQuality != reference.compressionQuality)
                return false;
            if (target.convertToNormalmap != reference.convertToNormalmap)
                return false;
            if (target.crunchedCompression != reference.crunchedCompression)
                return false;
            if (target.fadeout != reference.fadeout)
                return false;
            if (target.filterMode != reference.filterMode)
                return false;
            if (target.generateCubemap != reference.generateCubemap)
                return false;
            if (target.heightmapScale != reference.heightmapScale)
                return false;
            if (target.isReadable != reference.isReadable)
                return false;
            if (target.maxTextureSize != reference.maxTextureSize)
                return false;
            if (target.mipMapBias != reference.mipMapBias)
                return false;
            if (target.mipmapEnabled != reference.mipmapEnabled)
                return false;
            if (target.mipmapFadeDistanceEnd != reference.mipmapFadeDistanceEnd)
                return false;
            if (target.mipmapFadeDistanceStart != reference.mipmapFadeDistanceStart)
                return false;
            if (target.mipmapFilter != reference.mipmapFilter)
                return false;
            if (target.mipMapsPreserveCoverage != reference.mipMapsPreserveCoverage)
                return false;
            if (target.normalmapFilter != reference.normalmapFilter)
                return false;
            if (target.npotScale != reference.npotScale)
                return false;

            if (target.sRGBTexture != reference.sRGBTexture)
                return false;
            if (target.streamingMipmaps != reference.streamingMipmaps)
                return false;
            if (target.streamingMipmapsPriority != reference.streamingMipmapsPriority)
                return false;
            if (target.textureCompression != reference.textureCompression)
                return false;
            if (target.textureShape != reference.textureShape)
                return false;

            //DHGames Start-----------------------
            if (!m_ignoreWrapMode)
            {
                //DHGames End-----------------------
                if (target.wrapMode != reference.wrapMode)
                    return false;
                if (target.wrapModeU != reference.wrapModeU)
                    return false;
                if (target.wrapModeV != reference.wrapModeV)
                    return false;
                if (target.wrapModeW != reference.wrapModeW)
                    return false;
                //DHGames Start-----------------------
            }
            //DHGames End-----------------------


            var refDefault = reference.GetDefaultPlatformTextureSettings();
            var impDefault = target.GetDefaultPlatformTextureSettings();
            if (!CompareImporterPlatformSettings(refDefault, impDefault))
                return false;

            foreach (var g in NodeGUIUtility.SupportedBuildTargetGroups)
            {
                var platformName =
                    BuildTargetUtility.TargetToAssetBundlePlatformName(g,
                        BuildTargetUtility.PlatformNameType.TextureImporter);

                var impSet = reference.GetPlatformTextureSettings(platformName);
                var targetImpSet = target.GetPlatformTextureSettings(platformName);
                if (!CompareImporterPlatformSettings(impSet, targetImpSet))
                    return false;
            }


            return true;
        }

        private void OverwriteImportSettings(TextureImporter target, TextureImporter reference, string tagName)
        {
            target.textureType = reference.textureType;

            var dstSettings = new TextureImporterSettings();
            var srcSettings = new TextureImporterSettings();

            target.ReadTextureSettings(srcSettings);
            reference.ReadTextureSettings(dstSettings);

            dstSettings.spriteBorder = srcSettings.spriteBorder;
            if (!m_overwriteSpriteSheet)
            {
                dstSettings.spriteAlignment = srcSettings.spriteAlignment;
                dstSettings.spriteExtrude = srcSettings.spriteExtrude;
                dstSettings.spriteMode = srcSettings.spriteMode;
                if (!m_setMeshType)
                {
                    dstSettings.spriteMeshType = srcSettings.spriteMeshType;
                }
                dstSettings.spritePixelsPerUnit = srcSettings.spritePixelsPerUnit;
                dstSettings.spriteTessellationDetail = srcSettings.spriteTessellationDetail;
            }

            if (m_overwriteSpriteSheet && m_ignorePivotMode)
            {
                dstSettings.spritePivot = srcSettings.spritePivot;
                dstSettings.spriteAlignment = srcSettings.spriteAlignment;
            }
            
            target.SetTextureSettings(dstSettings);

            if (m_overwriteSpriteSheet)
            {
                target.spritesheet = reference.spritesheet;
            }

            // some unity version do not properly copy properties via TextureSettings,
            // so also perform manual copy
            target.allowAlphaSplitting = reference.allowAlphaSplitting;
            target.alphaIsTransparency = reference.alphaIsTransparency;
            target.alphaSource = reference.alphaSource;
            target.alphaTestReferenceValue = reference.alphaTestReferenceValue;
            target.androidETC2FallbackOverride = reference.androidETC2FallbackOverride;
            target.anisoLevel = reference.anisoLevel;
            target.borderMipmap = reference.borderMipmap;
            target.compressionQuality = reference.compressionQuality;
            target.convertToNormalmap = reference.convertToNormalmap;
            target.crunchedCompression = reference.crunchedCompression;
            target.fadeout = reference.fadeout;
            target.filterMode = reference.filterMode;
            target.generateCubemap = reference.generateCubemap;
            target.heightmapScale = reference.heightmapScale;
            target.isReadable = reference.isReadable;
            target.maxTextureSize = reference.maxTextureSize;
            target.mipMapBias = reference.mipMapBias;
            target.mipmapEnabled = reference.mipmapEnabled;
            target.mipmapFadeDistanceEnd = reference.mipmapFadeDistanceEnd;
            target.mipmapFadeDistanceStart = reference.mipmapFadeDistanceStart;
            target.mipmapFilter = reference.mipmapFilter;
            target.mipMapsPreserveCoverage = reference.mipMapsPreserveCoverage;
            target.normalmapFilter = reference.normalmapFilter;
            target.npotScale = reference.npotScale;
            target.sRGBTexture = reference.sRGBTexture;
            target.streamingMipmaps = reference.streamingMipmaps;
            target.streamingMipmapsPriority = reference.streamingMipmapsPriority;
            target.textureCompression = reference.textureCompression;
            target.textureShape = reference.textureShape;
            //DHGames Start-----------------------
            if (!m_ignoreWrapMode)
            {
                //DHGames End-----------------------
                target.wrapMode = reference.wrapMode;
                target.wrapModeU = reference.wrapModeU;
                target.wrapModeV = reference.wrapModeV;
                target.wrapModeW = reference.wrapModeW;
                //DHGames Start-----------------------
            }
            //DHGames End-----------------------

            if (m_overwritePackingTag)
            {
                if (!string.IsNullOrEmpty(tagName))
                {
                    target.spritePackingTag = tagName;
                }
                else
                {
                    target.spritePackingTag = reference.spritePackingTag;
                }
            }

            var defaultPlatformSetting = reference.GetDefaultPlatformTextureSettings();
            target.SetPlatformTextureSettings(defaultPlatformSetting);

            foreach (var g in NodeGUIUtility.SupportedBuildTargetGroups)
            {
                var platformName =
                    BuildTargetUtility.TargetToAssetBundlePlatformName(g,
                        BuildTargetUtility.PlatformNameType.TextureImporter);
                var impSet = reference.GetPlatformTextureSettings(platformName);
                target.SetPlatformTextureSettings(impSet);
            }
        }

        bool CompareImporterPlatformSettings(TextureImporterPlatformSettings c1, TextureImporterPlatformSettings c2)
        {
            if (c1.allowsAlphaSplitting != c2.allowsAlphaSplitting)
                return false;
            if (c1.androidETC2FallbackOverride != c2.androidETC2FallbackOverride)
                return false;
            if (c1.compressionQuality != c2.compressionQuality)
                return false;
            if (c1.crunchedCompression != c2.crunchedCompression)
                return false;
            if (c1.format != c2.format)
                return false;
            if (c1.maxTextureSize != c2.maxTextureSize)
                return false;
            if (c1.name != c2.name)
                return false;
            if (c1.overridden != c2.overridden)
                return false;
            if (c1.resizeAlgorithm != c2.resizeAlgorithm)
                return false;
            if (c1.textureCompression != c2.textureCompression)
                return false;

            return true;
        }
    }
}