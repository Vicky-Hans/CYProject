using System;
using System.Collections.Generic;
using DH.Config;
using DH.Data;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class SelectCellItemViewModel : ViewModelBase
    {
	    [AutoNotify] private CellItemBaseViewModel cellItemBaseViewVm;
	    private int index;
	    [AutoNotify] private bool isSelect;
	    [AutoNotify] private bool isSelectType;
	    [AutoNotify] private Vector2 sizeIcon;
	    [AutoNotify] private Vector2 sizeBg;
	    [AutoNotify] private Vector2 selectSize;
	    
	    [AutoNotify] private bool isButEnabled = true;

	    public List<Reward> SelectRewardList = new();
	    public Action ClickEvent;
	    [AutoNotify] private SelectItemType selectType;
	    #region 非选择item

	    public static SelectCellItemViewModel Create(Resource reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,bool isSelect = false)
	    {
		    return new  SelectCellItemViewModel(reward.Id,reward.Type,reward.Count,sizeType,showLimit,isShowNum,isSelect);
	    }
        
	    public static SelectCellItemViewModel Create(ResourceData reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,bool isSelect = false)
	    {
		    return new  SelectCellItemViewModel(reward.Id,reward.Type,reward.Count,sizeType,showLimit,isShowNum,isSelect);
	    }

	    public static SelectCellItemViewModel Create(Reward reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,bool isSelect = false)
	    {
		    return new  SelectCellItemViewModel(reward.Id,(int)reward.Type,reward.Count,sizeType,showLimit,isShowNum,isSelect);
	    }

	    #endregion
	    
	    #region 选择item

	    public static SelectCellItemViewModel Create(List<Resource> reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,bool isSelect = true,int selectIndex = -1)
	    {
		    var temp =new List<Reward>();
		    for (int i = 0; i < reward.Count; i++)
		    {
			    temp.Add(new Reward((RewardType)reward[i].Id,reward[i].Type,reward[i].Count));
		    }
		    return new  SelectCellItemViewModel(temp,selectIndex,sizeType,showLimit,isShowNum,isSelect);
	    }
        
	    public static SelectCellItemViewModel Create(List<ResourceData> reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,bool isSelect = true,int selectIndex = -1)
	    {

		    var temp =new List<Reward>();
		    for (int i = 0; i < reward.Count; i++)
		    {
			    temp.Add(new Reward((RewardType)reward[i].Id,reward[i].Type,reward[i].Count));
		    }
		    
		    return new  SelectCellItemViewModel(temp,selectIndex,sizeType,showLimit,isShowNum,isSelect);
	    }

	    public static SelectCellItemViewModel Create(List<Reward> reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,bool isSelect = true,int selectIndex = -1)
	    {
		    return new  SelectCellItemViewModel(reward,selectIndex,sizeType,showLimit,isShowNum,isSelect);
	    }

	    #endregion
	    
	    
        [Preserve]
        public SelectCellItemViewModel(int id,int type,long count,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,bool isSelect = false)
        {
	        IsSelect = isSelect;
	        IsSelectType = isSelect;
	        SelectType = isSelect ? SelectItemType.Select : SelectItemType.Base;
	        CellItemBaseViewVm = new CellItemBaseViewModel(id,type,count,sizeType,showLimit,isShowNum);
        }
        
        [Preserve]
        public SelectCellItemViewModel(List<Reward> rewardList,int selectIndex = -1,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,bool isSelect = true)
        {
	        SelectRewardList.Clear();
	        if(rewardList!=null)
		        SelectRewardList.AddRange(rewardList);
	        IsSelectType = isSelect;
	        IsSelect = false;
	        if (isSelect && selectIndex == -1)
	        {
		        IsSelect = true;
	        }
	        SelectType = isSelect ? SelectItemType.Select : SelectItemType.Base;
	        var reward = GetPackageSelectReward(selectIndex);
	        CellItemBaseViewVm = new CellItemBaseViewModel(reward.Id,(int)reward.Type,reward.Count,sizeType,showLimit,isShowNum);
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
	        IsSelect = false;
	        if (IsSelectType && index == -1)
	        {
		        IsSelect = true;
	        }
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