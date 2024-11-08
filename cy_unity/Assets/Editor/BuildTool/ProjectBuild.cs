      
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DH.Asset;
using DH.Asset.Editor;
using DHEditor.Toolset;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Player;
using UnityEngine;
using UnityEngine.AssetGraph;
using Debug = UnityEngine.Debug;
using Model = UnityEngine.AssetGraph.DataModel.Version2;
using System.Reflection;
using System.Text;
using DH.Editor;
using DHFramework;
using HybridCLR.Editor.Installer;
using NUnit.Framework;
using TMPro;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build;
using UnityEditor.Build.Pipeline;
using UnityEngine.ResourceManagement.Util;
using AddressableAssetGroup = UnityEditor.AddressableAssets.Settings.AddressableAssetGroup;

namespace DH.Tool
{
    public static class ProjectBuild
    {
        private class AssetBatchEditor : IDisposable
        {
            public AssetBatchEditor()
            {
                AssetDatabase.StartAssetEditing();
            }

            public void Dispose()
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        public static readonly string DllBuildPath =
            $"{Application.dataPath.Replace("Assets", null)}/DllBuild";
        
        private static readonly string LogTag = "[dhinfo] [ProjectBuild] ";
        private static readonly string ErrorLogTag = "[dherror] [ProjectBuild] ";

        private static readonly string LocalBuildPathKey = "LocalBuildPath";
        private static readonly string LocalLoadPathKey = "LocalLoadPath";
        /// <summary>
        /// Asset Graph 分组配置根目录
        /// </summary>
        private static readonly string GroupGraphRoot = "Assets/AssetGraph/Flow/Import/Manually";

        private static void Info(string description)
        {
            Debug.Log($"{LogTag} {description}");
        }

        private static void Error(string description)
        {
            Debug.Log($"{ErrorLogTag} {description}");
        }
        
        private static void PrepareDirectory(string dirName)
        {
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
        }

        private static void CompileDll(string buildDir, BuildTarget target)
        {
            var group = BuildPipeline.GetBuildTargetGroup(target);

            ScriptCompilationSettings scriptCompilationSettings = new ScriptCompilationSettings();
            scriptCompilationSettings.group = group;
            scriptCompilationSettings.target = target;
            PrepareDirectory(buildDir);
            ScriptCompilationResult scriptCompilationResult = PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, buildDir);
            foreach (var ass in scriptCompilationResult.assemblies)
            {
                Info($"compile assemblies:{ass}");
            }
        }
        
        [MenuItem("Tools/构建/PrepareAssets")]
        private static void PrepareAssets()
        {
            EditorUserBuildSettings.buildAppBundle = true;
            DHHybridCLR.Editor.BuildPipeline.BuildPipeline.CheckHCLRInstall();
            
            if (Directory.Exists(DHAssetsConfig.ReadOnlyPath))
            {
                Directory.Delete(DHAssetsConfig.ReadOnlyPath, true);
            }

            bool isReleaseMode = IsReleaseMode();
            EditorUserBuildSettings.development = !isReleaseMode;
            if (ExecutionEnvironment.InBatchMode)
            {
                var targetGroup = BuildTargetGroup.Unknown;
#if UNITY_ANDROID
                targetGroup = BuildTargetGroup.Android;
#elif UNITY_IOS
                targetGroup = BuildTargetGroup.iOS;
#endif
                if (targetGroup != BuildTargetGroup.Unknown)
                {
                    if (!isReleaseMode)
                    {
                        ExportExcelTools.Execute();
                    }
                    var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
                    var symbolArray = new HashSet<string>(defineSymbols.Split(';'));
                    if (!isReleaseMode)
                    {
                        symbolArray.Add("MANAGED_DEVICEINFO");
                        symbolArray.Add("DEBUG");
                        symbolArray.Add("DH_DEBUG");
                    }
                    else
                    {
                        symbolArray.Remove("DEBUG");
                        symbolArray.Remove("DH_DEBUG");
                        symbolArray.Remove("MANAGED_DEVICEINFO");
                    }
    
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(';',symbolArray));
                }
            }

            SetGameConfig(IsReleaseOrVsnMode());

            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC2;
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 用于项目组初始化构建目录和AssetBundle输出目录
        /// 设置好后需要对AssetGroupTemplates
        /// </summary>
        private static void SetProfile()
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            var names = settings.profileSettings.GetVariableNames();
            if (names.Contains(LocalBuildPathKey))
            {
                settings.profileSettings.SetValue(settings.activeProfileId,LocalBuildPathKey,"[DH.Asset.Editor.DroidHangAddressableConfig.LocalBuildPath]");
            }
            else
            {
                settings.profileSettings.CreateValue(LocalBuildPathKey,"[DH.Asset.Editor.DroidHangAddressableConfig.LocalBuildPath]");
            }

