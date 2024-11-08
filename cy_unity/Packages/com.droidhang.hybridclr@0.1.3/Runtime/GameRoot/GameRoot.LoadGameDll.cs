using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DH.Asset;
using HybridCLR;
using UnityEngine;

namespace DHHybridCLR.Scripts
{
    public partial class GameRoot
    {
        /// <summary>
        /// 加载某一个热更新的dll
        /// </summary>
        /// <param name="hotfixDllName"></param>
        /// <param name="loadCallback"></param>
        /// <returns></returns>
        public IEnumerator LoadGameDll(string hotfixDllName, Action<System.Reflection.Assembly> loadCallback)
        {
            DHFramework.DHLog.Debug($"[GameRoot::LoadGameDll] Start load {hotfixDllName}.dll");
#if UNITY_EDITOR
            var gameDll = AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == hotfixDllName);
            yield return null;
            loadCallback?.Invoke(gameDll);
#else
            if (!StartupConfig.EnableHybridCLR)
            {
                var gameDll = AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == hotfixDllName);
                loadCallback?.Invoke(gameDll);
                yield break;
            }

            yield return LoadData($"{hotfixDllName}.bytes", "hotfix", dict =>
            {
                if (dict == null)
                {
                    DHFramework.DHLog.Debug("[GameRoot::LoadGameDll] dict is null!");
                    loadCallback?.Invoke(null);
                    return;
                }

                if (!dict.TryGetValue(hotfixDllName, out var hotfixData))
                {
                    DHFramework.DHLog.Debug($"[GameRoot::LoadGameDll] can't find {hotfixDllName}.dll!");
                    loadCallback?.Invoke(null);
                    return;
                }

                if (hotfixData == null || hotfixData.Length == 0)
                {
                    DHFramework.DHLog.Debug($"[GameRoot::LoadGameDll] can't find {hotfixDllName}.dll is empty");
                    loadCallback?.Invoke(null);
                    return;
                }

                var gameDll = System.Reflection.Assembly.Load(hotfixData);
                DHFramework.DHLog.Debug($"[GameRoot::LoadGameDll] load {hotfixDllName}.dll finish");
                loadCallback?.Invoke(gameDll);
            }, DHAssetsConfig.ReadOnlyPath);
#endif
        }
    }
}
