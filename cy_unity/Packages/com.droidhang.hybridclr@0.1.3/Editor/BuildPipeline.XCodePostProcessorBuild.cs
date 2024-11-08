using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

#if UNITY_IPHONE
using UnityEditor.iOS.Xcode;
#endif


namespace DHHybridCLR.Editor.BuildPipeline
{
    public partial class BuildPipeline
    {
#if UNITY_IPHONE
        private static void EnableFileSharing(string targetPath)
        {
            var plistPath = targetPath + "/Info.plist";
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            var rootDict = plist.root;
            var val = rootDict["UIFileSharingEnabled"];

            //如果已经存在了，就根据提前设置的来
            if (val != null)
            {
                return;
            }

            rootDict.SetBoolean("UIFileSharingEnabled", true);
            File.WriteAllText(plistPath, plist.WriteToString());
        }

        [PostProcessBuild(88)]
        private static void OnPostProcessBuild(BuildTarget target, string targetPath)
        {
            if (target != BuildTarget.iOS)
            {
                Debug.LogWarning("[BuildPipeline::OnPostProcessBuild]" +
                                 "Target is not iPhone. XCodePostProcess will not run");
                return;
            }

            EnableFileSharing(targetPath);

            var projPath = Path.GetFullPath(targetPath) + "/Unity-iPhone.xcodeproj/project.pbxproj";

            var proj = new PBXProject();
            proj.ReadFromFile(projPath);

            var targetGuid = proj.GetUnityFrameworkTargetGuid();

            var shellName = "PreBuildIl2CPP";
            var shellPath = "";
            var sbShellScript = new StringBuilder();
            sbShellScript.AppendLine("echo PreBuildIl2CPP Start");
            sbShellScript.AppendLine("export CMAKE_ROOT=/Applications/CMake.app/Contents/bin");
            sbShellScript.AppendLine("export PATH=$CMAKE_ROOT:$PATH");
            sbShellScript.AppendLine("cd ../HybridCLRData/iosBuild/");
            sbShellScript.AppendLine("sh build_libil2cpp.sh");
            sbShellScript.AppendLine("cp build/libil2cpp.a $PROJECT_DIR/Libraries/libil2cpp.a");
            sbShellScript.AppendLine("echo PreBuildIl2CPP Finished");
            var sGuid = proj.InsertShellScriptBuildPhase(0, targetGuid, shellName, shellPath, sbShellScript.ToString());
            Debug.Log($"[BuildPipeline::OnPostProcessBuild] AddShellScriptBuildPhase:{sGuid}");

            proj.WriteToFile(projPath);

            PostStreamingData(BuildTarget.iOS, Path.Combine(targetPath, "Data", "Raw"));
            OnEncryptMetadataProcess(Path.Combine(targetPath, "Data", "Managed", "Metadata", "global-metadata.dat"));
        }
#endif
    }
}
