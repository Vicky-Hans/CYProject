using System;
using System.Collections.Generic;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Commands;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class BuyPowerItemViewModel : ViewModelBase
    {
        [AutoNotify] private string titleText;
        [AutoNotify] private string numsText;
        [AutoNotify] private string butText;
        [AutoNotify] private bool isFree;
        [AutoNotify] private bool buyOutGo;
        [AutoNotify] private long costPrice;//钻石价格
        private int livesType = 5;
        public Action livesItemOpBtnCallback;
        public int MaxSaveCount;
        public Reward DiamondCost;
        public ICommand OnClickOpBtn { get; set; }
        [Preserve]
        public BuyPowerItemViewModel(int livesKey,Action livesOpBtnCallback)
        {
            livesType = livesKey;
            livesItemOpBtnCallback = livesOpBtnCallback;
            MaxSaveCount = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.StorageLimit).Content[0];
            DiamondCost = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.diamond_hp_price).Reward[0];
            OnClickOpBtn = new AsyncCommand(ClickOpBtn);
            InitUI();
            PlayerInfoManager.Instance.PropertyChanged += PropChange;
        }

        private void PropChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayerInfoManager.Instance.SecondDay))
            {
                InitUI();
            }
        }

        private void InitUI()
        {
            var name = ConfigCenter.ItemLanguageCfgColl
                .GetDataById((int)GameConst.ItemIdCode.EnergyDrink).Name;
            if (livesType == 5)
            {
                IsFree = true;
                NumsText = $"{ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.General_tips24).Name} {DataCenter.livesData.AdTimes}";
                TitleText =$"{name} {GetAdBuyCount()}";
                ButText = ConfigCenter.GlobalLanguageCfgColl
                    .GetDataById(GlobalLanguageId.Shop_tips16).Name;

                BuyOutGo = DataCenter.livesData.AdTimes <= 0;
            }
            else if (livesType == 15)
            {
                IsFree = false;
                NumsText = $"{ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.General_tips24).Name} {DataCenter.livesData.DiamondTimes}";
                TitleText =$"{name} {GetDiamondBuyCount()}";
                CostPrice = DiamondCost.Count;
                ButText = DiamondCost.Count+"";
                BuyOutGo = DataCenter.livesData.DiamondTimes <= 0;
            }
            else
            {
                IsFree = false;
                NumsText = $"{DataCenter.livesData.DiamondStoreTimes}/{MaxSaveCount}";
                TitleText =ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.General_tips25).Name;
                CostPrice = DiamondCost.Count;
                ButText = DiamondCost.Count+"";
                BuyOutGo = DataCenter.livesData.DiamondStoreTimes <= 0;
            }
        }


        private async UniTask ClickOpBtn()
        {
            if (livesType == 5)
            {
                OnClickAdBuy().Forget();
            }
            else if (livesType == 15)
            {
                OnClickDiamondBuy().Forget();
            }
            else
            {
                OnClickExtraBuy().Forget();
            }
        }
        
        public async UniTask OnClickAdBuy()
        {
            if (DataCenter.livesData.AdTimes <= 0)
            {
                string contentStr = "";
                var config = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.General_tips20);
                if (config != null) contentStr = config.Name;
                ToastManager.Show(contentStr);
                return;
            }
            UIHelper.ShowRewardAds(() =>
            {
                AdBuy().Forget();
            });
        }
        //5次购买
        private async UniTask AdBuy()
        {
            var req = new ReqLivesGet { Op = 1 };
            var result = await GameNetworkManager.Instance.SendAsync<RspLivesGet>(req);
            if (result.rsp != null && result.rsp.Status == 0)
            {
                DataCenter.livesData.DealLivesChange(GetAdBuyCount(),true);
                DataCenter.livesData.AdTimes -= 1;
                if (DataCenter.livesData.AdTimes < 0) DataCenter.livesData.AdTimes = 0;
                livesItemOpBtnCallback?.Invoke();
                NumsText = $"{ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.General_tips24).Name} {DataCenter.livesData.AdTimes}";
                BuyOutGo = DataCenter.livesData.AdTimes <= 0;
            }
        }

        public async UniTask OnClickDiamondBuy()
        {
            if (DataCenter.livesData.DiamondTimes <= 0)
            {
                string contentStr = "";
                var config = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.General_tips20);
                if (config != null) contentStr = config.Name;
                ToastManager.Show(contentStr);
                return;
            }
            if (!DataCenter.itemsData.CheckItemIsEnough(2, CostPrice))
            {
                List<Reward> rewardList = new List<Reward>(1);
                Reward rewardItem = new Reward(RewardType.Item, 2,CostPrice);
                rewardList.Add(rewardItem);
                ToastManager.ShowLanguage(GlobalLanguageId.Shop_tips12,UIHelper.GetRewardName(rewardItem));
                return;
            }
            var req = new ReqLivesGet{Op = 2};
            var result = await GameNetworkManager.Instance.SendAsync<RspLivesGet>(req);
            if (result.rsp != null && result.rsp.Status == 0)
            {
                DataCenter.livesData.DealLivesChange(GetDiamondBuyCount(),true);
                DataCenter.itemsData.ChangeItemsCount(2, CostPrice, false);
                DataCenter.livesData.DiamondTimes -= 1;
                if (DataCenter.livesData.DiamondTimes < 0) DataCenter.livesData.DiamondTimes = 0;
                var totalTimes = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.hp_dimond_times).Content[0];
                var priceCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.diamond_hp_price).Reward;
                int index = totalTimes - DataCenter.livesData.DiamondTimes;
                if (index >= priceCfg.Count) index = priceCfg.Count - 1;
                CostPrice = priceCfg[index].Count;
                livesItemOpBtnCallback?.Invoke();
                NumsText = $"{ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.General_tips24).Name} {DataCenter.livesData.DiamondTimes}";
                BuyOutGo = DataCenter.livesData.DiamondTimes <= 0;
            }
        }
        //存储点击事件
        public async UniTask OnClickExtraBuy()
        {
            if (DataCenter.livesData.DiamondStoreTimes <= 0)
            {
                string contentStr = "";
                var config = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.General_tips20);
                if (config != null) contentStr = config.Name;
                ToastManager.Show(contentStr);
                return;
            }
            if (!Lodash.CheckRewardIsEnough(DiamondCost))
            {
                List<Reward> rewardList = new List<Reward>(1);
                rewardList.Add(DiamondCost);
                // JumpViewModel tempVm = new(rewardList);
                // UIManager.Instance.OpenDialog<JumpView>(tempVm).Forget();
                string contentStr = "";
                var config = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.Equip_15);
                if (config != null) contentStr = config.Name;
                ToastManager.Show(contentStr);
                return;
            }
            var req = new ReqLivesGet {Op = 3};
            var result = await GameNetworkManager.Instance.SendAsync<RspLivesGet>(req);
            if (result.rsp != null)
            {
                if (result.rsp.Status == 0)
                {
                    DataCenter.livesData.DealLivesChange(this.GetDiamondBuyCount(),true);
                    DataCenter.itemsData.ChangeItemsCount(2, DiamondCost.Count, false);
                    DataCenter.livesData.OnSaveCountBuy();
                    CostPrice = DiamondCost.Count;
                    livesItemOpBtnCallback?.Invoke();
                    NumsText = $"{DataCenter.livesData.DiamondStoreTimes}/{MaxSaveCount}";
                    BuyOutGo = DataCenter.livesData.DiamondStoreTimes <= 0;
                }
                else
                {
                    ToastManager.Show(UIHelper.GetNetErrorMessage(result.rsp.Status));
                }
            }
        }
        //购买5次体力
        public int GetAdBuyCount()
        {
            var cfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.ad_hp_get_count);
            return cfg.Content[0];
        }
        //购买15次体力
        public int GetDiamondBuyCount()
        {
            var cfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.diamond_hp_count);
            return cfg.Content[0];
        }
        protected override void OnDispose()
        {
            PlayerInfoManager.Instance.PropertyChanged -= PropChange;
            base.OnDispose();
        }
    }
}
