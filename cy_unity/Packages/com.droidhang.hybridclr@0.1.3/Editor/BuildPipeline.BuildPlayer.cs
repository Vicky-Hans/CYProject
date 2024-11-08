using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HybridCLR.Editor;
using UnityEditor;
using UnityEngine;
using HybridCLR.Editor.Commands;
using Debug = UnityEngine.Debug;

namespace DHHybridCLR.Editor.BuildPipeline
{
    public partial class BuildPipeline
    {
        public static Type XLuaMonoPInvokeAttribute; //需要在打包前手动赋值，避免对XLua引用
        
        private static bool CheckPlatform(BuildTarget target)
        {
            return target == EditorUserBuildSettings.activeBuildTarget;
        }

        public static uint GetPInvokeMethodCount()
        {
            var sw = Stopwatch.StartNew();
            uint pInvokeCount = 0;
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in allAssemblies)
            {
                if (!assembly.FullName.Contains("Assembly-CSharp"))
                {
                    continue;
                }
                
                var allTypes = assembly.GetTypes().ToArray();
                var aotPInvoke = typeof(AOT.MonoPInvokeCallbackAttribute);
                //var xLuaPInvoke = typeof(XLua.MonoPInvokeCallbackAttribute);
                var bindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
                
                foreach (var type in allTypes)
                {
                    var allMtds = type.GetMethods(bindFlags);
                    foreach (var mtd in allMtds)
                    {
                        try
                        {
                            if (mtd.IsDefined(aotPInvoke) || mtd.IsDefined(XLuaMonoPInvokeAttribute))
                            {
                                pInvokeCount++;
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            
            sw.Stop();

            Debug.Log($"[BuildPipeline:GetPInvokeMethodCount] cnt:{pInvokeCount} {sw.ElapsedMilliseconds}ms!!!");

            return pInvokeCount;
        }
        
        [MenuItem("HybridCLRxLua/Export/BeforeBuild", false, 102)]
        public static void BeforeBuild()
        {
            if (!IsEnableHybridCLR())
            {
                return;
            }
            
            
            
            GenerateHotfixScripts(EditorUserBuildSettings.activeBuildTarget);
        }

        private static void PrepareHCLR()
        {
            // SettingsUtil.HybridCLRSettings.ReversePInvokeWrapperCount = (int) (GetPInvokeMethodCount() * 1.5f);
            // ReversePInvokeWrapperGeneratorCommand.GenerateReversePInvokeWrapper();
            // AOTReferenceGeneratorCommand.GenerateAOTGenericReference(false);
            // MethodBridgeGeneratorCommand.GenerateMethodBridge(false);
            
            PrebuildCommand.GenerateAll();
        }

        [MenuItem("HybridCLRxLua/Export/ActiveBuildTarget", false, 102)]
        private static void Build_ActiveBuild()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    Build_Android();
                    break;
                case BuildTarget.StandaloneWindows64:
                    Build_Win64();
                    break;
                case BuildTarget.StandaloneOSX:
                    Build_MacOSX();
                    break;
                case BuildTarget.iOS:
                    Build_iOS();
                    break;
                default:
                    Debug.LogError($"[BuildPipeline::CheckPlatform]" +
                                   $"{EditorUserBuildSettings.activeBuildTarget}" +
                                   $"not support..." +
                                   $"请先切到合适平台再打包");
                    break;
            }
        }

        private static BuildPlayerOptions _bpOption = new BuildPlayerOptions
        {
            scenes = GetBuildScenes(),
            options = BuildOptions.CompressWithLz4,
            target = BuildTarget.StandaloneWindows64,
            targetGroup = BuildTargetGroup.Standalone,
        };

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

        [MenuItem("HybridCLRxLua/Export/Win64", false, 201)]
        private static void Build_Win64()
        {
            Debug.Log($"[BuildPipeline::Build_Win64] Begin");
            if (!CheckPlatform(BuildTarget.StandaloneWindows64))
            {
                throw new Exception($"[BuildPipeline::Build_Win64] 请先切到pc平台再打包");
            }

            BeforeBuild();

            var outputPath = $"{SettingsUtil.ProjectDir}/Release-Win64";
            var location = $"{outputPath}/HybridCLRXLua.exe";

            Debug.Log($"[BuildPipeline::Build_Win64] BuildPlayer");

            _bpOption.locationPathName = location;
            _bpOption.target = BuildTarget.StandaloneWindows64;
            _bpOption.targetGroup = BuildTargetGroup.Standalone;
            var report = UnityEditor.BuildPipeline.BuildPlayer(_bpOption);
            Debug.Log($"[BuildPipeline::Build_Win64] End");
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildPipeline::Build_Win64] BuildPlayer 打包失败:{report.summary.result}");
            }
            else
            {
                Application.OpenURL($"file:///{location}");
            }
        }

