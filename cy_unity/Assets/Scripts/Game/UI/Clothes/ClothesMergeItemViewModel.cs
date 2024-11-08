using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using DH.Config;
using DH.Data;
using DH.UIFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
	public enum ClothesMergeItemState
	{
		None,
		Sketch,
		Reward,
		HeroEquip,
	}

	public partial class ClothesMergeItemViewModel : ViewModelBase
    {
	    public override bool AutoDispose => true;
	    [AutoNotify] private CellItemBaseViewModel cellItemBaseViewVm;
	    [AutoNotify] private long uid;
		[AutoNotify] private bool useIngState;
		[AutoNotify] private bool selectState;
		[AutoNotify] private bool lockState;
		[AutoNotify] private ClothesMergeItemState state;
		
		public RectTransform rect; //提示框位置
		public Vector3 ClickPosition;
        [Preserve]
        public ClothesMergeItemViewModel(long uid,Action<ClothesMergeItemViewModel> clickAction=null)
        {
	        State = ClothesMergeItemState.HeroEquip;
	        Uid = uid;
	        CellItemBaseViewVm = CellItemBaseViewModel.Create(uid,ECellItemSizeType.Size150X134);
	        CellItemBaseViewVm.OnClickEvent = (info) =>
	        {
		        clickAction?.Invoke(this);
	        };
	        CellItemBaseViewVm.IsOpenMask = true;
	        CellItemBaseViewVm.DataChanged = RefreshMergeRed;
	        RefreshMergeRed();
	        UseIngState = DataCenter.clothesData.IsUseIng(uid);
	        DataCenter.clothesData.Wear.CollectionChanged += HeroEquipWearChanged;
	        DataCenter.clothesData.PropertyChanged += ItemChanged;
	        ClothesManager.Instance.PropertyChanged += ManagerChanged;
        }

        [Preserve]
        public ClothesMergeItemViewModel(Reward reward,Action<ClothesMergeItemViewModel> clickAction=null,int pos=0)
        {
	        State = ClothesMergeItemState.Reward;
	        Uid = ClothesManager.Instance.GetRewardUid(reward.Id, pos);
	        CellItemBaseViewVm = CellItemBaseViewModel.Create(reward,ECellItemSizeType.Size150X134);
	        CellItemBaseViewVm.IsShowNum = true;
	        CellItemBaseViewVm.OnClickEvent = (info) =>
	        {
		        if(!LockState)
					clickAction?.Invoke(this);
	        };
	        DataCenter.clothesData.Wear.CollectionChanged += HeroEquipWearChanged;
	        DataCenter.clothesData.PropertyChanged += ItemChanged;
	        ClothesManager.Instance.PropertyChanged += ManagerChanged;

        }

    

        protected override void OnDispose()
        {
	        base.OnDispose();
	        DataCenter.clothesData.Wear.CollectionChanged -= HeroEquipWearChanged; 
	        DataCenter.clothesData.PropertyChanged -= ItemChanged;
	        ClothesManager.Instance.PropertyChanged -= ManagerChanged;
        }

        private void ManagerChanged(object sender, PropertyChangedEventArgs e)
        {
	        RefreshMergeRed();
        }

        private void ItemChanged(object sender, PropertyChangedEventArgs e)
        {
	        RefreshMergeRed();
        }

        private void RefreshMergeRed()
        {
	        if (state == ClothesMergeItemState.HeroEquip)
	        {
		        CellItemBaseViewVm.IsRedDot = !ClothesManager.Instance.IsMergeSelect && ClothesManager.Instance.CheckIsCanMerge(DataCenter.clothesData.GetHeroEquipDataByUid(uid));
	        }
        }

        private void HeroEquipWearChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
	        if (CellItemBaseViewVm.BaseData.HeroEquip != null)
	        {
		        UseIngState = DataCenter.clothesData.IsUseIng(CellItemBaseViewVm.BaseData.HeroEquip.Uid);
	        }
        }
        
        public RectTransform GetRectTransform()
        {
	        return rect;
        }
        
    }
}