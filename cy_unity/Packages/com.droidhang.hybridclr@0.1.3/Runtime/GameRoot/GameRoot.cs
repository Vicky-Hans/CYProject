using System.Collections;
using DHFramework;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using HybridCLR;
using System.IO;
using DHHybridCLR.Utils;
using UnityEngine.Networking;
namespace DHHybridCLR.Scripts
{
    public partial class GameRoot : MonoBehaviour
    {
        public static GameRoot Instance = null;
        public StartupLauncherConfig StartupConfig { get; set; }
        private static readonly string HotUpdateVersionKey = "hotupdate_version";
        private static readonly string HotUpdateMd5Key = "hotupdate_md5";
#if UNITY_WEBGL || WECHAT_MINI
        public static Action StartupLauncher = null;
#endif

        private IEnumerator Start()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitStartupConfig();
#if UNITY_WEBGL || WECHAT_MINI
            StartupLauncher?.Invoke();
            yield return null;
#else
            if (StartupConfig.EnableHybridCLR)
            {
                using (new Benchmark("[GameRoot]LoadMetadataForAotAssembly"))
                {
#if !UNITY_EDITOR
                    yield return LoadData("base.bytes", "base", OnLoadMetadataData);
#endif
                }
            }
            using (new Benchmark("[GameRoot]Startup"))
            {
                yield return LoadGameDll("Startup", RunDll);
            }
#endif
        }
        private void InitStartupConfig()
        {
            StartupConfig = Resources.Load<StartupLauncherConfig>("StartupLauncherConfig");
            if (StartupConfig == null)
            {
                DHLog.Error("请先创建StartupLauncherConfig以启动游戏");
            }
        }
#if !(UNITY_WEBGL || WECHAT_MINI)
        /// <summary>
        /// 加载某一个热更新的dll
        /// </summary>
        /// <param name="hotfixDllName"></param>
        /// <param name="loadCallback"></param>
        /// <returns></returns>
        public IEnumerator LoadGameDll(string hotfixDllName, Action<System.Reflection.Assembly> loadCallback)
        {
#if UNITY_EDITOR
            var gameDll = AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == hotfixDllName);
            yield return null;
            loadCallback?.Invoke(gameDll);
#else
            yield return LoadData($"{hotfixDllName}.bytes", "hotfix", dict =>
            {
                if (dict == null)
                {
                    loadCallback?.Invoke(null);
                    return;
                }
                if (!dict.TryGetValue(hotfixDllName, out var hotfixData))
                {
                    loadCallback?.Invoke(null);
                    return;
                }
                if (hotfixData == null || hotfixData.Length == 0)
                {
                    loadCallback?.Invoke(null);
                    return;
                }
                var gameDll = System.Reflection.Assembly.Load(hotfixData);
                loadCallback?.Invoke(gameDll);
            }, DHAssetsConfig.ReadOnlyPath);
#endif
        }
        /// <summary>
        /// 加载XXX.bytes文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="password"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator LoadData(string fileName, string password, Action<Dictionary<string, byte[]>> callback, string readOnlyPath = "")
        {
            //只读路径
            var loadPath = string.IsNullOrEmpty(readOnlyPath) ? Application.streamingAssetsPath : readOnlyPath;
            if (!IsBaseVersion())
            {
                //判断是否存在热更新文件
                var runtimePath = DHUtility.Path.GetRegularPath(Path.Combine(DHAssetsConfig.ReadWritePath, fileName));
                if (File.Exists(runtimePath)) loadPath = DHAssetsConfig.ReadWritePath;
            }
            var fullLoadPath = DHUtility.Path.GetRemotePath(Path.Combine(loadPath, fileName));
            var req = UnityWebRequest.Get(fullLoadPath);
            yield return req.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
            {
                callback?.Invoke(null);
                yield break;
            }
#else
            if (req.isHttpError || req.isNetworkError)
            {
                callback?.Invoke(null);
                yield break;
            }
#endif
            var bytes = req.downloadHandler?.data;
            if (bytes == null || bytes.Length == 0)
            {
                callback?.Invoke(null);
                yield break;
            }
            yield return null;
            bytes = XXTEA.Decrypt(bytes, password);
            using var ms = new MemoryStream(bytes);
            using var output = new MemoryStream(bytes.Length);
            if (!Utility.Decompress(ms, output))
            {
                callback?.Invoke(null);
                yield break;
            }
            output.Seek(0, SeekOrigin.Begin);
            var dictBytes = new Dictionary<string, byte[]>();
            {
                using var br = new BinaryReader(output);
                var count = br.ReadInt32();
                for (var i = 0; i < count; ++i)
                {
                    var fn = br.ReadString();
                    var cnt = br.ReadInt32();
                    var buf = br.ReadBytes(cnt);
                    dictBytes.Add(fn, buf);
                }
            }
            callback?.Invoke(dictBytes);
        }
        private static void OnLoadMetadataData(Dictionary<string, byte[]> dllBytes)
        {
            dllBytes ??= new Dictionary<string, byte[]>();
            var mode = HomologousImageMode.SuperSet;
            foreach (var dll in dllBytes)
            {
                try
                {
                    var err = RuntimeApi.LoadMetadataForAOTAssembly(dll.Value, mode);
                    if (err != LoadImageErrorCode.OK)
                    {
                        DHLog.Debug($"[GameRoot::LoadMetadataForAotAssembly] load {dll.Key} ret:{err}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
#endif
        private void RunDll(System.Reflection.Assembly gameDll)
        {
            if (gameDll == null)  return;
            var appType = gameDll.GetType("DH.Launch.StartupLauncher");
            if (appType == null) return;
            var mainMethod = appType.GetMethod("LaunchEntry");
            if (mainMethod == null) return;
            mainMethod.Invoke(null, null);
        }
        private bool IsBaseVersion()
        {
            var curVersion = GetVersion();
            return curVersion == Application.version;
        }
        private static string GetVersion()
        {
            var appVersion = Application.version;
            var hotUpdateVersion = DHUnityUtil.PlayerPrefs.GetString(HotUpdateVersionKey, "");
            if (string.IsNullOrEmpty(hotUpdateVersion)) return appVersion;
            var res = CompareVersion(appVersion, hotUpdateVersion);
            if (res > 0) return appVersion; //大版本比热更版本高
            if (res >= 0) return appVersion;
            //大版本比热更版本低:如果a.b不相等,则直接返回appVersion,用于兼容内网包覆盖安装
            var appList = GetSplitVersion(appVersion);
            var hotUpdateList = GetSplitVersion(hotUpdateVersion);
            if (appList[0] != hotUpdateList[0] || appList[1] != hotUpdateList[1]) return appVersion;
            return hotUpdateVersion;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns>v1 > v2: 返回1  v1 == v2:返回0  v1 < v2:返回-1</returns>
        private static int CompareVersion(string v1, string v2)
        {
            var version1 = GetSplitVersion(v1);
            var version2 = GetSplitVersion(v2);
            for (var i = 0; i < version1.Length; ++i)
            {
                var s1 = version1[i];
                var s2 = version2[i];
                if (s1 > s2) return 1;
                if (s1 < s2) return -1;
            }
            return 0;
        }
        private static int[] GetSplitVersion(string version)
        {
            var trimStr = version.Trim();
            var vLists = trimStr.Split('.');
            var vIntValues = new int[vLists.Length];
            for (var i = 0; i < vIntValues.Length; ++i)
            {
                int.TryParse(vLists[i], out var num);
                vIntValues[i] = num;
            }
            return vIntValues;
        }
    }
}