        [MenuItem("HybridCLRxLua/Export/Android", false, 202)]
        private static void Build_Android()
        {
            Debug.Log($"[BuildPipeline::Build_Android] Begin");
            if (!CheckPlatform(BuildTarget.Android))
            {
                throw new Exception($"[BuildPipeline::Build_Android] 请先切到Android平台再打包");
            }

            BeforeBuild();
            
            var outputPath = $"{SettingsUtil.ProjectDir}/Release-Android/";
            var location = outputPath + "/HybridCLRXLua.apk";

            Debug.Log($"[BuildPipeline::Build_Android] BuildPlayer");

            _bpOption.locationPathName = location;
            _bpOption.target = BuildTarget.Android;
            _bpOption.targetGroup = BuildTargetGroup.Android;
            var report = UnityEditor.BuildPipeline.BuildPlayer(_bpOption);
            Debug.Log($"[BuildPipeline::Build_Android] End");
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildPipeline::Build_Android] BuildPlayer 打包失败:{report.summary.result}");
            }
            else
            {
                Application.OpenURL($"file:///{outputPath}");
            }
        }

        [MenuItem("HybridCLRxLua/Export/OSX", false, 203)]
        private static void Build_MacOSX()
        {
            Debug.Log($"[BuildPipeline::Build_MacOSX] Begin");
            if (!CheckPlatform(BuildTarget.StandaloneOSX))
            {
                throw new Exception($"[BuildPipeline::Build_MacOSX] 请先切到Mac平台再打包");
            }

            BeforeBuild();

            var outputPath = $"{SettingsUtil.ProjectDir}/Release-MacOSX";
            var location = $"{outputPath}/HybridCLRXLua.app";

            Debug.Log($"[BuildPipeline::Build_MacOSX] BuildPlayer");

            _bpOption.locationPathName = location;
            _bpOption.target = BuildTarget.StandaloneOSX;
            _bpOption.targetGroup = BuildTargetGroup.Standalone;
            var report = UnityEditor.BuildPipeline.BuildPlayer(_bpOption);
            Debug.Log($"[BuildPipeline::Build_MacOSX] End");
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildPipeline::Build_MacOSX] BuildPlayer 打包失败:{report.summary.result}");
            }
            else
            {
                Application.OpenURL($"file:///{location}");
            }
        }

        [MenuItem("HybridCLRxLua/Export/iOS", false, 204)]
        private static void Build_iOS()
        {
            Debug.Log($"[BuildPipeline::Build_iOS] Begin");
            if (!CheckPlatform(BuildTarget.iOS))
            {
                throw new Exception($"[BuildPipeline::Build_iOS] 请先切到iOS平台再打包");
            }

            BeforeBuild();

            var outputPath = $"{SettingsUtil.ProjectDir}/Release-iOS/";
            Debug.Log($"[BuildPipeline::Build_iOS] BuildPlayer");

            _bpOption.locationPathName = outputPath;
            _bpOption.target = BuildTarget.iOS;
            _bpOption.targetGroup = BuildTargetGroup.iOS;
            var report = UnityEditor.BuildPipeline.BuildPlayer(_bpOption);
            Debug.Log($"[BuildPipeline::Build_iOS] End");
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildPipeline::Build_iOS] BuildPlayer 打包失败:{report.summary.result}");
            }
            else
            {
                Application.OpenURL($"file:///{outputPath}");
            }
        }
    }
}