            if (names.Contains(LocalLoadPathKey))
            {
                settings.profileSettings.SetValue(settings.activeProfileId,LocalLoadPathKey,"{DH.Asset.DHAssetsConfig.RuntimeDataPath}");
            }
            else
            {
                settings.profileSettings.CreateValue(LocalLoadPathKey,"{DH.Asset.DHAssetsConfig.RuntimeDataPath}");
            }
        }
        
        private static void PrepareAddressableSetting()
        {
            SetProfile();
            
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            using (new TaskBenchmark("Addressable分组"))
            {
                var fileInfos = Directory.GetFiles(GroupGraphRoot, "*.asset",
                    SearchOption.TopDirectoryOnly);
                foreach (var file in fileInfos)
                {
                    Model.ConfigGraph configGraph = AssetDatabase.LoadAssetAtPath<Model.ConfigGraph>(file);
                    if (configGraph)
                    {
                        AssetGraphUtility.ExecuteGraph(configGraph);
                    }
                }

                foreach (AddressableAssetGroup assetGroup in settings.groups)
                {
                    var schema = assetGroup.GetSchema<BundledAssetGroupSchema>();
                    if (schema == null)
                    {
                        continue;
                    }
                    schema.BuildPath.SetVariableByName( settings,LocalBuildPathKey);
                    schema.LoadPath.SetVariableByName( settings,LocalLoadPathKey);
                }
            }

           using (new TaskBenchmark("冗余依赖分析"))
           {
               var rule = new CheckBundleDupeDependenciesV2();
               try
               {
                   rule.CheckUISprite += RuleOnCheckUISprite;
                   rule.ClearGroupData(settings);
                   rule.ClearAnalysis();
                   rule.RefreshAnalysis(settings);
                   rule.FixIssues(settings);
               }
               finally
               {
                   rule.CheckUISprite -= RuleOnCheckUISprite;
               }


               SetDupGroup(settings,CheckBundleDupeDependenciesV2.GroupName);
           }

            using (new TaskBenchmark("移除未使用的Group"))
            { 
                AddressableHelper.RemoveMissingGroupReferences(settings);
            }

            using (new TaskBenchmark("移除未使用的Label"))
            {
                HashSet<string> usedLabels = new HashSet<string>();
                foreach (AddressableAssetGroup assetGroup in settings.groups)
                {
                    if (assetGroup.entries == null)
                    {
                        continue;
                    }
                    
                    foreach (AddressableAssetEntry entry in assetGroup.entries)
                    {
                        foreach (string label in entry.labels)
                            usedLabels.Add(label);
                    }
                }

                HashSet<string> unused = new HashSet<string>(settings.GetLabels());
                unused.RemoveWhere(l => usedLabels.Contains(l));
                if (unused.Count > 0)
                {
                    foreach (string s in unused)
                       settings.RemoveLabel(s, false);
                }
            }
        }
        
        private static bool RuleOnCheckUISprite(string assetPath)
        {
            return assetPath.Contains("UI/Image", StringComparison.Ordinal) && assetPath.EndsWith(".png",StringComparison.Ordinal);
        }
        
        [MenuItem("Tools/构建/清除UIReference的资源")]
        private static void ClearUIReferenceRes()
        {
            using (new TaskBenchmark("清除UIReference的资源"))
            {
                FieldInfo editorFontRef = typeof(TMP_FontAsset).GetField("m_SourceFontFile_EditorRef", BindingFlags.NonPublic | BindingFlags.Instance);
                
                var basePath = Path.GetFullPath(".") + "/";
            
                //处理拼UI时引用的字体
                var fontAssetsPath = Directory.GetFiles(Path.Combine(Application.dataPath, "Editor/DevelopFolder/DevTemplate/UIFonts"),
                    "*.asset", SearchOption.AllDirectories);

                foreach (var path in fontAssetsPath)
                {
                    var filePath = path.Replace(basePath, string.Empty);
                    filePath = DHUtility.Path.GetRegularPath(filePath);
                
                    TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(filePath);
                    if (fontAsset)
                    {
                        if (editorFontRef != null)
                        {
                            editorFontRef.SetValue(fontAsset, null);
                        }
                        
                        fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;
                        fontAsset.ClearFontAssetData(true);
                    }
                }
            
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private static string[] GetBuildScenes()
        {
            List<string> scenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled)
                {
                    continue;
                }

                scenes.Add(scene.path);
            }

            return scenes.ToArray();
        }

