using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using DH.Config;
using DH.Data;
using DH.UIFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
	public partial class ClothesMergeTopItemViewModel : ViewModelBase
    {
	    public override bool AutoDispose => true;
	    [AutoNotify] private int showSketchType;
	    [AutoNotify] private HeroEquipData baseData;
	    [AutoNotify] private Reward replaceItem;
		[AutoNotify] private ClothesMergeItemState showState;
		[AutoNotify] private CellItemBaseViewModel cellItemBaseViewVm;
		[AutoNotify] private CellItemBaseViewModel cellItemViewShowVm;
		[AutoNotify] private bool isShowAdd;

		[AutoNotify] private string partPath;
		[AutoNotify] private string qualityPath;
		[AutoNotify] private string baseBgPath;
		[AutoNotify] private string baseIconPath;
		[AutoNotify] private long uid = 0;

		public Action<long,ClothesMergeItemState> ClickBackAction;

		[AutoNotify] private bool isExit;//数据是否存在
		[AutoNotify] private bool isShowExit;//表现是否需要显示
		[AutoNotify] private RectTransform noneTransform;
		[AutoNotify] private bool isFirst;
        [Preserve]
        public ClothesMergeTopItemViewModel(HeroEquipData data,bool isShowAdd=true,Action<long,ClothesMergeItemState> backClick=null,bool isFirst=false,int showSketchType = 2)
        {
	        ClickBackAction = backClick;
	        IsShowAdd = isShowAdd;
	        Merge(data);
	        IsFirst = isFirst;
	        if (showSketchType == 1)
	        {
		        data.QuaId = ClothesManager.Instance.GetQuaUpStateStart(data.QuaId);
		        CellItemViewShowVm = CellItemBaseViewModel.Create(data);
	        }
	        else
	        {
		        CellItemViewShowVm = CellItemBaseViewModel.Create(data);
	        }

	        ShowSketchType = showSketchType;
        }

        public void Merge(HeroEquipData data=null)
        {
	        BaseData = data;
	        Uid = BaseData?.Uid ?? 0;
	        Refresh();
        }
        
        public void Merge(Reward data,long uid)
        {
	        ReplaceItem = data;
	        Uid = uid;
	        RefreshItem();
        }

        private void Refresh()
        {
	        if (BaseData == null)
	        {
		        ShowState = ClothesMergeItemState.None;
	        }else
	        {
		        ShowState = BaseData.Uid == 0 ?ClothesMergeItemState.Sketch : ClothesMergeItemState.HeroEquip;
	        }

	        PartPath = ClothesManager.Instance.GetPartEquipMinIcon(BaseData?.Id ?? 0,BaseData?.QuaId ?? 0);
	        QualityPath = ClothesManager.Instance.GetRareIcon(BaseData?.Id ?? 0);
	        
	        BaseBgPath = ClothesManager.Instance.GetClothesQuaBgPath(BaseData?.Id ?? 0,BaseData);
	        BaseIconPath = ClothesManager.Instance.GetPartEquipBgIcon(BaseData?.Id ?? 0);
	        
	        CellItemBaseViewVm?.Dispose();
	        CellItemBaseViewVm = CellItemBaseViewModel.Create(BaseData);
	        if (CellItemBaseViewVm != null)
	        {
		        CellItemBaseViewVm.OnClickEvent = (info) =>
		        {
			        ClickBack();
		        };
	        }

        }

        private void RefreshItem()
        {
	        ShowState = ReplaceItem == null ? ClothesMergeItemState.None : ClothesMergeItemState.Reward;
		    CellItemBaseViewVm = CellItemBaseViewModel.Create(ReplaceItem);
		    if (CellItemBaseViewVm != null)
		    {
			    CellItemBaseViewVm.OnClickEvent = (info) =>
			    {
				    ClickBack();
			    };
		    }
        }

        public void ClickBack()
        {
	        IsExit = false;
	        IsShowExit = false;
	        if (IsFirst)
	        {
		        Merge();
	        }
	        else
	        {
		        var heroData = new HeroEquipData
		        {
			        Uid = 0,
			        Id = BaseData.Id,
			        QuaId = BaseData.QuaId,
			        Lv = 0,
		        };
		        Merge(heroData);
	        }

	        ClickBackAction?.Invoke(Uid, showState);
        }
    }
}