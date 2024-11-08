using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class ClothesMergeSucceedViewModel : ViewModelBase
    {
	    [AutoNotify] private CellItemBaseViewModel cellItemBaseViewVm;
		[AutoNotify] private bool isShowMergeAnimation;
		[AutoNotify] private bool isShowMergeSucceed;
		[AutoNotify] private string itemNameStr;
		[AutoNotify] private string startLevelStr;
		[AutoNotify] private string nextLevelStr;
		[AutoNotify] private string attrName;
		[AutoNotify] private string startAtkStr;
		[AutoNotify] private string nextAtkStr;
		[AutoNotify] private string skillDescStr;

		public Action ResultAction;
		public Func<object, object> GetGridCellCallback => GetGridCellCallbackByIndex;
		[AutoNotify] private ObservableDictionary<int,CellItemBaseViewModel> gridDictionary = new();
		[AutoNotify] private int showNum;

		private List<HeroEquip> ShowReward;
		
		private ClickTextComponent clickTextCmp;

		public ClickTextComponent ClickTextCmp
		{
			get => null;
			set { 
				clickTextCmp = value;
				if (clickTextCmp != null)
				{
					clickTextCmp.ClickCallback = OnClickLinkCallback;
				}
			}
		}
		
        [Preserve]
        public ClothesMergeSucceedViewModel(long uid,List<Resource> mergeList,List<HeroEquip> showReward = null)
        {
	        ShowReward = showReward;
	        if (mergeList.Count == 2)
	        {
		        gridDictionary.Add(1,CellItemBaseViewModel.Create(mergeList[0]));
		        gridDictionary.Add(4,CellItemBaseViewModel.Create(mergeList[1]));
	        }else if (mergeList.Count == 3)
	        {
		        gridDictionary.Add(0,CellItemBaseViewModel.Create(mergeList[0]));
		        gridDictionary.Add(2,CellItemBaseViewModel.Create(mergeList[1]));
		        gridDictionary.Add(3,CellItemBaseViewModel.Create(mergeList[2]));
	        }
	        else
	        {
		        for (int i = 0; i < mergeList.Count; i++)
		        {
			        gridDictionary.Add(i,CellItemBaseViewModel.Create(mergeList[i]));
		        }

	        }
	        
	        ShowNum = mergeList.Count;
	        IsShowMergeSucceed = true;
	        
	        ResultAction = RefreshCloseSucceedUI;
	        var data = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
	        if(data==null || data.IsNull()) return;
	        CellItemBaseViewVm = CellItemBaseViewModel.Create(uid,ECellItemSizeType.Size216X150);
	        var skillId = ClothesManager.Instance.GetSkillIdByQuaId(data.Id,data.QuaId);
	        if (skillId != 0)
	        {
		        SkillDescStr = ClothesManager.Instance.GetClothesSkillDesc(skillId);
	        }
	        else
	        {
		        SkillDescStr = string.Empty;
	        }
	        ItemNameStr = ClothesManager.Instance.GetClothesItemName(data.Id);


	        var lastQua = ClothesManager.Instance.GetLastQuaId(data.QuaId);
	        NextLevelStr = ClothesManager.Instance.GetClothesMaxLevel(data.QuaId).ToString();
	        StartLevelStr = ClothesManager.Instance.GetClothesMaxLevel(lastQua).ToString();

	        var attrList = ClothesManager.Instance.GetClothesAttrList(data.Uid,false);
	        if (attrList.Count > 0)
	        {
		        var baseAttr = attrList.First().Value;
		        var cfg = ConfigCenter.AttributesCfgColl.GetDataByName(attrList.First().Key);
		        var mergeValue = ClothesManager.Instance.GetQuaValue(data.QuaId);
		        var lastMergeValue = ClothesManager.Instance.GetQuaValue(lastQua);
		        AttrName = $"{AttributesManager.Instance.GetAttrName(attrList.First().Key)}:";
		        StartAtkStr = $"{UIHelper.ParseValueToStringByType(baseAttr * lastMergeValue,GameConst.ENumType.NumberTypeValue,cfg.Type,true)}";
		        NextAtkStr = $"{UIHelper.ParseValueToStringByType(baseAttr*mergeValue,GameConst.ENumType.NumberTypeValue,cfg.Type,true)}";
	        }
	        else
	        {
		        StartAtkStr = "0";
		        NextAtkStr = "0";
	        }
	        
        }

        public void RefreshCloseSucceedUI()
        {
	        // EquipManager.Instance.RefreshCloseSucceedUI();
	        if (ShowReward != null)
	        {
		        UIManager.Instance.CloseDialog<ClothesMergeSucceedView>();
		        UIHelper.OpenCommonRewardView(ShowReward,mergeSucceed:true);
	        }
        }
        
        private object GetGridCellCallbackByIndex(object index)
        {
	        if (gridDictionary.TryGetValue((int)index, out CellItemBaseViewModel ret))
	        {
		        return ret;
	        }
	        return null;
        }

        [Command]
        private void OnClickClose()
        {
	        UIManager.Instance.CloseDialog<ClothesMergeSucceedView>();
        }
        
        private void OnClickLinkCallback(string info, Vector3 arg2)
        {
	        UIHelper.OnClickDescLink(info,arg2);
        }
    }
}