        /// <summary>
        /// 导出AndroidStudio工程或者Xcode工程
        /// </summary>
        private static void ExportProject()
        {
            EditorUserBuildSettings.development = !IsReleaseMode();
            var outputPath = ExecutionEnvironment.InBatchMode ? ExecutionEnvironment.NeoBuildDir : 
                Path.Combine(Path.GetFullPath("."),"LocalBuild");
            var buildMethod = ExecutionEnvironment.InBatchMode ? BatchBuildTools.BuildMethod.Neo : BatchBuildTools.BuildMethod.Local;
            DHHybridCLR.Editor.BuildPipeline.BuildPipeline.PrepareFcg();
#if UNITY_2022_1_OR_NEWER
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup),Il2CppCodeGeneration.OptimizeSpeed);
#else
            EditorUserBuildSettings.il2CppCodeGeneration = Il2CppCodeGeneration.OptimizeSpeed;
#endif
#if UNITY_ANDROID
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                scenes = GetBuildScenes(),
                locationPathName = outputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = IsReleaseMode() ? BuildOptions.None : BuildOptions.Development,
            };
            options.options |= BuildOptions.CompressWithLz4HC;
            using (new TaskBenchmark("导出AndroidStudio工程"))
            {
                BatchBuildTools.AndroidBatchBuild(buildMethod, options);
            }
#elif UNITY_IOS
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                scenes = GetBuildScenes(),
                locationPathName = outputPath,
                options = IsReleaseMode() ? BuildOptions.None : BuildOptions.Development,
                target = BuildTarget.iOS,
                targetGroup = BuildTargetGroup.iOS,
            };
            using (var bench = new TaskBenchmark("导出Xcode工程"))
            {
                BatchBuildTools.IosBatchBuild(buildMethod, options);
            }
