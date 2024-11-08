using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.NativeCore.Platform;
using DHFramework;
using DHFramework.Localization;
using DHHybridCLR.Scripts;
using UnityEngine;

namespace DH.Launch
{
    public partial class StartupEntry
    {
        private void Update()
        {
            OnUpdate();
        }

        private async UniTask ExecuteStartGameAsync()
        {
            await UniTask.WaitForEndOfFrame(this);
            await StartGameAsync();

            if (autoDestroyStartupDlg)
            {
                DestroyStartupPage();
            }
        }

        private async UniTask StartGameAsync()
        {
            using (new Benchmark("[Startup] StartGame"))
            {
                Debug.Log("[GameRoot::RunDll] Begin");
                await Localization.Init(GameRoot.Instance.StartupConfig.LocalizationConfigPath,DeviceUtility.GetLanguage);
#if UNITY_WEBGL || WECHAT_MINI
                WebGLStartGame?.Invoke();
                WebGLStartGame = null;
#else
                System.Reflection.Assembly gameDll = AppDomain.CurrentDomain.GetAssemblies()
                    .First(assembly => assembly.GetName().Name == StartupConfig.StartDllName);

                if (gameDll == null)
                {
                    Debug.LogError($"[GameRoot::RunDll] {StartupConfig.StartDllName} is null");
                    return;
                }

                var appType = gameDll.GetType(StartupConfig.StartTypeName);
                if (appType == null)
                {
                    Debug.LogError($"[GameRoot::RunDll] {StartupConfig.StartTypeName} is null");
                    return;
                }


                var mainMethod = appType.GetMethod(StartupConfig.StartMethodName);
                if (mainMethod == null)
                {
                    Debug.LogError($"[GameRoot::RunDll] {StartupConfig.StartMethodName} is null");
                    return;
                }

                await (UniTask)mainMethod.Invoke(null, null);
#endif
            }
        }
    }
}