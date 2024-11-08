using System;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System.Collections.Generic;
using System.Collections.Specialized;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class LuckEggFundViewModel : ViewModelBase
    {
        public ObservableList<LuckEggFundItemViewModel> CollegeDiscountsItems = new ObservableList<LuckEggFundItemViewModel>();
        [AutoNotify] public BtnPriceNodeModel priceNodeModel;
        public CommonTopViewModel TopItemsModel;
        [AutoNotify] private string timeDes;
        public PackageCfg packageCfg;
        public LuckyEggData Data => DataCenter.luckyEggData;
        public bool IsBuyState => GetBuyState();
        
        [AutoNotify]public bool isMoveScroll;

        [Preserve]
        public LuckEggFundViewModel()
        {
            InitLevelList();
            InitTop();
            InitPriceShow();
            RefreshTimeDesc();
            DataCenter.luckyEggData.FundClaimed.CollectionChanged += ClaimChanged;
            DataCenter.luckyEggData.FundPlusClaimed.CollectionChanged += ClaimChanged;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            foreach (var item in CollegeDiscountsItems)
            {
                item?.Dispose();
            }
            TopItemsModel?.Dispose();
            priceNodeModel?.Dispose();
            DataCenter.luckyEggData.FundClaimed.CollectionChanged -= ClaimChanged;
            DataCenter.luckyEggData.FundPlusClaimed.CollectionChanged -= ClaimChanged;
        }
        
        private void ClaimChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(IsBuyState));
            IsMoveScroll = true;
        }

        private void RefreshInfo()
        {
            InitLevelList();
            InitPriceShow();
        }

        private bool GetBuyState()
        {
            return DataCenter.luckyEggData.GetPlusIsBuy();
        }

        //初始化购买奖励 需要添加package 配置
        private void InitPriceShow()
        {
            packageCfg = ConfigCenter.PackageCfgColl.GetDataById((int)EPackageID.LuckEgg);
            if(packageCfg!=null)
                PriceNodeModel = new BtnPriceNodeModel(packageCfg.Id);
        }

        private void InitTop()
        {
            TopItemsModel = UIHelper.GetTopModel(GameConst.ItemIdCode.EggCoin,GameConst.ItemIdCode.EggRedHeart,GameConst.ItemIdCode.Stone);
            TopItemsModel.ClickItemAction = (data, id) =>
            {
                if (data.Id == (int)GameConst.ItemIdCode.EggCoin && !DataCenter.luckyEggData.IsTimeOver())
                {
                    ActivityUIManager.Instance.OpenBuyEggCoin();
                }
                if (data.Id == (int)GameConst.ItemIdCode.EggRedHeart && !DataCenter.luckyEggData.IsTimeOver())
                {
                    ActivityUIManager.Instance.EggTabType = LuckEggShowView.Main;
                }
            };
        }
        private void InitLevelList()
        {
            CollegeDiscountsItems.Clear();
            var levelItems = ActivityUIManager.Instance.GetActivityFund(ActivityFund.LuckEgg);
            foreach (var cfg in levelItems)
            {
                var model = new LuckEggFundItemViewModel(cfg);
                CollegeDiscountsItems.Add(model);
            }
        }
        
        //点击购买VIP
        [Command]
        public void BuyLevel()
        {
            if(Data.IsTimeOver()) return;
            ShopManager.Instance.SendBuyPackageBuyRecharge(packageCfg.Id, (id) =>
            {
                ToastManager.ShowLanguage(GlobalLanguageId.ConfRecharge0);
                DataCenter.luckyEggData.AddBuyState();
                RaisePropertyChanged(nameof(IsBuyState));
            });
        }
        
        
        private float interval;
        private void RefreshTimeDesc()
        {
            var times = Math.Max(0,
                DataCenter.luckyEggData.EndStamp - ServerTime.Instance.GetNowTime());
           TimeDes =  ServerTime.Instance.SecondsDHAndMS(times);
        }
        public override void Update()
        {
            if (UIHelper.CalculateTime(ref interval))
            {
                RefreshTimeDesc();
            }
        }
        
    }
}