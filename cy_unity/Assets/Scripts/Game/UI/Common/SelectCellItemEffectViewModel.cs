using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Generic;
using DH.Config;
using DH.Data;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using UnityEngine;
using UnityEngine.Scripting;


namespace DH.Game.ViewModels
{
	public enum SelectItemType
	{
		Base,
		Select,
	}

    public partial class SelectCellItemEffectViewModel : ViewModelBase
    {
        
	    [AutoNotify] private CellItemViewModel cellItemBaseViewVm;
	    private int index;
	    [AutoNotify] private bool isSelect;
	    [AutoNotify] private SelectItemType selectType;
	    
	    public List<Reward> SelectRewardList = new();
	    public Action ClickEvent;
	    
	    #region 非选择item

	    public static SelectCellItemEffectViewModel Create(Resource reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,SelectItemType selectType= SelectItemType.Base)
	    {
		    return new  SelectCellItemEffectViewModel(reward.Id,reward.Type,reward.Count,sizeType,showLimit,isShowNum,selectType);
	    }
        
	    public static SelectCellItemEffectViewModel Create(ResourceData reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,SelectItemType selectType= SelectItemType.Base)
	    {
		    return new  SelectCellItemEffectViewModel(reward.Id,reward.Type,reward.Count,sizeType,showLimit,isShowNum,selectType);
	    }

	    public static SelectCellItemEffectViewModel Create(Reward reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,SelectItemType selectType= SelectItemType.Base)
	    {
		    return new  SelectCellItemEffectViewModel(reward.Id,(int)reward.Type,reward.Count,sizeType,showLimit,isShowNum,selectType);
	    }

	    #endregion
	    
	    #region 选择item

	    public static SelectCellItemEffectViewModel Create(List<Resource> reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,SelectItemType selectType= SelectItemType.Select,int selectIndex = -1)
	    {
		    var temp =new List<Reward>();
		    for (int i = 0; i < reward.Count; i++)
		    {
			    temp.Add(new Reward((RewardType)reward[i].Id,reward[i].Type,reward[i].Count));
		    }
		    return new  SelectCellItemEffectViewModel(temp,selectIndex,sizeType,showLimit,isShowNum,selectType);
	    }
        
	    public static SelectCellItemEffectViewModel Create(List<ResourceData> reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,SelectItemType selectType= SelectItemType.Select,int selectIndex = -1)
	    {

		    var temp =new List<Reward>();
		    for (int i = 0; i < reward.Count; i++)
		    {
			    temp.Add(new Reward((RewardType)reward[i].Id,reward[i].Type,reward[i].Count));
		    }
		    
		    return new  SelectCellItemEffectViewModel(temp,selectIndex,sizeType,showLimit,isShowNum,selectType);
	    }

	    public static SelectCellItemEffectViewModel Create(List<Reward> reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,SelectItemType selectType= SelectItemType.Select,int selectIndex = -1)
	    {
		    return new  SelectCellItemEffectViewModel(reward,selectIndex,sizeType,showLimit,isShowNum,selectType);
	    }

	    #endregion
	    
	    
        [Preserve]
        public SelectCellItemEffectViewModel(int id,int type,long count,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,SelectItemType selectType= SelectItemType.Base)
        {
	        IsSelect = false;
	        SelectType = selectType;
	        CellItemBaseViewVm = new CellItemViewModel(id,type,count,sizeType,showLimit,isShowNum);
        }
        
        [Preserve]
        public SelectCellItemEffectViewModel(List<Reward> rewardList,int selectIndex = -1,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,SelectItemType selectType= SelectItemType.Select)
        {
	        SelectRewardList.Clear();
	        if(rewardList!=null)
		        SelectRewardList.AddRange(rewardList);
	        SelectType = selectType;
	        IsSelect = SelectType == SelectItemType.Select && selectIndex == -1;
	        var reward = GetPackageSelectReward(selectIndex);
	        CellItemBaseViewVm = new CellItemViewModel(reward.Id,(int)reward.Type,reward.Count,sizeType,showLimit,isShowNum);
        }
        
        public Reward GetPackageSelectReward(int index)
        {
	        if(SelectRewardList==null || SelectRewardList.Count==0) return null;
	        if (index == -1) return SelectRewardList[0];
	        if (SelectRewardList.Count <= index) return SelectRewardList[0];
	        return SelectRewardList[index];
        }
        
        public void MergeSelect(int index)
        {
	        IsSelect = SelectType == SelectItemType.Select && index == -1;
	        CellItemBaseViewVm.Merge(GetPackageSelectReward(index));
        }



        [Command]
        private void OnClickSelectItem()
        {
	        //打开物品选择界面
	        ClickEvent?.Invoke();
        }
    }
}