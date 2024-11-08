using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using DHHybridCLR.Utils;
using HybridCLR.Editor;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DHHybridCLR.Editor.BuildPipeline
{
    public partial class BuildPipeline
    {
        private static Helper _keyHelper;

        private static void PrepareMetadata()
        {
            RevertMetadataLoaderCppFile();

            _keyHelper = new Helper();

            //非压缩模式直接修改cpp文件
            if (IsEnableEncryptGlobalMetadata())
            {
                OnPrepareDecryptCode();
            }
        }

        private static void OnEncryptMetadataProcess(string rootPath = null)
        {
            if (!IsEnableEncryptGlobalMetadata())
            {
                UnityEngine.Debug.Log($"[BuildPipeline:EncryptMetadata] 未启用");
                return;
            }

            var srcPath = rootPath;
            if (string.IsNullOrEmpty(rootPath))
            {
                srcPath = $"{SettingsUtil.ProjectDir}/Temp/StagingArea/Data/Managed/Metadata/global-metadata.dat";
            }

            UnityEngine.Debug.Log($"[BuildPipeline:EncryptMetadata] {srcPath}");

            Encrypt(srcPath);
        }

        private static void Encrypt(string strFileName)
        {
            if (!File.Exists(strFileName))
            {
                throw new Exception($"[BuildPipeline::Encrypt] {strFileName}找不到!!!");
            }

            var sw = Stopwatch.StartNew();
            var fileBytes = File.ReadAllBytes(strFileName);

            var savePath = Path.Combine(Path.GetFullPath("."),"HybridCLR/Temp");
            if (!string.IsNullOrEmpty(savePath) && savePath.Length > 0)
            {
                if (Directory.Exists(savePath))
                {
                    var dateStr = DateTime.Now.ToString("yyyyMMddHHmm");
                    var dstFile = Path.Combine(savePath, $"global-metadata_{dateStr}.dat");
                    File.WriteAllBytes(dstFile, fileBytes);

                    Debug.Log($"[BuildPipeline:Encrypt] back file to : {dstFile}");
                }
            }

            var msIn = new MemoryStream(fileBytes);
            var msOut = new MemoryStream();

            Utility.Compress(msIn, msOut);

            fileBytes = msOut.ToArray();

            _keyHelper.Xor(fileBytes);

            File.WriteAllBytes(strFileName, fileBytes);

            sw.Stop();

            Debug.Log($"[BuildPipeline:Encrypt] finished: {sw.ElapsedMilliseconds} ms!!!");
        }

        private static void OnPrepareDecryptCode()
        {
            if (!IsEnableEncryptGlobalMetadata())
            {
                Debug.Log($"[BuildPipeline:OnPrepareDecryptCode] 未启用");
                return;
            }

            var strMetadataLoaderFile = "MetadataLoader2020.cpp";
#if !UNITY_2020_1_OR_NEWER
            strMetadataLoaderFile = "MetadataLoader2019.cpp";
#endif
            var assets = AssetDatabase.FindAssets(strMetadataLoaderFile);
            if (assets.Length == 0)
            {
                throw new Exception($"[BuildPipeline::OnPrepareDecryptCode] MetadataLoader.cpp找不到!!!");
            }

            var filePath = AssetDatabase.GUIDToAssetPath(assets[0]);
            var codeTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            if (codeTemplate == null || string.IsNullOrEmpty(codeTemplate.text))
            {
                throw new Exception($"[BuildPipeline::OnPrepareDecryptCode] codeTemplate is empty!!");
            }

            var codeTxt = codeTemplate.text;

            strMetadataLoaderFile = "MetadataLoaderHeader.cpp";
            assets = AssetDatabase.FindAssets(strMetadataLoaderFile);
            if (assets.Length == 0)
            {
                throw new Exception($"[BuildPipeline::OnPrepareDecryptCode] MetadataLoaderHeader.cpp找不到!!!");
            }

            filePath = AssetDatabase.GUIDToAssetPath(assets[0]);
            codeTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            if (codeTemplate == null || string.IsNullOrEmpty(codeTemplate.text))
            {
                throw new Exception($"[BuildPipeline::OnPrepareDecryptCode] MetadataLoaderHeader is empty!!");
            }

            var codeHeader = codeTemplate.text;

            var src = Path.Combine(SettingsUtil.LocalIl2CppDir, "libil2cpp", "vm", "MetadataLoader.cpp");
            src = Path.GetFullPath(src).Replace('\\', '/');
            var tmpFile = src + ".DISABLED";
            if (!File.Exists(tmpFile))
            {
                File.Copy(src, tmpFile, true);
            }

            var password = _keyHelper.Password;
            var paramsStr = new StringBuilder();
            paramsStr.AppendLine("int startPos = 0;");
            foreach (var pass in password)
            {
                paramsStr.AppendLine($"\t\t\tfileOutput[startPos++] ^= 0x{pass:x};");
            }

            codeHeader = codeHeader.Replace("//!{{ProcessCode}}//", paramsStr.ToString());
            codeTxt = codeTxt.Replace("//{{HEADER}}//", codeHeader);

            File.WriteAllText(src, codeTxt);

            src = Path.Combine(SettingsUtil.LocalIl2CppDir, "libil2cpp", "vm", "GlobalMetadata.cpp");
            src = Path.GetFullPath(src).Replace('\\', '/');
            tmpFile = src + ".DISABLED";
            if (!File.Exists(tmpFile))
            {
                File.Copy(src, tmpFile, true);
            }

            codeTxt = File.ReadAllText(src);
            codeTxt = codeTxt.Replace("IL2CPP_ASSERT(s_GlobalMetadataHeader->sanity ==",
                "// IL2CPP_ASSERT(s_GlobalMetadataHeader->sanity ==");
            codeTxt = codeTxt.Replace("IL2CPP_ASSERT(s_GlobalMetadataHeader->version ==",
                "// IL2CPP_ASSERT(s_GlobalMetadataHeader->version ==");
            File.WriteAllText(src, codeTxt);

            var utilSrc = Path.Combine(SettingsUtil.LocalIl2CppDir, "libil2cpp", "utils", "hyclrxlua");
            utilSrc = Path.GetFullPath(utilSrc).Replace('\\', '/');
            if (Directory.Exists(utilSrc))
            {
                Directory.Delete(utilSrc, true);
            }

            Directory.CreateDirectory(utilSrc);

            assets = AssetDatabase.FindAssets("HyclrXluaCppModule");
            if (assets.Length == 0)
            {
                throw new Exception($"[BuildPipeline::OnPrepareDecryptCode] HyclrXluaCppModule!!!");
            }

            filePath = AssetDatabase.GUIDToAssetPath(assets[0]);
            if (!ExtractApiCodeToPath(filePath, utilSrc))
            {
                throw new Exception($"[BuildPipeline::OnPrepareDecryptCode] 释放HyclrXluaCppModule出错");
            }
        }

        private static void RevertMetadataLoaderCppFile()
        {
            var src = Path.Combine(SettingsUtil.LocalIl2CppDir, "libil2cpp", "vm", "MetadataLoader.cpp");
            src = Path.GetFullPath(src).Replace('\\', '/');
            var tmpFile = src + ".DISABLED";

            if (File.Exists(tmpFile))
            {
                File.Copy(tmpFile, src, true);
                File.Delete(tmpFile);

                Debug.Log($"[BuildPipeline:RevertMetadataLoaderCppFile] MetadataLoader.cpp ok!!!");
            }
            else
            {
                Debug.Log($"[BuildPipeline:RevertMetadataLoaderCppFile] can't find {tmpFile} file!!!");
            }

            src = Path.Combine(SettingsUtil.LocalIl2CppDir, "libil2cpp", "vm", "GlobalMetadata.cpp");
            src = Path.GetFullPath(src).Replace('\\', '/');
            tmpFile = src + ".DISABLED";

            if (File.Exists(tmpFile))
            {
                File.Copy(tmpFile, src, true);
                File.Delete(tmpFile);

                Debug.Log($"[BuildPipeline:RevertMetadataLoaderCppFile] GlobalMetadata.cpp ok!!!");
            }
            else
            {
                Debug.Log($"[BuildPipeline:RevertMetadataLoaderCppFile] can't find {tmpFile} file!!!");
            }

            var utilSrc = Path.Combine(SettingsUtil.LocalIl2CppDir, "libil2cpp", "utils", "hyclrxlua");
            utilSrc = Path.GetFullPath(utilSrc).Replace('\\', '/');
            if (Directory.Exists(utilSrc))
            {
                Directory.Delete(utilSrc, true);
            }
        }
    }
}
