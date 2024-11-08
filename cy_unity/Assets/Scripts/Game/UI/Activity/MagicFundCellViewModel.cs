using System;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class MagicFundCellViewModel : ViewModelBase
    {
        
	    [AutoNotify] private ObservableList<CellItemViewModel> freeScrollviewList = new();
	    [AutoNotify] private ObservableList<CellItemViewModel> plusScrollviewList = new();
		[AutoNotify] private float progressValue;
		[AutoNotify] private string levelTextStr;
		[AutoNotify] private bool isShowMaskImg;
		[AutoNotify] private bool isShowProgress;
		[AutoNotify] private Vector3 progressLocalPos;
		private Action onClickClaimCallback;
		private EPassPortType curType;
		[AutoNotify] private ActivityFundCfg curCfg;
        [Preserve]
        public MagicFundCellViewModel(ActivityFundCfg cfg, Action claimCallback)
        {
	        curCfg = cfg;
	        onClickClaimCallback = claimCallback;

	        InitPanel();
	        UpdatePanel();
	        ProgressLocalPos = cfg.Factor == 7? new Vector3(0, 10, 0) : new Vector3(0, -10, 0);
        }

        protected override void OnDispose()
        {
	        foreach (var item in FreeScrollviewList)
	        {
		        item.Dispose();
	        }

	        foreach (var item in PlusScrollviewList)
	        {
		        item.Dispose();
	        }
	        FreeScrollviewList.Clear();
	        PlusScrollviewList.Clear();
	        base.OnDispose();
        }

        private ECellItemState GetCurCellState(EPassportRewardType rewardType)
        {
	        if (rewardType == EPassportRewardType.PassportTypeVip && !DataCenter.magicDrawData.FundPlus)
	        {
		        return ECellItemState.None;
	        }
	        
	        // // 是否可以领取
	        if (DataCenter.magicDrawData.Days < curCfg.Factor)
	        {
		        return ECellItemState.None;
	        }

	        // 是否已经领取
	        if (DataCenter.magicDrawData.CheckFundIsClaimed(rewardType, curCfg.Id))
	        {
		        return ECellItemState.Finish;
	        }

	        return ECellItemState.GetIng;
        }

        private void OnClickCellItem(object obj)
        {
	        onClickClaimCallback?.Invoke();
        }

        private void InitPanel()
        {
	        foreach (var item in curCfg.FreeReward)
	        {
		        CellItemViewModel tempVm = CellItemViewModel.Create(item);
		        tempVm.State = ECellItemState.None;
		        tempVm.IsShowLock = false;
		        freeScrollviewList.Add(tempVm); 
	        }
	        foreach (var item in curCfg.PassReward2)
	        {
		        CellItemViewModel tempVm = CellItemViewModel.Create(item);
		        tempVm.State = ECellItemState.None;
		        tempVm.IsShowLock = false;
		        plusScrollviewList.Add(tempVm); 
	        }
	        LevelTextStr = $"{curCfg.Factor}";
	        IsShowMaskImg = false;
	        IsShowProgress = false;
        }

        public void UpdatePanel()
        {
	        foreach (var item in FreeScrollviewList)
	        {
		        item.State = GetCurCellState(EPassportRewardType.PassportTypeFree);
		        item.IsShowLock = item.State == ECellItemState.None;
		        item.SetClickAction(item.State == ECellItemState.GetIng ? OnClickCellItem : null);
	        }
	        foreach (var item in PlusScrollviewList)
	        {
		        item.State = GetCurCellState(EPassportRewardType.PassportTypeVip);
		        item.IsShowLock = item.State == ECellItemState.None;
		        item.SetClickAction(item.State == ECellItemState.GetIng ? OnClickCellItem : null);
	        }
	        IsShowMaskImg = DataCenter.magicDrawData.Days < curCfg.Factor;
	        isShowProgress = DataCenter.magicDrawData.Days == curCfg.Factor - 1;
	        if (DataCenter.magicDrawData.Days >= curCfg.Factor)
	        {
		        ProgressValue = DataCenter.magicDrawData.Days > curCfg.Factor ? 1.0f : 0.5f;
	        }
	        else
	        {
		        ProgressValue = 0.0f;
	        } }
    }
}