#endif
        }

        [MenuItem("Tools/构建/编译链接库")]
        public static void CompileCurrentTargetDll()
        {
            CompileDll(DllBuildPath,EditorUserBuildSettings.activeBuildTarget);
        }

        [MenuItem("Tools/构建/刷新Addressable Group")]
        public static void RefreshAssetGroup()
        {
            PrepareAddressableSetting();
        }

        /// <summary>
        /// 构建整包
        /// </summary>
        [MenuItem("Tools/构建/构建整包")]
        public static void BuildFull()
        {
            if (DHHybridCLR.Editor.BuildPipeline.BuildPipeline.IsEnableHybridCLR())
            {
                var controller = new InstallerController();
                if (!controller.HasInstalledHybridCLR())
                {
                    Debug.Log($"[dhinfo][BuildPipeline::CheckHCLRInstall] Start Install");
                    controller.InstallDefaultHybridCLR();
                }
            }
            EditorUserBuildSettings.development = !IsReleaseMode();
            Debug.Log($"[dhinfo]EditorUserBuildSettings.development {EditorUserBuildSettings.development}");
            BuildAssets(true);
            ExportProject();
        }

        /// <summary>
        /// 构建AssetBundle
        /// </summary>
        [MenuItem("Tools/构建/构建热更新资源")]
        public static void BuildAssetBundle()
        {
            BuildAssets(false);
        }
        
        public static void BuildAssets(bool buildFullApk)
        {
            PrepareAssets();

            PrepareAddressableSetting();

            ClearUIReferenceRes();

            var content = new DroidHangAddressableBuildContent();
            // Build addressable
            AddressableAssetSettingsDefaultObject.Settings = content.Settings;
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            for(int index = 0;index < settings.DataBuilders.Count;index++)
            {
                if (settings.DataBuilders[index] is CustomBuildScriptPackedMode)
                {
                    settings.ActivePlayerDataBuilderIndex = index;
                    break;
                }
            }

            if (Directory.Exists(DroidHangAddressableConfig.LocalCachePath))
            {
                Directory.Delete(DroidHangAddressableConfig.LocalCachePath, true);
            }
            
            using (new TaskBenchmark("SBP构建AssetBundle"))
            {
                AddressableAssetSettings.BuildPlayerContent(out var result);
                if (!string.IsNullOrEmpty(result.Error))
                {
                    Error($"构建AssetBundle失败，失败原因:{result.Error}，路径：{result.Duration}, outPath：{result.OutputPath}");
                    ExecutionEnvironment.EditorApplicationExit(-1);
                    return;
                }
            }

            using (new TaskBenchmark("AssetBundle后处理"))
            {
                DHHybridCLR.Editor.BuildPipeline.BuildPipeline.BeforeBuild();
                content.PostProcessContent(null,buildFullApk);
            }
        }

        private static bool IsReleaseMode()
        {
            string mode = ExecutionEnvironment.GetCommandLineParam("-mode");
            return mode.Contains("release");
        }

        private static bool IsReleaseOrVsnMode()
        {
            string mode = ExecutionEnvironment.GetCommandLineParam("-mode");
            return mode.Contains("release") || mode.Contains("vsn");
        }
        
        /// <summary>
        /// 创建或修改游戏gameconfig文件
        /// </summary>
        static void SetGameConfig(bool enableRelease)
        {
            var configAsset = Resources.Load<StartupLauncherConfig>("StartupLauncherConfig");
            if (configAsset == null)
            {
                Debug.LogError("StartupLauncherConfig 未找到");
                return;
            }

            configAsset.EnableRelease = enableRelease;

            Debug.Log($"[ProjectBuild:SetGameConfig] 游戏配置文件: EnableRelease: {configAsset.EnableRelease}");
            EditorUtility.SetDirty(configAsset);
            AssetDatabase.SaveAssets();
        }

        private static void SetDupGroup(AddressableAssetSettings aaSettings, string groupName,bool includeAddress = false)
        {
            var group = aaSettings.FindGroup(groupName);
            if (!group)
                group = aaSettings.CreateGroup(groupName, false, false, false, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));

            foreach (var schema in group.Schemas)
            {
                var assetSchema = schema as BundledAssetGroupSchema;
                if (assetSchema != null)
                {
                    assetSchema.IncludeInBuild = true;
                    assetSchema.UseAssetBundleCrc = false;
                    assetSchema.IncludeAddressInCatalog = includeAddress;
                    assetSchema.IncludeLabelsInCatalog = false;
                    assetSchema.IncludeGUIDInCatalog = false;
                    assetSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogetherByLabel;
                    assetSchema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
                    assetSchema.InternalBundleIdMode =
                        BundledAssetGroupSchema.BundleInternalIdMode.GroupGuid;
                    assetSchema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
                    assetSchema.InternalIdNamingMode = BundledAssetGroupSchema.AssetNamingMode.GUID;
                    assetSchema.BuildPath.SetVariableByName( aaSettings,LocalBuildPathKey);
                    assetSchema.LoadPath.SetVariableByName( aaSettings,LocalLoadPathKey);

                    ForceSetProvider(assetSchema);
                }
            }
        }
        
        private static void ForceSetProvider(BundledAssetGroupSchema schema)
        {
            var type = schema.GetType();
            bool dirty = false;

#if UNITY_WEBGL
            if (schema.AssetBundleProviderType.Value != typeof(AssetBundleProvider))
            {
                var assetBundleProviderProp = type.GetField("m_AssetBundleProviderType",BindingFlags.Instance | BindingFlags.NonPublic);
                var assetBundleProvider = new SerializedType()
                {
                    Value = typeof(AssetBundleProvider)
                };
                assetBundleProviderProp?.SetValue(schema,assetBundleProvider);
                dirty = true;
            }

            if (schema.BundledAssetProviderType.Value != typeof(BundledAssetProvider))
            {
                var bundledProviderProp = type.GetField("m_BundledAssetProviderType",BindingFlags.Instance | BindingFlags.NonPublic);
                var bundledProvider = new SerializedType()
                {
                    Value = typeof(BundledAssetProvider)
                };
                bundledProviderProp?.SetValue(schema,bundledProvider);
                dirty = true;
            }

            schema.UseAssetBundleCache = false;
            schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.AppendHash;
#else
            if (schema.AssetBundleProviderType.Value != typeof(DHAssetBundleProvider))
            {
                var assetBundleProviderProp = type.GetField("m_AssetBundleProviderType",BindingFlags.Instance | BindingFlags.NonPublic);
                var assetBundleProvider = new SerializedType()
                {
                    Value = typeof(DHAssetBundleProvider)
                };
                assetBundleProviderProp?.SetValue(schema,assetBundleProvider);
                dirty = true;
            }

            if (schema.BundledAssetProviderType.Value != typeof(DHBundledAssetProvider))
            {
                var bundledProviderProp = type.GetField("m_BundledAssetProviderType",BindingFlags.Instance | BindingFlags.NonPublic);
                var bundledProvider = new SerializedType()
                {
                    Value = typeof(DHBundledAssetProvider)
                };
                bundledProviderProp?.SetValue(schema,bundledProvider);
                dirty = true;
            }

            schema.UseAssetBundleCache = true;
            schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
#endif
           
            

            if (dirty)
            {
                EditorUtility.SetDirty(schema);
            }
        }
    }
}

    