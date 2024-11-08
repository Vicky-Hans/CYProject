using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using Game.UI.MainUi;

namespace DH.Game.ViewModels
{
    public partial class NewbieViewModel : ViewModelBase
    {
        [AutoNotify] private ObservableList<NewBieRewardCellItemViewModel> rewardsList = new();
        [AutoNotify] private string heroNameStr;
        public BtnPriceNodeModel PriceNodeModel;
        
        private readonly FirstFlushCfg cfg;
        private float delayTime;
        
        [AutoNotify]private bool getAwardGo;
        public NewBieData Data => DataCenter.newBieData; 
        
        [Preserve]
        public NewbieViewModel()
        {
            cfg = ConfigCenter.FirstFlushCfgColl.GetDataById(1);
            InitUI();
            UpdateRewardsList();
            DataCenter.newBieData.PropertyChanged += ProperChange;
            PlayerInfoManager.Instance.PropertyChanged += ProperChange;
            ShowHighAward();
            var heroId = 2000;
            var nameCfg = ConfigCenter.HeroMainLanguageCfgColl.GetDataById(heroId);
            if (nameCfg != null)
            {
                HeroNameStr = nameCfg.Name;
            }
        }
        
        private void UpdateRewardsList()
        {
            rewardsList.Clear();
            NewBieRewardCellItemViewModel cellItem1 = new(cfg.Reward, 1);
            RewardsList.Add(cellItem1);
            NewBieRewardCellItemViewModel cellItem2 = new(cfg.Reward1, 2);
            RewardsList.Add(cellItem2);
            NewBieRewardCellItemViewModel cellItem3 = new(cfg.Reward2, 3);
            RewardsList.Add(cellItem3);
        }

        public void ProperChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(Data.IsBuy) or nameof(PlayerInfoManager.Instance.SecondDay) or nameof(Data.Claim))
            {
                 InitUI();
            }
        }

        void InitUI()
        {
            var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(cfg.PackageId);
            var cfgInfo = ConfigCenter.RechargeCfgColl.GetDataById(packageCfg.RechargeId);
            string priceStr = "";
            if (cfgInfo != null) priceStr = PayController.Instance.GetLocalizePrice(cfgInfo.PayId);
            PriceNodeModel = new BtnPriceNodeModel(priceStr);
            GetAwardGo =Data.IsBuy && Data.IsGetAward(GetNowDay());
        }

        [Command]
        private async void OnClickBuyButton()
        {
            if (Data.IsBuy)
            {
                if (Data.IsGetAward(GetNowDay()))return;
                
                var result = await GameNetworkManager.Instance.SendAsync<RspFirstChargeClaim>(new ReqFirstChargeClaim());
                if (result.rsp.Status == 0)
                {
                    Lodash.DealRewards(result.rsp.Rewards.ToList());
                    Data.Claim = GetNowDay();
                    UIHelper.OpenCommonRewardView(result.rsp.Rewards.ToList());

                    if (Data.Isover())
                    {
                        MainUiManager.Instance.RemoveRightBut(MainStageInfoNodeRightButType.Newbie);
                        UIManager.Instance.CloseDialog<NewbieView>();
                    }
                }
                else
                {
                    ToastManager.Show(UIHelper.GetNetErrorMessage(result.rsp.Status));
                }
            }
            else
            {       
                // PayController.Instance.Pay(cfg.PackageId,-1,(result) =>
                // {
                //     if (result == null || result.Status != 0)
                //     {
                //         DHLog.Debug($"新手礼包购买失败 {result.Status}");
                //         return;
                //     }
                //     Data.BuyGift();
                //     Lodash.DealRewards(result.Rewards.ToList());
                //     UIHelper.OpenCommonRewardView(result.Rewards.ToList());
                // });
                
                ShopManager.Instance.SendBuyPackageBuyRecharge(cfg.PackageId, (packageId) =>
                {
                    Data.BuyGift();
                });
            }
            
        }
        
        void ShowHighAward()
        {
            List<Reward> temp = new ();
            temp.AddRange(cfg.Reward);
            temp.AddRange(cfg.Reward1);
            temp.AddRange(cfg.Reward2);
        }
        
        
        protected override void OnDispose()
        {
            DataCenter.newBieData.PropertyChanged -= ProperChange;
            PlayerInfoManager.Instance.PropertyChanged -= ProperChange;
            base.OnDispose();
        }

        int GetNowDay()
        {
            return ServerTime.Instance.GetTimeDay(Data.BuyStamp)+1;
        }
    }
}