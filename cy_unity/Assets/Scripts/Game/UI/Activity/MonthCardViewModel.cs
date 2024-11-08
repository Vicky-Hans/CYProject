using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using DHFramework;
using UnityEngine.Scripting;


namespace DH.Game.ViewModels
{
    public enum MonthCardShowType
    {  
        TimeCard,
        PermanentCard,
    }

    public partial class MonthCardViewModel : ViewModelBase
    {
        public MonthCardItemViewModel MonthCardItem;
        public PrivilegeMonthCardItemViewModel PrivilegeMonthCardItem;
        public CommonTopViewModel CommonTopVieVm;
        [AutoNotify] private MonthCardShowType showType = MonthCardShowType.TimeCard;
        [AutoNotify] private BottomComponentViewModel bottomComponentVm;
        [AutoNotify] private ObservableList<BottomOpCellItemViewModel> opScrollViewList = new();
        [AutoNotify] private ObservableList<CellItemViewModel> awardScrollviewList = new();
        public MonthCardData Data = DataCenter.monthCardData;
        public BtnPriceNodeModel PriceNodeModel;
        [AutoNotify] private ObservableList<PermanentCardPropertyViewModel> effectDesList = new();
        [Preserve]
        public MonthCardViewModel()
        {
            MonthCardItem = new MonthCardItemViewModel();
            PrivilegeMonthCardItem = new PrivilegeMonthCardItemViewModel();
            if (Data.DoubleStatus!=2)
                DoubleAwardScrollView();
            List<int> list = new List<int>() {(int)GameConst.ItemIdCode.EnergyDrink, (int)GameConst.ItemIdCode.Money, (int)GameConst.ItemIdCode.Stone};
            CommonTopVieVm = new(list);
            InitOpBtnList();
            BottomComponentVm = new(CloseUI, opScrollViewList);
            OnClickOpBtnCallback(ShowType);
            InitPermanent();

        }
        

        private void InitOpBtnList()
        {
            OpScrollViewList.Clear();
            foreach (MonthCardShowType type in Enum.GetValues(typeof(MonthCardShowType)))
            {
                BottomOpCellData curData = new();
                switch (type)
                {       
                    case MonthCardShowType.PermanentCard:
                     {
                         curData.ChooseBgPath = "common[common_panel_10]";
                         curData.ChooseIconPath = "monthly[monthly_icon_2]";
                         curData.OpName = ConfigCenter.MonthlyVipMainLanguageCfgColl.GetDataById((int)MonthType.PermanentCard).Name;
                         curData.OnClickCallback = OnClickOpBtnCallback;
                         curData.OpType = type; 
                     } break;
                    case MonthCardShowType.TimeCard:
                    {
                        curData.ChooseBgPath = "common[common_panel_10]";
                        // curData.ChooseIconPath = "mainui[icon_home_1]";
                        curData.ChooseIconPath = "mainui[icon_home_2]";
                        curData.OpName = LocalizeHelper.GetGlobal(GlobalLanguageId.MonthlyVip_tips01);
                        curData.OnClickCallback = OnClickOpBtnCallback;
                        curData.OpType = type;
                    } break;
                    default:
                    {
                        DHLog.Error($"InitOpBtnList 未处理的枚举类型 {type} 请及时处理");
                    } break;   
                }
                BottomOpCellItemViewModel tempVm = new(curData);
                OpScrollViewList.Add(tempVm);
            }
        }
        private void OnClickOpBtnCallback(Object type)
        {
            foreach (var item in OpScrollViewList)
            {
                item.CurOpCellData.IsChoose = (int)item.CurOpCellData.OpType == (int)type;
            }
            ShowType = (MonthCardShowType)type;
        }


        #region 终身卡

        private void InitPermanent()
        {
            EffectDesList.Clear();
            var cfg = ConfigCenter.MonthlyVipMainCfgColl.GetDataById((int)MonthType.PermanentCard);
            for (int i = 0; i < cfg.EffectId.Count; i++)
            {
                var effectId = cfg.EffectId[i];
                var effectCfg = ConfigCenter.MonthlyVipEffectLanguageCfgColl.GetDataById(effectId);
                var effectCfg2 = ConfigCenter.MonthlyVipEffectCfgColl.GetDataById(effectId);
                var desTemplate = Data.OutPutEffectDes(effectCfg.Dec, effectCfg2.Value);
                EffectDesList.Add(new PermanentCardPropertyViewModel((MonthCardEffectType)effectId,desTemplate));
            }
            
            var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(cfg.PackageId);
            var cfgInfo = ConfigCenter.RechargeCfgColl.GetDataById(packageCfg.RechargeId);
            string priceStr = "";
            if (cfgInfo != null) priceStr = PayController.Instance.GetLocalizePrice(cfgInfo.PayId);
            PriceNodeModel = new BtnPriceNodeModel(priceStr);
        }

        [Command]
        private  void BuyPermanentBtn()
        {
            if (Data.IsPermanent)return;
            
            var cfg = ConfigCenter.MonthlyVipMainCfgColl.GetDataById((int)MonthType.PermanentCard);
            ShopManager.Instance.SendBuyPackageBuyRecharge(cfg.PackageId,null,0,-1,1, (rewardList,costList) =>
            {
                Data.LifetimeCard = true;
                Lodash.DealRewards(rewardList); 
                Lodash.DealRewards(costList,false); 
                //UIHelper.OpenCommonRewardView(rewardList.ToList());
                Data.AddSweepCount(MonthType.PermanentCard);
                Data.AddMaxPatrolTimes(MonthType.PermanentCard);
                UIManager.Instance.OpenDialog<MonthCardAwardShowView>(new MonthCardAwardShowViewModel(cfg,rewardList)).Forget();
            });
        }


        #endregion


        #region 双月卡奖励

        /// <summary>
        /// 双月卡奖励
        /// </summary>
        public void DoubleAwardScrollView()
        {
            AwardScrollviewList.Clear();
            var awards = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.MothlyVip_both).Reward;
            for (int i = 0; i < awards.Count; i++)
            {
                var vm = CellItemViewModel.Create(awards[i],ECellItemSizeType.Size120X100);
                vm.State = Data.DoubleStatus == 1 ? ECellItemState.GetIng : ECellItemState.None;
                AwardScrollviewList.Add(vm);
            }
        }

        [Command]
        private async void OnClickGetDoubleAwardButton()
        {
            if (Data.DoubleStatus != 1)return;
            
            var req = new ReqMonthCardDoubleClaim();
            var result = await GameNetworkManager.Instance.SendAsync<RspMonthCardDoubleClaim>(req);
            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                Lodash.DealRewards(result.rsp.Reward.ToList());
                UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList());
                Data.DoubleStatus = 2;
            }
        }
        
        
        #endregion
        


        public void CloseUI()
        {
            UIManager.Instance.CloseDialog<MonthCardView>();
        }
        
        protected override void OnDispose()
        {
            MonthCardItem.Dispose();
            PrivilegeMonthCardItem.Dispose();
            CommonTopVieVm.Dispose();
            base.OnDispose();
        }
        
    }
}