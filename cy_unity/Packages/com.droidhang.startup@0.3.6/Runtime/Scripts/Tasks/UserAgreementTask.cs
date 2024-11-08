using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH;
using DH.Asset;
using DH.HotService;
using DH.NativeCore;
using DHFramework;
using UnityEngine;

namespace DH.Launch
{
    public class UserAgreementTask : TaskBase
    {
        private const string NeedShowAgreementKey = "need_show_agreement";
        private string agreementShowKey = "is_apply_user_protocol";
        private StartupLauncherConfig userAgreementConfig;
        private GameObject AgreementDlgUIObj;
        
        public override async UniTask Start(TaskManager taskMgr, object userData)
        {
            base.Start(taskMgr, userData);
            
            userAgreementConfig = Resources.Load<StartupLauncherConfig>("StartupLauncherConfig");
            
            int hasShow = DHUnityUtil.PlayerPrefs.GetInt(agreementShowKey, 0);
            var needShowUI = hasShow == 0;

            if (!needShowUI)
            {
                needShowUI = GetNeedShowAgreement();
                SetNeedShowAgreement(false);
            }

            if (needShowUI)
            { 
                await ShowAgreementUI();
            }
            else
            {
                CheckCurLanguageAgreementEtag(true).Forget();
                FinishTask();
            }
        }

        private void FinishTask()
        {
            SetResult(UserData);
            
            IsDone = true;
        }

        private async UniTask ShowAgreementUI()
        {
            var path = StartupEntry.Instance.StartupConfig.UserAgreementUIPath;
            AgreementDlgUIObj = await AssetsManager.InstantiateWithParentAsync(path, StartupEntry.Instance.CanvasRootTrans, false);
            
            UserAgreementDlg.Instance.InitUI(OnGetAgreementResult);
        }
        
        /// <summary>
        /// 当有改变时设置本地变量为1，下次登录时弹出用户协议弹窗
        /// </summary>
        /// <param name="flag"></param>
        private void SetNeedShowAgreement(bool flag)
        {
            DHUnityUtil.PlayerPrefs.SetInt(NeedShowAgreementKey, flag ? 1 : 0);
            DHUnityUtil.PlayerPrefs.Save();
        }

        /// <summary>
        /// 获取是否需要弹窗的标志
        /// </summary>
        /// <returns></returns>
        private bool GetNeedShowAgreement()
        {
            return DHUnityUtil.PlayerPrefs.GetInt(NeedShowAgreementKey, 0) == 1;
        }
        
        /// <summary>
        /// 检查协议链接的etag
        /// </summary>
        /// <param name="setNeedShowUI"></param>
        private async UniTaskVoid CheckCurLanguageAgreementEtag(bool setNeedShowUI)
        {
            var userUrl = StartupEntry.Instance.GetUserAgreementUrl();
            var etag = await Usdk.GetUrlEtagWrap(userUrl);
            OnGetUrlEtag(etag, userUrl, setNeedShowUI);
            var privacyUrl = StartupEntry.Instance.GetPrivacyAgreement();
            etag = await Usdk.GetUrlEtagWrap(privacyUrl);
            OnGetUrlEtag(etag, privacyUrl, setNeedShowUI);
        }

        private void OnGetAgreementResult(bool agree)
        {
            DHUnityUtil.PlayerPrefs.SetInt(agreementShowKey, agree ? 1 : 0);
            DHUnityUtil.PlayerPrefs.Save();
            if (agree)
            {
                CheckCurLanguageAgreementEtag(false).Forget();
                
                DestroyAgreementDlg();
                FinishTask();
            }
            else
            {
                StartupLauncher.Quit();
            }
        }

        private void OnGetUrlEtag(string etag, string url, bool setShowFlag)
        {
            if (!string.IsNullOrEmpty(etag)) //不为空的etag才是有效的，
            {
                string localKey = DHUtility.Format("{0}_{1}", url, StartupEntry.Instance.GetCurLanguageCode());
                string localEtag = DHUnityUtil.PlayerPrefs.GetString(localKey, "");
                DHUnityUtil.PlayerPrefs.SetString(localKey, etag);
                DHUnityUtil.PlayerPrefs.Save();

                if (!localEtag.Equals(etag) && setShowFlag)
                {
                    SetNeedShowAgreement(true);
                }
            }
        }
        
        private void DestroyAgreementDlg()
        {
            if (AgreementDlgUIObj)
            {
                AssetsManager.Release(AgreementDlgUIObj);
                AgreementDlgUIObj = null;
            }
        }
    }
}
