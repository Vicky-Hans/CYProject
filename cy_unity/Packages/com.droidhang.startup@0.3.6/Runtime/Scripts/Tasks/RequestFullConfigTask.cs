using Cysharp.Threading.Tasks;
using DH.HotService;
using DH.NativeCore;
using DH.UNet.Utility;
using DHFramework;
using UnityEngine;

namespace DH.Launch
{
    public class RequestFullConfigTask : TaskBase
    {
        public override async UniTask Start(TaskManager taskMgr, object userData)
        {
            await base.Start(taskMgr, userData);

            int sid = HotUpdateManager.Instance.GetCacheSid();
            DHLog.Debug($"[startup][RequestVsnTask]获取到缓存的分服热更新sid:{sid}");
            DHLog.Debug($"[startup][RequestVsnTask]当前包名：{Application.identifier}");
            DHLog.Debug($"[startup][RequestVsnTask]整包版本号：{Application.version}");
            DHLog.Debug($"[startup][RequestVsnTask]loginCenterConfig:{Usdk.LoginCenterConfig}");
            
            Usdk.GetLoginCenterFullConfigCallback(sid, OnResponseFullConfig);
        }

        private void OnResponseFullConfig(RspFullCfg rsp)
        {
            var json = DHFramework.DHUtility.Json.ToJson(rsp);
  
            DHLog.Debug($"[startup][RequestConfigTask][GetLoginCenterFullConfig]{json}");
            
            if (!string.IsNullOrEmpty(rsp.errmsg))
            {
                DHLog.Error($"[startup][RequestConfigTask] {rsp.errmsg}");
            }

            UNetUtility.RspVsn vsn = Usdk.GetVsnConfig(rsp);

            if (vsn.isSuccess)
            {
                StartupEntry.Instance.SendULogEvent("2100210042", "configure_success");
                
                SetResult(UserData);
                IsDone = true;
            }
            else
            {
                Owner.OpenMessageBox(LanguageId.DialogTitle, LanguageId.ConfigError, ReStartTask, yesBtnText:LanguageId.Retry);
                
                DHLog.Error("[startup]获取登陆中心配置失败");
            }
        }
    }
}
