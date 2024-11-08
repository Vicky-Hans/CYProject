using System;
using System.IO;
using DHHybridCLR.Editor.BuildPipeline.BuildPipelineCore;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Il2Cpp;

namespace DHHybridCLR.Editor.BuildPipeline
{
    public partial class BuildPipeline : BuildPipelineCallbackBase
    {
        public override void OnPostBuildPlayerScriptDLLs(BuildReport report)
        {
            if (!IsEnableHybridCLR())
            {
                return;
            }

            LinkGeneratorCommand.GenerateLinkXml();
        }

        public override void OnPreprocessBuild(BuildReport report)
        {
            if (!IsEnableHybridCLR())
            {
                return;
            }
            //
            // Cleanup();
            // PrepareHCLR();
            PrepareMetadata();
#if !UNITY_ANDROID && !UNITY_IPHONE
            var path = report.summary.outputPath;
            path = path.Replace('\\', '/');
            path = path.Substring(0, path.LastIndexOf('/'));
            var file = Directory.GetFiles(path, "global-metadata.dat", SearchOption.AllDirectories);
            if (file.Length == 0)
            {
                throw new Exception($"[BuildPipeline::Encrypt] OnPostprocessBuild {path}找不到!!!");
            }

            OnEncryptMetadataProcess(file[0]);
#endif
            
        }

        public override void OnPostGenerateGradleAndroidProject(string path)
        {
            if (!IsEnableHybridCLR())
            {
                return;
            }
            
#if UNITY_ANDROID
            PostStreamingData(BuildTarget.Android, path + "/src/main/assets/");
            OnEncryptMetadataProcess(path + "/src/main/assets/bin/Data/Managed/Metadata/global-metadata.dat");
#endif
        }

        public override void OnBeforeConvertRun(BuildReport report, Il2CppBuildPipelineData data)
        {
            if (!IsEnableHybridCLR())
            {
                return;
            }
            
            GenerateAOTMetaScripts(EditorUserBuildSettings.activeBuildTarget);
#if !UNITY_ANDROID && !UNITY_IPHONE
            PostStreamingData(data.target);
#endif
        }
    }
}
