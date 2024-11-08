using System;
using HybridCLR.Editor.Installer;
using HybridCLR.Editor.Settings;
using UnityEditor;
using UnityEngine;

namespace DHHybridCLR.Editor.BuildPipeline
{
    public partial class BuildPipeline
    {
        [MenuItem("HybridCLRxLua/CheckHCLRInstall", false, 301)]
        public static void CheckHCLRInstall()
        {
            if (!IsEnableHybridCLR())
            {
                Debug.Log("已关闭HybridCLR");
                Environment.SetEnvironmentVariable("UNITY_IL2CPP_PATH", "");
                Debug.Log($"[CheckSettings] 清除 UNITY_IL2CPP_PATH");
                
                HybridCLRSettings.Instance.enable = false;
                HybridCLRSettings.Instance.hotUpdateAssemblies = Array.Empty<string>();
                HybridCLRSettings.Save();
                return;
            }

            HybridCLRSettings.Instance.enable = true;
            HybridCLRSettings.Save();
            
            var controller = new InstallerController();

            if (!controller.HasInstalledHybridCLR())
            {
                Debug.Log($"[BuildPipeline::CheckHCLRInstall] Start Install");
                controller.InstallDefaultHybridCLR();
            }
            
            Debug.Log($"[BuildPipeline::CheckHCLRInstall] Check end");
        }

        public static bool IsEnableHybridCLR()
        {
            var configAsset = Resources.Load<StartupLauncherConfig>("StartupLauncherConfig");
            var enabled = configAsset != null && configAsset.EnableHybridCLR;
            Debug.Log($"[BuildPipeline::IsEnableHybridCLR] {enabled}");
            return enabled;
        }
        
        public static bool IsEnableEncryptGlobalMetadata()
        {
            var configAsset = Resources.Load<StartupLauncherConfig>("StartupLauncherConfig");
            var enabled = configAsset != null && configAsset.EnabledEncryptGlobalMetadata;
            Debug.Log($"[BuildPipeline::EnabledEncryptGlobalMetadata] {enabled}");
            return enabled;
        }
    }
}
