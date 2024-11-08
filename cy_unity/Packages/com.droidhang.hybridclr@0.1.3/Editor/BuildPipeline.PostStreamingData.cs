using System;
using System.Collections.Generic;
using System.IO;
using HybridCLR.Editor;
using UnityEditor;
using UnityEngine;

namespace DHHybridCLR.Editor.BuildPipeline
{
    public partial class BuildPipeline
    {
        private static readonly List<string> LstCustomFiles = new List<string>
        {
            "base.bytes",
        };

        private static void PostStreamingData(BuildTarget target, string rootPath = null)
        {
            Debug.Log($"[BuildPipeline::PostStreamingData] rootPath is : {rootPath}");
            var srcPath = rootPath;
            if (string.IsNullOrEmpty(rootPath))
            {
                srcPath = $"{SettingsUtil.ProjectDir}/Temp/StagingArea/Data/StreamingAssets";
            }
            
            Debug.Log($"[BuildPipeline::PostStreamingData] srcPath is : {srcPath}");

            var streamingPath = Path.GetFullPath(srcPath).Replace('\\', '/');
            if (!streamingPath.EndsWith("/"))
            {
                streamingPath = streamingPath + "/";
            }

            if (!Directory.Exists(streamingPath))
            {
                throw new Exception($"[BuildPipeline] PostStreamingData {streamingPath} 不存在？？？？");
            }

            foreach (var customFile in LstCustomFiles)
            {
                var file = Path.Combine(Application.streamingAssetsPath, customFile);
                if (!File.Exists(file))
                {
                    throw new Exception($"[BuildPipeline] PostStreamingData {file} 不存在？？？？");
                }

                var dst = Path.Combine(streamingPath, customFile);
                File.Copy(file, dst, true);
                Debug.Log($"[BuildPipeline::PostStreamingData]success Copy：{file} to {dst}");
            }
        }
    }
}
