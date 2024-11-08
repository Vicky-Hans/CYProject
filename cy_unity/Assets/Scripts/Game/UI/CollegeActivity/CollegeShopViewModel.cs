using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using DH.Config;
using DH.Data;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class CollegeShopViewModel : ViewModelBase,IComparer
    {
        public override bool AutoDispose => true;
        [AutoNotify] private CommonTopViewModel commonTopItemsVm;
		[AutoNotify] private ObservableList<CollegeShopItemViewModel> scrollViewList = new();

        private ICollectionView ollectionView;

        public ICollectionView CollectionView
        {
            get => null;
            set
            {
                ollectionView = value;
                if (ollectionView == null) return;
                ollectionView.Comparer = this;
                ollectionView?.Refresh();
            }
        }

        [Preserve]
        public CollegeShopViewModel()
        {
            InitUI();
            InitTopItems();
            DataCenter.collegeData.PackageRecord.CollectionChanged += PackageChanged;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            DataCenter.collegeData.PackageRecord.CollectionChanged -= PackageChanged;
        }

        private void PackageChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            
        }


        private void InitTopItems()
        {
            CommonTopItemsVm = new CommonTopViewModel(new List<GameConst.ItemIdCode>
            {
                GameConst.ItemIdCode.Stone,
                GameConst.ItemIdCode.Money,
            });
        }
        
        void InitUI()
        {
            var list = ConfigCenter.PackageCfgColl.GetDataByRule((int)EPackageType.CollegeShop);
            for (int i = 0; i < list.Count; i++)
            {
                ScrollViewList.Add(new CollegeShopItemViewModel(list[i].Id,() =>
                {
                    ollectionView?.Refresh();
                }));
            }
        }

        public int Compare(object x, object y)
        {
            if (!(x is CollegeShopItemViewModel itemA) || !(y is CollegeShopItemViewModel itemB)) return 0;
            return SortValue(itemB) - SortValue(itemA);
        }

        public int SortValue(CollegeShopItemViewModel item)
        {
            int sortValue=0;
            if (item.State != ShopBuyState.Finish)
            {
                sortValue += 10000;
            }

            if (item.PackageCfg.Num != 0)
            {
                sortValue += 1000 - item.PackageCfg.Num;
            }
            else
            {
                sortValue += 1000 - item.PackageId; 
            }

           
            return sortValue;
        }
    }
}