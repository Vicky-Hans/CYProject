using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DH.Asset;
using DHFramework;
using DHHybridCLR.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace DHHybridCLR.Scripts
{
    public partial class GameRoot
    {
        /// <summary>
        /// 加载XXX.bytes文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="password"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator LoadData(string fileName, string password, Action<Dictionary<string, byte[]>> callback, string readOnlyPath = "")
        {
            DHFramework.DHLog.Debug($"[GameRoot::LoadData] {fileName}");
            byte[] bytes = null;
            
            //只读路径
            string loadPath = string.IsNullOrEmpty(readOnlyPath) ? Application.streamingAssetsPath : readOnlyPath;

            if (!IsBaseVersion())
            {
                //判断是否存在热更新文件
                var runtimePath = DHUtility.Path.GetRegularPath(Path.Combine(DHAssetsConfig.ReadWritePath, fileName));
                if (File.Exists(runtimePath))
                {
                    loadPath = DHAssetsConfig.ReadWritePath;
                }
            }
            
            var fullLoadPath = DHUtility.Path.GetRemotePath(Path.Combine(loadPath, fileName));
            DHFramework.DHLog.Debug($"[GameRoot::LoadData] {fileName} from {fullLoadPath}");

            var req = UnityWebRequest.Get(fullLoadPath);
            yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
            {
                DHFramework.DHLog.Error($"[GameRoot::LoadData] error:{req.error}");
                callback?.Invoke(null);
                yield break;
            }
#else
            if (req.isHttpError || req.isNetworkError)
            {
                DHFramework.DHLog.Error($"[GameRoot::LoadData] error:{req.error}");
                callback?.Invoke(null);
                yield break;
            }
#endif
            bytes = req.downloadHandler?.data;

            if (bytes == null || bytes.Length == 0)
            {
                DHFramework.DHLog.Error($"[GameRoot::LoadData] bytes is empty!");
                callback?.Invoke(null);
                yield break;
            }

            DHFramework.DHLog.Debug($"[GameRoot::LoadData] {fileName} load ok!");

            yield return null;

            bytes = XXTEA.Decrypt(bytes, password);
            using (var ms = new MemoryStream(bytes))
            {
                using (var output = new MemoryStream(bytes.Length))
                {
                    if (!Utils.Utility.Decompress(ms, output))
                    {
                        DHFramework.DHLog.Error($"[GameRoot::LoadData] Decompress {fileName} error!");
                        callback?.Invoke(null);
                        yield break;
                    }

                    output.Seek(0, SeekOrigin.Begin);
                    var dictBytes = new Dictionary<string, byte[]>();
                    {
                        using (var br = new BinaryReader(output))
                        {
                            var count = br.ReadInt32();
                            for (var i = 0; i < count; ++i)
                            {
                                var fn = br.ReadString();
                                var cnt = br.ReadInt32();
                                var buf = br.ReadBytes(cnt);

                                dictBytes.Add(fn, buf);
                            }
                        }
                    }

                    DHFramework.DHLog.Debug($"[GameRoot::LoadData] {fileName} finished!");

                    callback?.Invoke(dictBytes);
                }
            }
        }
    }
}
