using System;
using System.Collections.Generic;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;

namespace DH.Game.ViewModels
{
	public enum TitleShowType
	{
		/// <summary>
		/// 恭喜获得
		/// </summary>
		Base,
		/// <summary>
		/// 获得剩余奖励
		/// </summary>
		Residue,
		/// <summary>
		/// 限定奖励
		/// </summary>
		Limit,
		/// <summary>
		/// 获得超级累计大奖
		/// </summary>
		SuperReward,
	}

	public partial class ShopDrawRewardViewModel : ViewModelBase
    {
        
		 [AutoNotify] private ObservableList<CellItemViewModel> scrollViewList = new();

		 [AutoNotify] private int chestBoxId;
		 [AutoNotify] private ItemPriceNodeModel itemPriceNodeModel;
		 [AutoNotify] private ShopBuyState shopBuyState;
		 [AutoNotify] private Action closeAction;
		 [AutoNotify] private TitleShowType titleShowType;
		 [AutoNotify] private string titleName;
		 [AutoNotify] private bool isTen;
		 private List<Resource> SuperRewardIdList = null;
		 
		 private ParticleSystem mergeSucceed;

		 public ParticleSystem MergeSucceed
		 {
			 get => mergeSucceed;
			 set
			 {
				 mergeSucceed = value;
				 if (mergeSucceedState && mergeSucceed!=null)
				 {
					 UIHelper.PlayEffect(mergeSucceed);
				 }
			 }
		 }
		 public bool mergeSucceedState;
		 [Preserve]
        public ShopDrawRewardViewModel(List<Resource> rewards,int chestBoxId=0,Action closeAction=null,TitleShowType title=TitleShowType.Base,bool isTen = false,List<Resource> superRewardIdList=null)
        {
	        SuperRewardIdList = superRewardIdList;
	        IsTen = isTen;
	        TitleShowType = title;
	        RefreshTitleName();
	        ShopManager.Instance.IsShowDrawRewardAnimation = false;
	        ChestBoxId = chestBoxId;
	        InitRewardList(rewards);
	        CloseAction = closeAction;
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        ItemPriceNodeModel?.Dispose();
	        UIHelper.ViewModelBaseOnDisposes(scrollViewList);
        }

        private void RefreshTitleName()
        {
	        switch (TitleShowType)
	        {
		        case TitleShowType.Limit: TitleName = LocalizeHelper.GetGlobal(GlobalLanguageId.MakeWish_11); break;
		        case TitleShowType.Residue: TitleName = LocalizeHelper.GetGlobal(GlobalLanguageId.MakeWish_12); break;
		        case TitleShowType.SuperReward: TitleName = LocalizeHelper.GetGlobal(GlobalLanguageId.LuckyJourney_02); break;
		        default: TitleName = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips02); break;
	        }
        }

        private void InitRewardList(List<Resource> rewards)
        {
	        UIHelper.SortReward(rewards);
	        scrollViewList.ClearAndDispose();
	        if (TitleShowType.SuperReward == TitleShowType)
	        {
		        foreach (var item in SuperRewardIdList)
		        {
			        var model = CellItemViewModel.Create(item);
			        model.StartPlayAdvEffect();
			        model.CellItemBaseViewVm.IsOpenMask = true;
			        scrollViewList.Add(model);
		        }
	        }
	        
	        foreach (var item in rewards)
	        {
		        var model = CellItemViewModel.Create(item);
			    model.OpenHighRewardTips();
		        model.CellItemBaseViewVm.IsOpenMask = true;
		        scrollViewList.Add(model);
	        }

	        if (ChestBoxId != 0)
	        {
		        ShopBuyState = ShopBuyState.Item;//ShopManager.Instance.GetEquipChestState(ChestBoxId);
		        var reward = ShopManager.Instance.GetEquipChestItem(ChestBoxId,IsTen);
		        if(reward!=null)
			        ItemPriceNodeModel = new ItemPriceNodeModel(reward, true);
	        }
	        else
	        {
		        ShopBuyState = ShopBuyState.None;
	        }

	        ShopManager.Instance.IsShowDrawRewardAnimation = true;
        }


        [Command]
        private void OnClickBtnUseItem()
        {
	        if(ChestBoxId==0) return;
	        if(ShopManager.Instance.IsShowDrawRewardAnimation) return;
	      

	        if (UIHelper.CheckRewardIsEnough(itemPriceNodeModel.Reward,true))
	        {
		        if (ChestBoxId == (int)EquipChestId.Clothes)
		        {
			        ShopManager.Instance.SendItemDrawClothes(ChestBoxId,itemPriceNodeModel.Reward,IsTen?10:1,DrawCallBack,false);
		        }
		        else
		        {
			        if (itemPriceNodeModel.Reward.Id == (int)GameConst.ItemIdCode.Stone)
			        {
				        ShopManager.Instance.SendDiamondDraw(ChestBoxId,DrawCallBack,false);
			        }
			        else
			        {
				        ShopManager.Instance.SendItemDraw(ChestBoxId,DrawCallBack,false);
			        }
		        }
	        }
        }
        
        [Command]
        private void OnClickBtnFree()
        {
	        if(ChestBoxId==0) return;
			ShopManager.Instance.SendAdDraw(ChestBoxId,DrawCallBack,false);
        }

        private void DrawCallBack(List<Resource> obj)
        {
	        InitRewardList(obj);
        }

        [Command]
        private void OnClickBtnClose()
        {
	        if(ShopManager.Instance.IsShowDrawRewardAnimation) return;
	        UIManager.Instance.CloseDialog<ShopDrawRewardView>();
	        CloseAction?.Invoke();
        }
    }
}