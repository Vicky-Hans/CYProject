using System;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Commands;
using DH.UIFramework.Observables;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class AdFreePlusPrivilegeMonthCardItemViewModel : ViewModelBase
    {
       
        [AutoNotify] private ObservableList<CellItemBaseViewModel> awardScrollviewList = new();
        [AutoNotify] private CellItemBaseViewModel atOnceAwardCellVm;
        [AutoNotify] private BtnPriceNodeModel btnPriceNodeVm;
        private MonthCardData Data => DataCenter.monthCardData;
        private MonthlyVipMainCfg MonthCard;
        [AutoNotify] private string titleStr;
        [AutoNotify] private bool isShowGrayButton;
        
        private readonly SimpleCommand<Tuple<Vector3, Vector3>> clickIconBtn;
        public ICommand OnClickIconBtn => clickIconBtn;
        
        [Preserve]
        public AdFreePlusPrivilegeMonthCardItemViewModel()
        {
            MonthCard =
                ConfigCenter.MonthlyVipMainCfgColl.GetDataById((int)MonthType.PrivilegeMonthCard);

            TitleStr = ConfigCenter.MonthlyVipMainLanguageCfgColl
                .GetDataById((int)MonthType.PrivilegeMonthCard).Name;
            AwardScrollviewList.Clear();
            for (int i = 0; i < MonthCard.DaiyReward.Count; i++)
            {
                var cellModel = CellItemBaseViewModel.Create(MonthCard.DaiyReward[i]);
                cellModel.SetSize(ECellItemSizeType.Size90X80);
                AwardScrollviewList.Add(cellModel);
            }

            AtOnceAwardCellVm = CellItemBaseViewModel.Create(MonthCard.Reward[0]);
            AtOnceAwardCellVm.SetSize(ECellItemSizeType.Size90X80);

            var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(MonthCard.PackageId);
            var cfgInfo = ConfigCenter.RechargeCfgColl.GetDataById(packageCfg.RechargeId);
            string priceStr = "";
            if (cfgInfo != null) priceStr = PayController.Instance.GetLocalizePrice(cfgInfo.PayId);
            BtnPriceNodeVm = new BtnPriceNodeModel(priceStr);
            IsShowGrayButton = Data.IsMonthVipPlus;
            clickIconBtn = new SimpleCommand<Tuple<Vector3, Vector3>>(OnClickInfoBtn);
        }
        
        [Command]
        private void OnClickBuyButton()
        {
            ShopManager.Instance.SendBuyPackageBuyRecharge(MonthCard.PackageId, null, 0, -1, 1,
                (rewardList, costList) =>
                {
                    OnBuyCallback(rewardList, costList).Forget();
                    IsShowGrayButton = Data.IsMonthVipPlus;
                });
        }

        private async UniTask OnBuyCallback(List<Resource> rewardList, List<Resource> costList)
        {
            Lodash.DealRewards(rewardList, costList);
            Data.OnBuyPlus();
            if (Data.IsCanGetAward(MonthType.PrivilegeMonthCard) &&
                !Data.ToDayIsGetAward(MonthType.PrivilegeMonthCard))
            {
                var req = new ReqMonthCardClaim();
                req.Op = MonthCard.Id;
                var tempRes = await GameNetworkManager.Instance.SendAsync<RspMonthCardClaim>(req);
                if (tempRes.rsp.Status == 0)
                {
                    Lodash.DealRewards(tempRes.rsp.Reward.ToList());
                    Data.GetTodayAward(MonthType.PrivilegeMonthCard);
                    List<Resource> tempRewards = new List<Resource>(rewardList);
                    List<Resource> tempRewards2 = new List<Resource>(tempRes.rsp.Reward);
                    var tempRewards3 = UIHelper.MergeLists(tempRewards, tempRewards2);
                    UIManager.Instance
                        .OpenDialog<MonthCardAwardShowView>(
                            new MonthCardAwardShowViewModel(MonthCard, tempRewards3, true))
                        .Forget();

                }
                else
                {
                    ToastManager.Show(UIHelper.GetNetErrorMessage(tempRes.rsp.Status));
                }
            }
            else
            {
                UIManager.Instance
                    .OpenDialog<MonthCardAwardShowView>(
                        new MonthCardAwardShowViewModel(MonthCard, rewardList)).Forget();
            }

            if (Data.IsMonthVip && Data.DoubleStatus != 2)
            {
                Data.DoubleStatus = 1;
            }
        }
        public void OnClickInfoBtn(Tuple<Vector3, Vector3> info)
        {
            var cfg = ConfigCenter.MonthlyVipMainCfgColl.GetDataById((int)MonthType.PrivilegeMonthCard);
            var OtherEffectDes = string.Empty;
            var index = 1;
            for (int i = 0; i < cfg.EffectId.Count; i++)
            {
                var effectId = cfg.EffectId[i];
                if (effectId == (int)MonthCardEffectType.AdRreeReward)continue;
                var effectCfg = ConfigCenter.MonthlyVipEffectLanguageCfgColl.GetDataById(effectId);
                var effectCfg2 = ConfigCenter.MonthlyVipEffectCfgColl.GetDataById(effectId);
                var desTemplate = Data.OutPutEffectDes(effectCfg.Dec, effectCfg2.Value);
                var isEnd = i == cfg.EffectId.Count - 1;
                OtherEffectDes += $"{index}. "+ desTemplate + (isEnd?"":"\n");
                index++;
            }
            UIHelper.OpenCommonTips (LocalizeHelper.GetGlobal(GlobalLanguageId.MonthlyVip_tips16),
                OtherEffectDes,info.Item1,info.Item2);
        }
    }
}