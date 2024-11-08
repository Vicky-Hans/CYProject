#if !UNITY_WEBGL
using Cysharp.Threading.Tasks;
using DH.HotService;
using DH.NativeCore.Platform;
using DHFramework;
using UnityEngine;
using DHFramework.Download;


namespace DH.Launch
{
    public class HotUpdateTask : TaskBase
    {
        private bool startDownload = false;
        private HotUpdateResult config;
        public override async UniTask Start(TaskManager taskMgr, object userData)
        {
            base.Start(taskMgr, userData);
            
            config = userData as HotUpdateResult;
            if (config == null)
            {
                DHLog.Error("[startup][HotUpdateTask]UserData 转化失败");
                return;
            }

            StartupEntry.Instance.SendULogEvent("2100210029", "update_start");
            
            if (!config.NeedUpdate)
            {
                OnCompleted();
                return;
            }

            if (config.BigVersion)
            {
                config.BigVersion = false;
                config.AddApkDownloadTask = true;
                OnCompleted();
                return;
            }
            
            StartClientUpdate();
        }

        private void OnCompleted()
        {
            if (IsDone)
            {
                return;
            }

            UDownload.CallSdkFunction = null;
            UDownload.CallSdkService = null;

            if (startDownload)
            {
                DHLog.Debug("[startup][HotUpdateTask][OnCompleted] 反注册回调");
                // 反注册回调
                UDownload.OnDownloadUpdate -= ProgressCallback;
                UDownload.OnDownloadCompleted -= CompleteCallback;
                UDownload.OnNetworkChangeToCarrier -= OnNetworkChangeToCarrier;
                UDownload.OnConfirmDownloadInCarrier -= OnConfirmDownloadInCarrier;
                UDownload.OnGroupDownloadCompleted -= GroupDownloadCompleted;
            }
            
            if (UserData != null && UserData is HotUpdateResult updateResult)
            {
                updateResult.UpdateComplete = true;
            }

            StartupEntry.Instance.SendULogEvent("2100210004", "update_success");
            SetResult(UserData);
            IsDone = true;
        }

        private void StartClientUpdate()
        {
            DHLog.Debug("[startup][HotUpdateTask][StartClientUpdate]");

            UDownload.ForceDisableRangeDownload = true;
            UDownload.CallSdkFunction = Usdk.CallFunction;
            UDownload.CallSdkService = Usdk.CallService;

            int maxAgentCount = 8;

            if (HotUpdateUtils.IsEmulator())
            {
                maxAgentCount = 4;
            }
            
            var result = UDownload.InitDownload(config.Vsn, config.GameCode, maxAgentCount);
            
            if (!result)
            {
                HotUpdateManager.Instance.ClearCacheSid();
                OnCompleted();
                return;
            }

            UDownload.SetCheckAvailableSpace(DeviceUtility.GetAvailableSpace);

            startDownload = true;
            UDownload.OnDownloadUpdate += ProgressCallback;
            UDownload.OnDownloadCompleted += CompleteCallback;
            UDownload.OnNetworkChangeToCarrier += OnNetworkChangeToCarrier;
            UDownload.OnConfirmDownloadInCarrier += OnConfirmDownloadInCarrier;
            UDownload.OnGroupDownloadCompleted += GroupDownloadCompleted;

            //启动热更新状态检测Manager
            HotUpdateManager.Instance.StartDownload(Usdk.RspFullCfg.vsn);
            UDownload.StartDownload(false);
        }

        #region 回调部分

        private void ProgressCallback(DetailInfo info)
        {
            Owner.OnDownloadProgress(info);
        }

        private void OnNetworkChangeToCarrier()
        {
            DHLog.Debug("[startup][HotUpdateTask]OnNetworkChangeToCarrier");
            
            Owner.OpenMessageBox(LanguageId.DialogTitle, LanguageId.DownloadInCarrierTip, 
                ResumeDownload, QuitGame,
                yesBtnText:LanguageId.Confirm, LanguageId.Exit);
        }

        private void OnConfirmDownloadInCarrier(long size, bool needUpdateFirstPackage)
        {
            DHLog.Debug($"[startup][HotUpdateTask]OnConfirmDownloadInCarrier：{size}");
            
            Owner.OpenMessageBox(LanguageId.DialogTitle, LanguageId.DownloadInCarrierTip, 
                ConfirmStartInCarrierNet, QuitGame,
                yesBtnText:LanguageId.Confirm, LanguageId.Exit);
        }

        private void GroupDownloadCompleted(int group, bool success)
        {
            DHLog.Debug($"[startup][HotUpdateTask]GroupDownloadCompleted group :{group} success:{success}");

            if (group == 2 && success)
            {
                UDownload.SwitchToBackground(1);
                UDownload.StartSilenceUpdate();

                OnCompleted();
            }
        }

        private void CompleteCallback(bool result, int errorCode)
        {
            DHLog.Debug("[startup][HotUpdateTask][CompleteCallback] result:{result}, status: {errorCode}");
     
            if (result)
            {
                OnCompleted();
            }else if (errorCode <= ErrorCode.UnknownNetworkError ||
                      errorCode == ErrorCode.NetworkUnreachable ||
                      errorCode == ErrorCode.DownloadTimeout)
            {
                //网络不好
                Owner.OpenMessageBox(LanguageId.DialogTitle, LanguageId.NetworkError, RetryDownload, QuitGame, yesBtnText:LanguageId.Retry, LanguageId.Exit);
            }else if (errorCode == ErrorCode.StorageSpaceNotEnough)
            {
                //空间不够
                Owner.OpenMessageBox(LanguageId.DialogTitle, LanguageId.NoStorageError, RetryDownload, QuitGame, yesBtnText:LanguageId.Retry, LanguageId.Exit);
            }else if (errorCode == 10000)
            {
                //无法重试的错误
                Owner.OpenMessageBox(LanguageId.DialogTitle, LanguageId.ResourceUpdateFailed, QuitGame, yesBtnText:LanguageId.Exit);
            }
            else
            {
                //下载失败
                Owner.OpenMessageBox(LanguageId.DialogTitle, LanguageId.ResourceUpdateFailed, QuitGame, yesBtnText:LanguageId.Exit);
            }
        }

        #endregion
        
        /// <summary>
        /// 重试
        /// </summary>
        private void RetryDownload()
        {
            HotUpdateManager.Instance.RetryDownload();
        }
        
        /// <summary>
        /// 恢复下载
        /// </summary>
        private void ResumeDownload()
        {
            UDownload.ResumeDownload();
        }
        
        /// <summary>
        /// 确认是否在蜂窝网络下下载
        /// </summary>
        private void ConfirmStartInCarrierNet()
        {
            UDownload.ConfirmStartInCarrierNet();
        }
        
        /// <summary>
        /// 退出游戏
        /// </summary>
        private void QuitGame()
        {
            StartupLauncher.Quit();
        }
    }
}

#endif
