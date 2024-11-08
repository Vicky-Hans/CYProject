#if !UNITY_WEBGL

using System;
using Cysharp.Threading.Tasks;
using DH.HotService;
using DH.NativeCore.Platform;
using DHFramework;
using DHFramework.Download;

namespace DH.Launch
{
    public class ApkDownloadTask : TaskBase
    {
        private bool startDownload = false;
        private HotUpdateResult config;
        private bool registered = false;
        
        public override async UniTask Start(TaskManager taskMgr, object userData)
        {
            base.Start(taskMgr, userData);

            config = userData as HotUpdateResult;
            if (config == null)
            {
                DHLog.Error("[startup][ApkDownloadTask]UserData 转化失败");
                return;
            }

            StartApkDownload();
        }

        private void OnCompleted()
        {
            UnRegisterCallback();
            IsDone = true;
        }

        private void StartApkDownload()
        {
            DHLog.Debug("[startup][ApkDownloadTask][StartClientUpdate]");
            int maxAgentCount = 8;

            if (HotUpdateUtils.IsEmulator())
            {
                maxAgentCount = 4;
            }

            var success = ApkDownload.StartUpdateApk(config.Vsn, config.GameCode, DeviceUtility.GetAvailableSpace, maxAgentCount);
            if (success)
            {
                RegisterCallback();
            }
            else
            {
                NotSupportApkDownload();
            }
        }

        private void RegisterCallback()
        {
            if (!registered)
            {
                registered = true;


                ApkDownload.OnDownloadUpdate += ProgressCallback;
                ApkDownload.OnDownloadCompleted += CompleteCallback;
                ApkDownload.OnNetworkChangeToCarrier += OnNetworkChangeToCarrier;
                ApkDownload.OnConfirmDownloadInCarrier += OnConfirmDownloadInCarrier;
                ApkDownload.OnConfirmDownload += ConfirmDownload;
            }
        }

        private void UnRegisterCallback()
        {
            if (registered)
            {
                registered = false;
                ApkDownload.OnDownloadUpdate -= ProgressCallback;
                ApkDownload.OnDownloadCompleted -= CompleteCallback;
                ApkDownload.OnNetworkChangeToCarrier -= OnNetworkChangeToCarrier;
                ApkDownload.OnConfirmDownloadInCarrier -= OnConfirmDownloadInCarrier;
                ApkDownload.OnConfirmDownload -= ConfirmDownload;
            }
        }

        #region 回调部分

        private void ProgressCallback(DetailInfo info)
        {
            Owner.OnDownloadProgress(info);
        }


        private void OnNetworkChangeToCarrier()
        {
            DHLog.Debug("[startup][ApkDownloadTask]OnNetworkChangeToCarrier");
            
            Owner.OpenMessageBox(LanguageId.DialogTitle, LanguageId.DownloadInCarrierTip, 
                ResumeDownload, QuitGame,
                yesBtnText:LanguageId.Confirm, LanguageId.Exit);
        }

        private void OnConfirmDownloadInCarrier(long size)
        {
            DHLog.Debug($"[startup][ApkDownloadTask]OnConfirmDownloadInCarrier: {size}");
            
            Owner.OpenMessageBox(LanguageId.DialogTitle, LanguageId.DownloadInCarrierTip, 
                ResumeDownload, QuitGame,
                yesBtnText:LanguageId.Confirm, LanguageId.Exit);
        }

        private void ConfirmDownload(long size)
        {
            DHLog.Debug("[startup][ApkDownloadTask] ConfirmDownload");
            
            Owner.OpenMessageBox(LanguageId.DialogTitle, LanguageId.ConfirmDownloadApk, 
                ConfirmStartInCarrierNet, QuitGame,
                yesBtnText:LanguageId.Confirm, LanguageId.Exit);
        }

        private void CompleteCallback(bool result, int errorCode)
        {
            DHLog.Debug($"[startup][ApkDownloadTask][CompleteCallback] result:{result}, status: {errorCode}");
            OnCompleted();
            
            if (result)
            {
                Owner.OpenMessageBox(LanguageId.DialogTitle, LanguageId.ConfirmInstallTip, 
                    Install, QuitGame, yesBtnText
                    :LanguageId.Confirm, LanguageId.Exit);
            }
            else if (errorCode == ErrorCode.InvalidMd5OrLength || errorCode == ErrorCode.Md5Error)
            {
                NotSupportApkDownload();
            }else if (errorCode <= ErrorCode.UnknownNetworkError ||
                      errorCode == ErrorCode.NetworkUnreachable ||
                      errorCode == ErrorCode.DownloadTimeout)
            {
                //网络错误
                Owner.OpenMessageBox(LanguageId.DialogTitle, LanguageId.NetworkError, 
                    RetryDownload, QuitGame, yesBtnText
                    :LanguageId.Retry, LanguageId.Cancel);
            }else if (errorCode == ErrorCode.StorageSpaceNotEnough)
            {
                //存储空间不足
                Owner.OpenMessageBox(LanguageId.DialogTitle, LanguageId.NoStorageError, 
                    RetryDownload, QuitGame, yesBtnText
                    :LanguageId.Retry, LanguageId.Cancel);
            }
            else
            {
                //下载错误
                Owner.OpenMessageBox(LanguageId.DialogTitle, LanguageId.ApkDownloadFailed, 
                    RetryDownload, QuitGame, yesBtnText
                    :LanguageId.Retry, LanguageId.Cancel);
            }
        }

        #endregion

        private void NotSupportApkDownload()
        {
            var storeUrl = Usdk.RspFullCfg.storeUrl;

            string desc = "";
            Action confirmCallback = null;
            
            if (string.IsNullOrEmpty(storeUrl))
            {
                confirmCallback = QuitGame;
                desc = LanguageId.NewVersionNoUrlTip;
            }
            else
            {
                desc = LanguageId.NewVersionTip;
                confirmCallback = () =>
                {
                    ApkDownload.OpenStoreUrl(storeUrl, DHUtility.Json.ToJson(config.Vsn));
                };
            }
            
            Owner.OpenMessageBox(LanguageId.DialogTitle, desc, confirmCallback, yesBtnText:LanguageId.Update);
        }

        private void Install()
        {
            //提示正在安装
            Owner.OpenMessageBox(LanguageId.DialogTitle, LanguageId.InstallingTip, QuitGame, yesBtnText:LanguageId.Confirm);
            ApkDownload.InstallApk();
        }
        
        /// <summary>
        /// 重试
        /// </summary>
        private void RetryDownload()
        {
            RegisterCallback();
            ApkDownload.RetryDownload();
        }
        
        /// <summary>
        /// 恢复下载
        /// </summary>
        private void ResumeDownload()
        {
            DHLog.Debug("[startup]ResumeDownload");
            ApkDownload.ResumeDownload();
        }
        
        /// <summary>
        /// 确认是否在蜂窝网络下下载
        /// </summary>
        private void ConfirmStartInCarrierNet()
        {
            DHLog.Debug("[startup]Confirm");
            ApkDownload.ConfirmStartInCarrierNet();
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
