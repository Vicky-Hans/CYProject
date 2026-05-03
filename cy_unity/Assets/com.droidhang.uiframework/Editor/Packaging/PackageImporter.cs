using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DH.UIFramework.Editor
{
    public static class PackageImporter
    {
        private static readonly string PackageName = "com.droidhang.uiframework";
        private static readonly string FileName = "UIFramework.unitypackage";
        private static readonly string RootPath = new DirectoryInfo(Application.dataPath).Parent.FullName;
#if BPC_PROJECT
        private static void ExportPackage()
        {
            var rootPath = GetRootPath();
            var paths = GetExportedFiles();
            AssetDatabase.ExportPackage(paths.ToArray(), Path.Combine(rootPath,FileName), ExportPackageOptions.Recurse);
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Lua组件/UI框架/发布")]
        public static void PublishUIFramework()
        {
            ExportPackage();
            var targetRoot = "Assets/Temp";
            var sourceRoot = $"Assets/{PackageName}";
            AssetDatabase.StartAssetEditing();
            try
            {
                // 移动文件到临时文件夹
                var paths = GetPublishFiles();
                if (Directory.Exists(targetRoot))
                {
                    Directory.Delete(targetRoot,true);
                }

                // 移动到临时发布目录
                foreach (var item in paths)
                {
                    var targetPath = item.Replace(sourceRoot, targetRoot);
                    var directory = new DirectoryInfo(targetPath).Parent.FullName;
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    File.Move(item,targetPath);
                }

                var result = CommandUtil.RunNormalCommand("npm", "publish --registry http://unityregistry.dhgames.cn", targetRoot);
                if (result.exitCode != 0)
                {
                    Debug.LogError(result.stderr);
                }
                else
                {
                    Debug.Log(result.message);
                }

                // 恢复文件
                foreach (var item in paths)
                {
                    var sourcePath = item.Replace(sourceRoot, targetRoot);
                    var directory = new DirectoryInfo(item).Parent.FullName;
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    File.Move(sourcePath,item);
                }
                
                Directory.Delete(targetRoot,true);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private static List<string> GetExportedFiles()
        {
            var rootPath = GetRootPath();
            var assetPaths = new []
            {
                $"{rootPath}/Runtime",
            };

            var editorPath = GetIgnoreFolder();
            var fileInfos = Directory.GetFiles($"{rootPath}/Editor");
            var directoryInfos = Directory.GetDirectories($"{rootPath}/Editor");
            var paths = new List<string>(fileInfos);
            paths.AddRange(directoryInfos);
            paths.AddRange(assetPaths);
            paths.RemoveAll(x => x.Contains("DS_Store"));
            paths.RemoveAll(x => x == editorPath);
            return paths;
        }
#endif

        private static List<string> GetPublishFiles()
        {
            var paths = new List<string>();
            var rootPath = GetRootPath();
            var fileInfos = Directory.GetFiles(rootPath);
            paths.AddRange(fileInfos);
            paths.AddRange(Directory.GetFiles(GetIgnoreFolder()));
            paths.RemoveAll(x => x.Contains("DS_Store"));
            paths.RemoveAll(x => x.Contains("Editor.meta"));
            paths.RemoveAll(x => x.Contains("Runtime.meta"));
            return paths;
        }
        
        /// <summary>
        /// 获取导出Package时需要忽略的文件夹
        /// </summary>
        /// <returns></returns>
        private static string GetIgnoreFolder()
        {
            var fileName =  GetRootPath();
            return Path.Combine(fileName, "Editor", "Packaging");
        }
        
        private static string GetRootPath()
        {
            return Path.Combine("Assets", PackageName);
        }

        [MenuItem("Lua组件/UI框架/导入")]
        public static void ImportPackage()
        {
            var path = Path.GetFullPath($"Packages/{PackageName}/{FileName}");
            var rootPath = GetRootPath();
            AssetDatabase.ImportPackage(path,true);
            AssetDatabase.Refresh();
        }
    }
}