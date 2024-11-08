using Cysharp.Threading.Tasks;
using DH;
using DH.HotService;
using DH.HttpsDns;
using DH.NativeCore.Platform;
using DHFramework;
using UnityEngine;
using DHFramework.Localization;

namespace DH.Launch
{
    public class InitSDKTask : TaskBase
    {
        private string gameCode;
        
        public override async UniTask Start(TaskManager taskMgr, object userData)
        {
            base.Start(taskMgr, userData);

            if (userData is GameConfig config)
            {
                gameCode = config.GameCode;
            }
            
            await InitSdk();
        }

        private async UniTask InitSdk()
        {
            var startupConfig = StartupEntry.Instance.StartupConfig;
            
            UsdkConfig config = new UsdkConfig()
            {
                area = DnsArea.Global,
                gameCode = gameCode,
                gameName = startupConfig.GameName,
                language = Localization.GetCurrentLanguage(),
                channel = startupConfig.EnableRelease ? "Release" : "UnityDev",
                vsn = HotUpdateUtils.GetVersion()
            };
            
            await Usdk.Init(config);
            AttSystem.Init();
            SetResult(UserData);
            IsDone = true;
        }
    }
}