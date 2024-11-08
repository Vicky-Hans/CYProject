using Cysharp.Threading.Tasks;
using DH.HotService;
using DH.Log;
using DH.UNet.Utility;
using DHFramework;

namespace DH.Launch
{
    public class HotUpdateResult
    {
        public bool NeedUpdate;
        public bool BigVersion;
        public bool UpdateComplete;
        public bool AddApkDownloadTask;
        public string GameCode;
        public UNetUtility.RspVsn Vsn;
    }
    
    public class CheckHotUpdateTask : TaskBase
    {
        public override async UniTask Start(TaskManager taskMgr, object userData)
        {
            base.Start(taskMgr, userData);

            var curVersion = HotUpdateUtils.GetVersion();
            ULogClient.SetHotUpdateVersion(curVersion);
            
            if (HotUpdateUtils.IsReviewCluster() || !HotUpdateSetting.CheckNeedHotUpdate())
            {
                OnCompleted(false);
                return;
            }

            HotUpdateCheck();
        }

        private void OnCompleted(bool needUpdate, bool bigVersion = false)
        {
            if (UserData is GameConfig config)
            {
                SetResult(new HotUpdateResult()
                {
                    NeedUpdate = needUpdate,
                    BigVersion = bigVersion,
                    GameCode = config.GameCode,
                    
                    Vsn = Usdk.GetVsnConfig(Usdk.RspFullCfg),
                });
            }
            
            IsDone = true;
        }
        
        private void HotUpdateCheck()
        {
            var curVersion = HotUpdateUtils.GetVersion();
            var rspVsn = Usdk.RspFullCfg.vsn;
            
            var updateType = HotUpdateUtils.GetUpdateType(curVersion, rspVsn.vsn);
            
            DHLog.Debug($"[startup]HotUpdateCheck：{curVersion}|{rspVsn.vsn}");
            DHLog.Debug($"[startup]HotUpdateCheck：updateType:{updateType}");

            if (updateType == HotUpdateUtils.UpdateType.Store)
            {
                OnCompleted(true, true);
            }
            else
            {
                var saveMd5 = HotUpdateUtils.GetHotUpdateMD5();
                if (updateType == HotUpdateUtils.UpdateType.NoNeedUpdate ||
                    curVersion.Equals(rspVsn.vsn) ||string.IsNullOrEmpty(rspVsn.md5) || rspVsn.md5 == saveMd5)
                {
                    //如果没有MD5(没有上传版本 / 灰度更新)或者MD5相同(已经更新到最新版本)
                    OnCompleted(false, false);
                }
                else
                {
                    OnCompleted(true, false);
                }
            }
        }
    }
}