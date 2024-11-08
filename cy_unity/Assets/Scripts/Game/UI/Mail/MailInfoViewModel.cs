using System;
using System.Collections.Generic;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using DHFramework.Localization;
using UnityEngine;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{ 
    public partial class MailInfoViewModel : ViewModelBase
    {
        [AutoNotify] private string titleStr;
        [AutoNotify] private string contentStr;
        [AutoNotify] private string opBtnImgPath;
        [AutoNotify] private string opBtnStr;
        [AutoNotify] private Color opBtnStrColor;
        
        [AutoNotify] private ObservableList<CellItemViewModel> awardList = new();
        [AutoNotify] private MailInfo baseMailInfo;
        [AutoNotify] private Action closeCallBack;
        [AutoNotify] private Vector2 contentSize;
        
        private Vector2[] offsetArray = new Vector2[2] { new Vector2(860f, 800f), new Vector2(860f, 1100f) };
        [Preserve]
        public MailInfoViewModel( MailInfo mailinfo)
        {
            baseMailInfo = mailinfo;
            TitleStr = DataCenter.maildata.GetMailTitle(baseMailInfo.Id, Localization.GetCurrentLanguageNumber());
            
            UpdateContentStr();
            UpdateRewardState();
            UpdateOpBtnInfo();
            UpdateContentSize();
        }
        
        public void OnClickCloseBtn()
        {
            CloseCallBack?.Invoke();
            UIManager.Instance.CloseDialog<MailInfoView>();
        }
        [Command]
        private void OnClickOpBtn()
        {
            var claimState = DataCenter.maildata.GetMailIsClaimRewardsByMailId(baseMailInfo.Id);
            if (claimState == EMailRewardState.MailRewardStateClaim)
            {
                DelMail();
            }
            else
            {
                ClaimReward();
            }

        }

        /// <summary>
        /// 文本的显示
        /// </summary>
        private void UpdateContentStr()
        {
            var str = DataCenter.maildata.GetMailContent(baseMailInfo.Id, Localization.GetCurrentLanguageNumber());
            if (baseMailInfo.CfgId != 0)
            {
                var cfg = ConfigCenter.MailCfgColl.GetDataById(baseMailInfo.CfgId);
                if (cfg is { Type: (int)EMailType.MailTypeLink })
                {
                    str = str.Replace("<link=handler>", $"<link={cfg.Hyperlink}>");
                }
            }
            ContentStr = str;
            var cfgLanguage = ConfigCenter.MailLanguageCfgColl.GetDataById(baseMailInfo.CfgId);
            ContentStr = $"<align=left>{ContentStr}</align>\n\n\n\n <align=right>{cfgLanguage.From}  </align>";
        }

        private void UpdateContentSize()
        {
            if (awardList.Count == 0)
            {
                ContentSize = offsetArray[1];
            }
            else
            {
                ContentSize = offsetArray[0];
            }
        }

        private void UpdateRewardState()
        {
            AwardList.Clear();
            var rewards = DataCenter.maildata.GetMailAffixByMailId(baseMailInfo.Id);
            var state = DataCenter.maildata.GetMailIsClaimRewardsByMailId(baseMailInfo.Id);
            foreach (var reward in rewards)
            {
                if (reward.type == (int)RewardType.HeroEquip)
                {
                    var tempVm =new CellItemViewModel(reward.id/100,reward.type,reward.count,ECellItemSizeType.Size166X150,false,true,new HeroEquip()
                    {
                        Id = reward.id/100,
                        Uid = 0,
                        QuaId = reward.id%100,
                        Lv = 1,
                    });
                    tempVm.State = state == EMailRewardState.MailRewardStateClaim? ECellItemState.Finish : ECellItemState.GetIng;
                    AwardList.Add(tempVm);
                }
                else
                {
                    var tempVm =new CellItemViewModel(reward.id,reward.type,reward.count);
                    tempVm.State =state == EMailRewardState.MailRewardStateClaim? ECellItemState.Finish : ECellItemState.GetIng;
                    AwardList.Add(tempVm);
                }


            }
        }

        private void UpdateOpBtnInfo()
        {
            var claimState = DataCenter.maildata.GetMailIsClaimRewardsByMailId(baseMailInfo.Id);
            if (AwardList.Count == 0 || claimState == EMailRewardState.MailRewardStateClaim)
            {
                OpBtnImgPath = $"common[commom_button_yellow]";
                OpBtnStr =LocalizeHelper.GetGlobal(GlobalLanguageId.Mail_buttonName_1);
                //OpBtnStrColor = UIHelper.HexColorStrToColor(DhHexColor.PurpleBtnTextColor);
            }
            else
            {
                OpBtnImgPath = $"common[commom_button_green]";
                OpBtnStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Mail_buttonName_2);
                //OpBtnStrColor = UIHelper.HexColorStrToColor(DhHexColor.BlueBtnConfirmTextColor);
            }
        }

        private async void DelMail()
        {
            // ActivityManager.Instance.Show(WaitType.Net);
            var req = new ReqMailDelete();
            req.MailIds.Add((int)baseMailInfo.Id);
            var result = await GameNetworkManager.Instance.SendAsync<RspMailDelete>(req);
            // ActivityManager.Instance.Hide(WaitType.Net);
            if (result.rsp is not { Status: 0 })
            {
                if(result.rsp == null) return;
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
                return;
            }
            var delList = new List<long>();
            delList.Add(baseMailInfo.Id);
            DataCenter.maildata.DeleteMails(delList);
            UIManager.Instance.CloseDialog<MailInfoView>();
            
        }

        private async void ClaimReward()
        {
            // ActivityManager.Instance.Show(WaitType.Net);
            var req = new ReqMailReward();
            req.MailIds.Add((int)baseMailInfo.Id);
            var result = await GameNetworkManager.Instance.SendAsync<RspMailReward>(req);
            // ActivityManager.Instance.Hide(WaitType.Net);
            if (result.rsp is not { Status: 0 })
            {
                if(result.rsp == null) return;
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
                return;
            }

            
            var changeList = result.rsp.MailIds.ToList();
            
            if(changeList.Count == 0) return;
            
            // 更新邮件状态
            foreach (var id in changeList)
            {
                DataCenter.maildata.SetMailIsClaimRewardsByMailId(id);
            }
            // 给奖励
            Lodash.DealRewards(result.rsp.Reward.ToList());
            UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList(), () =>
            {
                // 更新邮件奖励状态
                UpdateRewardState();
                // 更新按钮状态
                UpdateOpBtnInfo();
                CloseCallBack?.Invoke();
                UIManager.Instance.CloseDialog<MailInfoView>();
            });
        }
    }
}