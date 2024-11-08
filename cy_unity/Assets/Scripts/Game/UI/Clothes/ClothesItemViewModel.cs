using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using DH.Config;
using DH.Data;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class ClothesItemViewModel : ViewModelBase
    {
	    [AutoNotify] private bool isHeroEquip;
		[AutoNotify] private string titleNameStr;
		[AutoNotify] private Vector2 bgSize;
		public Func<object, object> GetGridCellCallback => GetGridCellCallbackByIndex;
		[AutoNotify] private ObservableDictionary<int,CellItemBaseViewModel> gridDictionary = new();
		public List<RectTransform> RectList;
		public Action RedRefreshAction;
        [Preserve]
        public ClothesItemViewModel(List<Reward> showList,string titleName=null)
        {
	        IsHeroEquip = false;
	        TitleNameStr = titleName ?? string.Empty;
	        BgSize = TitleNameStr != string.Empty ? new Vector2(1000, 242) : new Vector2(1000, 166);
	        if (showList != null)
	        {
		        for (int i = 0; i < showList.Count; i++)
		        {
			        var cellItem = CellItemBaseViewModel.Create(showList[i],ECellItemSizeType.Size150X134);
			        cellItem.IsShowOwnNum = true;
			        cellItem.IsOpenMask = true;
			        GridDictionary.Add(i,cellItem);
		        } 
	        }

	        DataCenter.clothesData.Wear.CollectionChanged += WearChanged;
	        DataCenter.clothesData.ClothesNewList.CollectionChanged += WearChanged;
        }
        
        [Preserve]
        public ClothesItemViewModel(List<HeroEquipData> showList,Action<HeroEquipData,RectTransform> clickItemAction,string titleName=null)
        {
	        IsHeroEquip = true;
	        TitleNameStr = titleName ?? string.Empty;
	        BgSize = TitleNameStr != string.Empty ? new Vector2(1000, 242) : new Vector2(1000, 166);
	        if (showList != null)
	        {
		        for (int i = 0; i < showList.Count; i++)
		        {
			        var pos = i;
			        var cellItem = CellItemBaseViewModel.Create(showList[i],ECellItemSizeType.Size150X134);
			        cellItem.OnClickEvent = (info) =>
			        {
				        clickItemAction?.Invoke(cellItem.HeroEquipData,RectList?[pos]);
			        };
			        cellItem.IsRedDot = ClothesManager.Instance.CheckUseRed(showList[i].Uid,out var redType);
			        cellItem.RedType = redType;
			        cellItem.IsOpenMask = true;
			        GridDictionary.Add(i,cellItem);
		        } 
	        }
	        DataCenter.clothesData.Wear.CollectionChanged += WearChanged;
	        DataCenter.clothesData.ClothesNewList.CollectionChanged += WearChanged;
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        DataCenter.clothesData.Wear.CollectionChanged -= WearChanged;
	        DataCenter.clothesData.ClothesNewList.CollectionChanged -= WearChanged;
	        foreach (var item in GridDictionary)
	        {
		        item.Value.Dispose();
	        }
        }

        private void WearChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
	        RefreshUseRed();
        }

        private void RefreshUseRed()
        {
	        if (IsHeroEquip)
	        {
		        foreach (var item in gridDictionary)
		        {
			        if (item.Value != null && item.Value.HeroEquipData != null)
			        {
				        item.Value.IsRedDot = ClothesManager.Instance.CheckUseRed(item.Value.HeroEquipData.Uid,out var redType);
				        item.Value.RedType = redType;

			        }
		        }
	        }
	        RedRefreshAction?.Invoke();
        }


        private object GetGridCellCallbackByIndex(object index)
		{
			if (gridDictionary.TryGetValue((int)index, out CellItemBaseViewModel ret))
			{
				return ret;
			}
			return null;
		}
        
    }
}