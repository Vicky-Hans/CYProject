using System.Collections.Generic;
using System.Collections.Specialized;
using DH.Data;
using DH.Game.UI;
using DH.Config;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using ObservableObject = DH.UIFramework.Observables.ObservableObject;
namespace DH.Game.ViewModels
{
    public partial class CollegeDiscountsModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        public ObservableList<ObservableObject> CollegeDiscountsItems = new ObservableList<ObservableObject>();
        [AutoNotify] public BtnPriceNodeModel priceNodeModel;
        public CommonTopViewModel TopItemsModel;
        
        public PackageCfg packageCfg;
        public CollegeData Data => DataCenter.collegeData;
        public CollegeActivityManager Manager => CollegeActivityManager.Instance;
        public bool IsBuyState => GetBuyState();
        
        [AutoNotify]public bool isMoveScroll;

        [Preserve]
        public CollegeDiscountsModel()
        {
            InitLevelList();
            InitTop();
            InitPriceShow();
            
            DataCenter.collegeData.FundClaimed.CollectionChanged += ClaimChanged;
            DataCenter.collegeData.FundPlusClaimed.CollectionChanged += ClaimChanged;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            DataCenter.collegeData.FundClaimed.CollectionChanged -= ClaimChanged;
            DataCenter.collegeData.FundPlusClaimed.CollectionChanged -= ClaimChanged;
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
            return DataCenter.collegeData.GetPlusIsBuy();
        }

        //初始化购买奖励 需要添加package 配置
        private void InitPriceShow()
        {
            packageCfg = ConfigCenter.PackageCfgColl.GetDataById((int)EPackageID.College);
            if(packageCfg!=null)
                PriceNodeModel = new BtnPriceNodeModel(packageCfg.Id);
        }

        private void InitTop()
        {
            TopItemsModel = new CommonTopViewModel(new List<GameConst.ItemIdCode>
            {
                GameConst.ItemIdCode.EnergyDrink,
                GameConst.ItemIdCode.Stone,
                GameConst.ItemIdCode.Money,
            });
        }

        private void InitLevelList()
        {
            CollegeDiscountsItems.Clear();
            var levelItems = ActivityUIManager.Instance.GetActivityFund(ActivityFund.College);
            foreach (var cfg in levelItems)
            {
                var model = new CollegeDiscountItemModel(cfg);
                CollegeDiscountsItems.Add(model);
            }
        }
        
        //点击购买VIP
        [Command]
        public void BuyLevel()
        {
            if(CollegeActivityManager.Instance.CheckEndTime()) return;
            ShopManager.Instance.SendBuyPackageBuyRecharge(packageCfg.Id, (id) =>
            {
                ToastManager.ShowLanguage(GlobalLanguageId.ConfRecharge0);
                DataCenter.collegeData.AddBuyState();
                RaisePropertyChanged(nameof(IsBuyState));
            });
        }
        

    }
}