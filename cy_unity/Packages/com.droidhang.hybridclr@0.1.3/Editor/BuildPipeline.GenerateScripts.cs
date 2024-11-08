using System.Collections.Generic;
using System.IO;
using DH.Asset.Editor;
using DHFramework;
using DHHybridCLR.Utils;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Settings;
using UnityEditor;
using UnityEngine;

namespace DHHybridCLR.Editor.BuildPipeline
{
    public partial class BuildPipeline
    {
        private static void Generate(string srcDir, string fileName, string password, List<string> fileList = null, string dstPath = "")
        {
            if (string.IsNullOrEmpty(dstPath))
            {
                dstPath = Application.streamingAssetsPath;
            }

            var name = fileName.Replace(".dll", "");
            var dstFile = DHUtility.Path.GetRegularPath(Path.Combine(dstPath, name));
            if (File.Exists(dstFile))
            {
                File.Delete(dstFile);
            }

            var srcFullDir = DHUtility.Path.GetRegularPath(Path.GetFullPath(srcDir));
            if (!srcFullDir.EndsWith("/"))
            {
                srcFullDir += "/";
            }

            if (!Directory.Exists(srcFullDir))
            {
                return;
            }

            if (fileList == null || fileList.Count == 0)
            {
                var files = Directory.GetFiles(srcFullDir, "*", SearchOption.AllDirectories);
                fileList = new List<string>(files.Length);
                foreach (var file in files)
                {
                    if (file.EndsWith(".meta"))
                    {
                        continue;
                    }

                    var tmp = file.Replace('\\', '/').Replace(srcFullDir, "").Replace(".dll", "");
                    fileList.Add(tmp);
                }
            }

            if (fileList.Count == 0)
            {
                Debug.Log($"[BuildPipeline::GenerateScripts] {srcDir} filelist is empty!!! {name}");
                return;
            }

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(fileList.Count);
                    foreach (var file in fileList)
                    {
                        var filePath = $"{srcDir}/{file}";
                        if (!File.Exists(filePath))
                        {
                            filePath += ".dll";
                        }
                
                        if (!File.Exists(filePath))
                        {
                            Debug.Log($"[BuildPipeline::GenerateScripts]文件{filePath}不存在!");
                            continue;
                        }

                        bw.Write(file.Replace(".dll", ""));
                        var bytes = File.ReadAllBytes(filePath);
                        bw.Write(bytes.Length);
                        bw.Write(bytes);
                    }

                    using (var output = new MemoryStream((int) ms.Length))
                    {
                        if (!Utility.Compress(ms, output))
                        {
                            Debug.LogError($"[BuildPipeline::GenerateScripts] {name}失败？？？？");
                            return;
                        }

                        var outBuffer = output.ToArray();
                        outBuffer = XXTEA.Encrypt(outBuffer, password);
                        PathUtility.EnsureExistFileDirectory(dstFile);
                        File.WriteAllBytes(dstFile, outBuffer);
                    }
                }
            }
        }

        private static void GenerateHotfixScripts(BuildTarget target, bool compileDll = true)
        {
            var hotDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            if (compileDll)
            {
                CompileDllCommand.CompileDll(target);
            }

            // 使用startup config里的列表替换HCLR的配置，避免多次复制
            var startupConfig = Resources.Load<StartupLauncherConfig>("StartupLauncherConfig");
            if (startupConfig == null)
            {
                DHLog.Error("请先创建StartupLauncherConfig以启动游戏");
                return;
            }

            var hotfixDllList = new List<string>();
            hotfixDllList.Add("Startup");
            hotfixDllList.AddRange(startupConfig.PreLoadDllList);
            hotfixDllList.Add(startupConfig.StartDllName);
            HybridCLRSettings.Instance.hotUpdateAssemblies = hotfixDllList.ToArray();
            HybridCLRSettings.Save();
            
            var dstPath = DroidHangAddressableConfig.LocalBuildPath.Replace("[BuildTarget]", EditorUserBuildSettings.activeBuildTarget.ToString());
            
            foreach (var dllName in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
            {
                Generate(hotDllSrcDir,  $"{dllName}.bytes", "hotfix", new List<string>{dllName}, dstPath);
            }
        }

        private static void GenerateAOTMetaScripts(BuildTarget target)
        {
            var aotDllDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            var manifest = Resources.Load<AOTMetaAssemblyManifest>("AOTMetaAssemblyManifest");
            var aotMetadataDll = new List<string>();
            
            if (manifest != null && manifest.DefaultAOTMetadataDlls != null && manifest.DefaultAOTMetadataDlls.Length > 0)
            {
                aotMetadataDll.AddRange(manifest.DefaultAOTMetadataDlls);
            }
            Generate(aotDllDir, "base.bytes", "base", aotMetadataDll);
            
            if (manifest != null && manifest.AOTMetadataDlls != null && manifest.AOTMetadataDlls.Length > 0)
            {
                aotMetadataDll.Clear();
                aotMetadataDll.AddRange(manifest.AOTMetadataDlls);
            }
            Generate(aotDllDir, "startupBase.bytes", "startupBase", aotMetadataDll);
        }

        private static void GenerateScripts(BuildTarget target, bool compileDll = true)
        {
            GenerateHotfixScripts(target, compileDll);
            GenerateAOTMetaScripts(target);
        }

        [MenuItem("HybridCLRxLua/GenerateScripts/ActiveBuildTarget", false, 100)]
        private static void GenerateDllActiveBuildTarget()
        {
            GenerateScripts(EditorUserBuildSettings.activeBuildTarget);
        }

        [MenuItem("HybridCLRxLua/GenerateScripts/Win64", false, 201)]
        private static void GenerateDllWin64()
        {
            GenerateScripts(BuildTarget.StandaloneWindows64);
        }

        [MenuItem("HybridCLRxLua/GenerateScripts/Android", false, 202)]
        private static void GenerateDllAndroid()
        {
            GenerateScripts(BuildTarget.Android);
        }

        [MenuItem("HybridCLRxLua/GenerateScripts/OSX", false, 203)]
        private static void GenerateDllOsx()
        {
            GenerateScripts(BuildTarget.StandaloneOSX);
        }

        [MenuItem("HybridCLRxLua/GenerateScripts/iOS", false, 204)]
        private static void GenerateDllIos()
        {
            GenerateScripts(BuildTarget.iOS);
        }
    }
}
