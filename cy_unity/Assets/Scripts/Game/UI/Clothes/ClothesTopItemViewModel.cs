using System;
using System.Collections.Specialized;
using System.ComponentModel;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class ClothesTopItemViewModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        [AutoNotify] private int part;
		[AutoNotify] private string iconPath;
		[AutoNotify] private CellItemBaseViewModel cellItemBaseViewVm;
        [AutoNotify] private bool isUpTips;

        private Action<int> ClickItemAction; 
        
        public RectTransform RectNode; //提示框位置

        public Action RedRefreshAction;
        [Preserve]
        public ClothesTopItemViewModel(int part,Action<int> clickItemAction)
        {
            Part = part;
            ClickItemAction = clickItemAction;
            IconPath = ClothesManager.Instance.GetPartEquipBgIconByPartId(part);
            var heroEquipData = DataCenter.clothesData.GetHeroEquipDataByPart(part);
            CellItemBaseViewVm = CellItemBaseViewModel.Create(heroEquipData);
            if (CellItemBaseViewVm != null)
            {
                CellItemBaseViewVm.OnClickEvent = (info) =>
                {
                    ClickItemAction?.Invoke(part);
                };
                
                CellItemBaseViewVm.DataChanged = RefreshUp;
            }


            RefreshUp();
            DataCenter.clothesData.Wear.CollectionChanged += WearChanged;
            DataCenter.itemsData.OnItemUpdate += ItemUpdate;
            DataCenter.clothesData.PropertyChanged += DataPropertyChanged;
        }
        

        protected override void OnDispose()
        {
            base.OnDispose();
            DataCenter.clothesData.Wear.CollectionChanged -= WearChanged;
            DataCenter.itemsData.OnItemUpdate -= ItemUpdate;
            DataCenter.clothesData.PropertyChanged -= DataPropertyChanged;
        }

        private void DataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ClothesData.Items))
            {
                RefreshUp();
            }
        }

        private void HeroEquipDataChange(object sender, PropertyChangedEventArgs e)
        {
            RefreshUp();
        }

        private void ItemUpdate(ResourceData data)
        {
            RefreshUp();
        }

        private void RefreshUp()
        {
            IsUpTips = ClothesManager.Instance.CheckIsCanUpLevel(DataCenter.clothesData.GetHeroEquipDataByPart(Part));
            RedRefreshAction?.Invoke();
        }

        private void WearChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Refresh();
            RefreshUp();
        }

        private void Refresh()
        {
            var heroEquipData = DataCenter.clothesData.GetHeroEquipDataByPart(Part);
            CellItemBaseViewVm = CellItemBaseViewModel.Create(heroEquipData);
            if (CellItemBaseViewVm != null)
            {
                CellItemBaseViewVm.OnClickEvent = (info) =>
                {
                    ClickItemAction?.Invoke(part);
                };
            }
          
            RaisePropertyChanged(nameof(CellItemBaseViewVm));
        }
    }
}