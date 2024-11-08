using System;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using DHFramework;
using DHFramework.Localization;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class MailCellItemViewModel:ViewModelBase
    {
        [AutoNotify] private string titleStr;
        [AutoNotify] private string timeStr;
        [AutoNotify] private bool isHaveGift;
        [AutoNotify] private bool isHaveGiftOpen;
        [AutoNotify] private bool isNew;
        [AutoNotify] private MailInfo baseMailInfo;
        [AutoNotify] private Action closeCallBack;
        [AutoNotify] private bool isDel;

        public MailCellItemViewModel(MailInfo mailInfo)
        {
            IsDel = false;
            baseMailInfo = mailInfo;
            UpdataMailInfo();
        }

        public void UpdataMailInfo()
        {
            var curLanguageCode = Localization.GetCurrentLanguageNumber();
            TitleStr = DataCenter.maildata.GetMailTitle(baseMailInfo.Id,curLanguageCode);
            var nowTime = Lodash.GetUnixTime();
            TimeStr =  UIHelper.ConvertTimeSecondToString(nowTime - baseMailInfo.SendTime, ETimeFormatType.TimeFormatTypeMail);
            var mIsHaveGift = DataCenter.maildata.GetMailIsHaveRewards(baseMailInfo.Id);
            var readState = DataCenter.maildata.GetMailIsRead(baseMailInfo.Id);
            if (readState == EMailReadState.MailStateUnRead)
            {
                IsNew = true;
            }
            else
            {
                IsNew = false;
            }
            var claimState = DataCenter.maildata.GetMailIsClaimRewardsByMailId(baseMailInfo.Id);
            var rewardState = DataCenter.maildata.GetMailIsHaveRewards(baseMailInfo.Id);
            var isGet =   rewardState && claimState == EMailRewardState.MailRewardStateUnClaim;
            if (mIsHaveGift)
            {
                IsHaveGift = isGet;
                IsHaveGiftOpen = !isGet;
            }
            else
            {
                IsHaveGift = IsHaveGiftOpen =false;
            }
        }

        [Command]
        private async void OnClickMailBtn()
        {

            // ActivityManager.Instance.Show(WaitType.Net);
            var req = new ReqMailRead();
            req.MailId = (int)baseMailInfo.Id;
            var result = await GameNetworkManager.Instance.SendAsync<RspMailRead>(req);
            // ActivityManager.Instance.Hide(WaitType.Net);
            if (result.rsp ==null || result.rsp.Status != 0)
            {
                DHLog.Debug($"邮件读取 {result.rsp.Status}");
                return;
            }
            
            DataCenter.maildata.SetMailIsRead(baseMailInfo.Id);
            var tempVm = new MailInfoViewModel(baseMailInfo);
            tempVm.CloseCallBack = () =>
            {
                UpdataMailInfo();
                CloseCallBack?.Invoke();
            };
            await UIManager.Instance.OpenDialog<MailInfoView>(tempVm);
        }
    }
}