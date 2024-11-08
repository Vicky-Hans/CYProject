using System;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;


namespace DH.Game.ViewModels
{



	public partial class BoosterPackItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string bgPath;
		[AutoNotify] private bool isShowArrowNode;
	    [AutoNotify] private ObservableList<SelectCellItemEffectViewModel> awardScrollviewList = new();
		[AutoNotify] private bool isShowLockPath;
		[AutoNotify] private BtnPriceNodeModel btnPriceNodeVm;
		[AutoNotify] private string limitNumsStr;
		[AutoNotify] private string arrowBgPath;
		[AutoNotify] private string arrowImgPath;
		[AutoNotify] private string opBtnImgPath;
		[AutoNotify] private EBoosterPackState curState;


		private TriggerGiftCfg curCfg;
		private Action<TriggerGiftCfg> callback;
		private TriggerGiftOneData curTriggerData;
		[Preserve]
		public BoosterPackItemViewModel(TriggerGiftCfg cfg, Action<TriggerGiftCfg> opCallback)
        {
	        curCfg = cfg;
	        callback = opCallback;
	        if (DataCenter.triggerGiftData.Data.TryGetValue(cfg.Type, out var value))
	        {
		        curTriggerData = value;
	        }
	        InitPanel();
	        
	        DataCenter.triggerGiftData.PropertyChanged += OptionalChange;
        }

        protected override void OnDispose()
        {
	        DataCenter.triggerGiftData.PropertyChanged -= OptionalChange;
	        base.OnDispose();
        }

        private void OptionalChange(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(DataCenter.triggerGiftData.OptionalRecord))
	        {
		        InitRewardsList();
	        }
        }

        

        private void InitPanel()
        {
	        InitRewardsList();
	        var alreadyBytCount = DataCenter.triggerGiftData.GetAlreadyBuyCount(curCfg.Type, curCfg.Id);
	        LimitNumsStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips18, curCfg.BuyLimit -alreadyBytCount, curCfg.BuyLimit);
	        curState = DataCenter.triggerGiftData.GetCurBoosterPackState(curCfg);
	        UpdatePanel();

        }

        private void InitRewardsList()
        {
	        AwardScrollviewList.Clear();
	        
	        if (curCfg.OptionalReward is { Count: > 0 })
	        {
		        var selectItemViewModel = SelectCellItemEffectViewModel.Create(curCfg.OptionalReward,ECellItemSizeType.Size132X132,
			        selectIndex:DataCenter.triggerGiftData.GetSelectPacket(curCfg.Id));
		        selectItemViewModel.ClickEvent = OnClickSelectReward;
		        selectItemViewModel.CellItemBaseViewVm.State = GetState();
		        selectItemViewModel.CellItemBaseViewVm.SetClickAction(GetState() == ECellItemState.Finish? null: OnClickSelectReward);
		        selectItemViewModel.CellItemBaseViewVm.CellItemBaseViewVm .IsOpenMask = true;
		        AwardScrollviewList.Add(selectItemViewModel);
	        }
	        
	        if (curCfg.Reward is { Count: > 0 })
	        {
		        for (int i = 0; i<curCfg.Reward.Count ; i++)
		        {
			        var reward = curCfg.Reward[i];
			        var selectItemViewModel = SelectCellItemEffectViewModel.Create(reward,ECellItemSizeType.Size132X132);
			        selectItemViewModel.CellItemBaseViewVm.State = GetState();
			        selectItemViewModel.CellItemBaseViewVm.CellItemBaseViewVm .IsOpenMask = true;
			        AwardScrollviewList.Add(selectItemViewModel);
		        }
	        }
        }

        ECellItemState GetState()
        {
	        if (curState == EBoosterPackState.SoldOut)
	        {
		        return ECellItemState.Finish;
	        }
	        if (curState == EBoosterPackState.CanOp && curCfg.Package == 0)
	        {
		        return ECellItemState.GetIng;
	        }
	        return ECellItemState.None;
        }

        [Command]
        private void OnClickOpBtn()
        {
	        if (curState == EBoosterPackState.Lock)
	        {
		        ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.AgeMagic_05));
		        return;
	        }

	        if(curState != EBoosterPackState.CanOp)
	        {
		        return;
	        }
	        if (curCfg.OptionalReward is { Count: > 0 } && !DataCenter.triggerGiftData.CheckSelectPacket(curCfg.Id))
	        {
		        ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.Trigger08));
		        return;
	        }
	        callback?.Invoke(curCfg);
        }

		public void UpdatePanel()
		{
			curState = DataCenter.triggerGiftData.GetCurBoosterPackState(curCfg);
			if (btnPriceNodeVm != null)
			{
				btnPriceNodeVm.Dispose();
			}

			IsShowLockPath = curState == EBoosterPackState.Lock;

			if (curState == EBoosterPackState.SoldOut)
			{
				var str = LocalizeHelper.GetGlobal(GlobalLanguageId.AgeMagic_03);
				btnPriceNodeVm = new(str);
				btnPriceNodeVm.IsShowIcon = false;
			} 
			else
			{
				if (curCfg.Package == -1)
				{
					var str = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips16);
					btnPriceNodeVm = new(str);
					btnPriceNodeVm.IsShowIcon = false;
				}
				else
				{
					btnPriceNodeVm = new(curCfg.Package);
					btnPriceNodeVm.IsShowIcon = false;
				}
			}
			
			var bgImg = "magic[magic_panel_2]";
			var arrowbgImg = "magic[magic_panel_7]";
			var arrowImg = "magic[magic_panel_5]";
			var opBtnImg = "common[common_button_grey2]";
			switch (curState)
			{
				case EBoosterPackState.Lock:
				{
					bgImg = "magic[magic_panel_3]";
					opBtnImg = curCfg.Package == -1 ? "common[commom_button_green3]" : "common[commom_button_yellow3]";
					break;
					
				}
				case EBoosterPackState.CanOp:
				{
					bgImg = "magic[magic_panel_2]";
					opBtnImg = curCfg.Package == -1 ? "common[commom_button_green3]" : "common[commom_button_yellow3]";
					break;
				}
				case EBoosterPackState.SoldOut:
				{
					bgImg = "magic[magic_panel_4]";
					arrowbgImg = "magic[magic_panel_8]";
					arrowImg = "magic[magic_panel_6]";
					opBtnImg = "common[common_button_grey2]";
					break;
				}
			}

			BgPath = bgImg;
			ArrowBgPath = arrowbgImg;
			ArrowImgPath = arrowImg;
			OpBtnImgPath = opBtnImg;
			
			UpdateRewardsList();
		}

		private void UpdateRewardsList()
		{
			foreach (var item in AwardScrollviewList)
			{
				item.CellItemBaseViewVm.State = GetState();
			}
		}

		private void OnClickSelectReward()  
        {
            if (curState == EBoosterPackState.SoldOut)return;
            if(curCfg ==null || curCfg.OptionalReward ==null || curCfg.OptionalReward.Count==0) return;
            UIManager.Instance.OpenDialog<CommonSelectRewardView>(new CommonSelectRewardViewModel(
	            curCfg.OptionalReward,
	            DataCenter.triggerGiftData.GetSelectPacket(curCfg.Id)
                ,(selectIndex)=> {
        	        DataCenter.triggerGiftData.SetSelectPacket(curCfg.Id,selectIndex);
                })).Forget();
        }
        private void OnClickSelectReward(Tuple<Vector3, Vector3> info)
        {
	        OnClickSelectReward();
        }


    }
}