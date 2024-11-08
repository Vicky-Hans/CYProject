using DH.Asset;
using DH.HotService;
using DH.NativeCore;
using DHFramework;
using UnityEngine;

namespace DH.Launch
{
    /// <summary>
    /// iOS att权限接入
    /// </summary>
    public static class AttSystem
    {
        public static long AttOpenTime
        {
            get
            {
                var timeStr = DHUnityUtil.PlayerPrefs.GetString(attOpenTime, "0");
                return long.Parse(timeStr);
            }
            set => DHUnityUtil.PlayerPrefs.SetString(attOpenTime, value.ToString());
        }

        public static int AttAfterHasOpen
        {
            get => DHUnityUtil.PlayerPrefs.GetInt(attAfterHasOpen, 0);
            set => DHUnityUtil.PlayerPrefs.SetInt(attAfterHasOpen, value);
        }
        
        //ATT系统弹窗时间
        private static string attOpenTime = "att_open_time";
        //ATT后置弹窗是否打开过
        private static string attAfterHasOpen = "att_after_has_open";
        
        public static void Init()
        {
            if (IsIOS())
            {
                //注册回调
                Usdk.Subscribe("Utils_openATTSettingsCallback", OnOpenATTSettings);
                DHGameActivityUtil.OnRequestIOSAttComplete += AttCompleteCallback;
                CheckOpenAttWindow();
            }
        }

        public static void Release()
        {
            if (IsIOS())
            {
                //反注册回调
                Usdk.Unsubscribe("Utils_openATTSettingsCallback", OnOpenATTSettings);
                DHGameActivityUtil.OnRequestIOSAttComplete -= AttCompleteCallback;
            }
        }

        /// <summary>
        /// att检查，
        /// </summary>
        /// <returns>true要显示ui</returns>
        public static bool AttCheck()
        {
            if (!IsIOS())
            {
                return false;
            }

            if (HotUpdateUtils.IsReviewCluster())
            {
                DHLog.Debug("[AttSystem:ATTCheck]:审核服不弹窗");
                return false;
            }

            var status = GetAttStatus();

            if (status == 2)
            {
                if (AttAfterHasOpen == 1)
                {
                    DHLog.Debug($"[AttSystem:ATTCheck]:hasOpen不弹窗");
                    return false;
                }

                var curTime = DHUtility.GetGameTime(DHUtility.TicksType.S);
                if (curTime - AttOpenTime < 86400)
                {
                    DHLog.Debug($"[AttSystem:ATTCheck]:opentime不弹窗");
                    return false;
                }
                
                return true;
            }
            
            DHLog.Debug($"[AttSystem:ATTCheck]:status不弹窗 {status}");
            return false;
        }
        
        /// <summary>
        /// GoToATTSetting 跳转到ATT设置
        /// </summary>
        public static void GoToAttSetting()
        {
            Usdk.CallService("Utils_openATTSettings", "");
        }

        private static void CheckOpenAttWindow()
        {
            var status = GetAttStatus();

            if (status == 0)
            {
                DHLog.Debug("[AttSystem.CheckOpenAttWindow]弹窗前");
                DHGameActivityUtil.RequestIOSAtt();
                DHLog.Debug("[AttSystem.CheckOpenAttWindow]弹窗后");
            }
            else
            {
                DHLog.Debug("[AttSystem:CheckOpenAttWindow]:status ~= 0不弹窗");
            }
        }

        private static int GetAttStatus()
        {
            var statusStr = Usdk.CallFunction("Utils_checkATTOpened", "");
            
            if(!int.TryParse(statusStr, out var status))
            {
                status = 3;
            }

            return status;
        }

        /// -1表示请求失败；
        /// 0表示ios原生返回的未决定；
        /// 1表示ios原生返回的受限制；
        /// 2表示ios原生返回的拒绝；
        /// 3表示ios原生返回的已授权；
        /// 低于ios 14版本，一直返回3
        private static void OnOpenATTSettings(string json)
        {
            DHLog.Debug($"[AttSystem:OnOpenATTSettings]:{json}");
        }
        
        //AttCompleteCallback ATT弹窗完成的底层回调
        private static void AttCompleteCallback(int status)
        {
            DHLog.Debug($"[AttSystem:AttCompleteCallback]:{status}");
            AttOpenTime = DHUtility.GetGameTime(DHUtility.TicksType.S);
        }

        private static bool IsIOS()
        {
            return UnityEngine.Application.platform == UnityEngine.RuntimePlatform.IPhonePlayer;
        }
    }
}
