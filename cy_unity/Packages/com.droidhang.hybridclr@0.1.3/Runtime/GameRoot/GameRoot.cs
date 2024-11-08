using System;
using System.Collections;
using DHFramework;
using UnityEngine;
using UnityEngine.LowLevel;

namespace DHHybridCLR.Scripts
{
    public partial class GameRoot : MonoBehaviour
    {
        public static GameRoot Instance = null;

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
                yield return LoadMetadataForAotAssembly("base.bytes", "base");
#endif
                }
            }

            using (new Benchmark("[GameRoot]Startup"))
            {
                yield return LoadGameDll("Startup", RunDll);
            }
#endif

        }
        
        private void RunDll(System.Reflection.Assembly gameDll)
        {
            DHFramework.DHLog.Debug("[GameRoot::RunDll] Begin");

            if (gameDll == null)
            {
                DHFramework.DHLog.Debug($"[GameRoot::RunDll] Dll is null");
                return;
            }

            var appType = gameDll.GetType("DH.Launch.StartupLauncher");
            if (appType == null)
            {
                DHFramework.DHLog.Debug($"[GameRoot::RunDll] DH.Launch.StartupLauncher is null");
                return;
            }


            var mainMethod = appType.GetMethod("LaunchEntry");
            if (mainMethod == null)
            {
                DHFramework.DHLog.Debug($"[GameRoot::RunDll] LaunchEntry is null");
                return;
            }

            mainMethod.Invoke(null, null);
        }

    }
}
