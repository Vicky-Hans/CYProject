using System;
using System.Collections.Generic;
using System.IO;
using HybridCLR.Editor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DHHybridCLR.Editor.BuildPipeline
{
    public partial class BuildPipeline
    {
        private static readonly List<string> LstLibCachePath = new List<string>
        {
            "il2cpp_android_arm64-v8a",
            "il2cpp_android_armeabi-v7a",
            "Il2cppBuildCache",
            "il2cpp_cache"
        };


        public static void PrepareFcg()
        {
            if (!IsEnableHybridCLR())
            {
                return;
            }
            
            Cleanup();
            PrepareHCLR();
        }
        
        private static void Cleanup()
        {
            var libraryPath = Path.Combine(Application.dataPath, "..", "Library");
            if (!Directory.Exists(libraryPath))
            {
                throw new Exception("[BuildPipeline::Cleanup] Library is not Exists!!!");
            }

            foreach (var cache in LstLibCachePath)
            {
                var tmp = Path.GetFullPath(Path.Combine(libraryPath, cache)).Replace('\\', '/');
                if (!Directory.Exists(tmp))
                {
                    continue;
                }
            
                Directory.Delete(tmp, true);
                Debug.Log($"[BuildPipeline:Cleanup] delete {tmp}");
            }

            var tmpStagingArea = Path.Combine(SettingsUtil.ProjectDir, "Temp/StagingArea");
            if (Directory.Exists(tmpStagingArea))
            {
                Directory.Delete(tmpStagingArea, true);
                Debug.Log($"[BuildPipeline:Cleanup] delete {tmpStagingArea}");
            }
        }
    }
}
