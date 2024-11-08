using System.Collections.Specialized;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class ShopDailyViewModel : ViewModelBase
    {
        [AutoNotify] private ObservableList<ShopDailyItemViewModel> scrollViewDailyList = new();
        [AutoNotify] private string endTImeValueStr;
        private float IntervalTime = 0;
        [AutoNotify] private string titleName;
        [Preserve]
        public ShopDailyViewModel()
        {
            TitleName = ShopManager.Instance.GetTitleName(2);
            RefreshTime();
            RefreshDailyShop();
            DataCenter.shopData.DailyShop.CollectionChanged += DailyShopChanged;
        }
        
        protected override void OnDispose()
        {
            base.OnDispose();
            UIHelper.ViewModelBaseOnDisposes(scrollViewDailyList);
            DataCenter.shopData.DailyShop.CollectionChanged -= DailyShopChanged;
        }

        private void DailyShopChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshDailyShop();
        }

        private void RefreshDailyShop()
        {
            scrollViewDailyList.ClearAndDispose();
            var dailyList = ShopManager.Instance.GetDailyList();
            UIHelper.SortList(dailyList,(itemA,itemB)=> itemA.Sequ>itemB.Sequ);
            for (int i = 0; i < dailyList.Count; i++)
            {
                if( DataCenter.shopData.GetDailyData(dailyList[i].Id)!=null)
                    scrollViewDailyList.Add(new ShopDailyItemViewModel(dailyList[i]));
            }
        }

        private void RefreshTime()
        {
            EndTImeValueStr = UIHelper.GetRefreshDayTime(DataCenter.shopData.RefreshStamp);//ServerTime.Instance.Seconds2Hhmmss(ServerTime.Instance.RemainTime(DataCenter.shopData.RefreshStamp));
        }

        public override void Update()
        {
            if(!UIHelper.CalculateTime(ref IntervalTime)) return;
            RefreshTime();
        }
    }
}