using System;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class PassportsCellViewModel : ViewModelBase
    {
        
	    [AutoNotify] private ObservableList<CellItemViewModel> normalScrollviewList = new();
		[AutoNotify] private bool isShowNormalMaskNode;
	    [AutoNotify] private ObservableList<CellItemViewModel> plusScrollviewList = new();
		[AutoNotify] private bool isShowPlusMaskNode;
		[AutoNotify] private string levelTextStr;
		[AutoNotify] private bool isCanDrag;
		[AutoNotify] private bool isShowUnLockBtn;
		[AutoNotify] private string normalBgPath;
		[AutoNotify] private string plusBgPath;
		/// <summary>
		///  背景图片数组 已领取，可领取，不可领取
		/// </summary>
		private string[] bgImgPathArray = new string[] { 
			"pass[pass_panel_5]", "pass[pass_panel_8]","pass[pass_panel_4]", 
			"pass[pass_panel_7]", "pass[pass_panel_9]","pass[pass_panel_6]"
		};
		private string[] plusBgImgPathArray = new string[] { 
			"pass[pass_panel_11]", "pass[pass_panel_15]","pass[pass_panel_10]", 
			"pass[pass_panel_12]", "pass[pass_panel_14]","pass[pass_panel_13]"
		};
		private string[] ChapterImgPathArray = new string[] { 
			"pass[pass_panel_22]", "pass[pass_panel_21]","pass[pass_panel_18]",
			"pass[pass_panel_17]", "pass[pass_panel_20]","pass[pass_panel_19]", 
		};
	    private int bgIndex;
        [AutoNotify] private bool isShowBgNode;
		private FundRewardsCfg curCfg;
		private Action<FundRewardsCfg> onClickUnlockCallback;
		private Action<FundRewardsCfg> onClickClaimCallback;
		private EPassPortType curType;
        [Preserve]
        public PassportsCellViewModel(FundRewardsCfg cfg, Action<FundRewardsCfg> unlockCallback, Action<FundRewardsCfg> claimCallback, EPassPortType type)
        {
	        curCfg = cfg;
	        onClickUnlockCallback = unlockCallback;
	        onClickClaimCallback = claimCallback;
	        curType = type;
	        
	        foreach (var item in cfg.FreeReward)
	        {
		        CellItemViewModel tempVm = CellItemViewModel.Create(item);
		        tempVm.State = GetCurCellState(EPassportRewardType.PassportTypeFree);
		        tempVm.IsShowLock = tempVm.State == ECellItemState.None;
		        tempVm.SetClickAction(tempVm.State == ECellItemState.GetIng ? OnClickCellItem : null);
		        NormalScrollviewList.Add(tempVm); 
	        }
	        foreach (var item in cfg.PassReward2)
	        {
		        CellItemViewModel tempVm = CellItemViewModel.Create(item);
		        tempVm.State = GetCurCellState(EPassportRewardType.PassportTypeVip);
		        tempVm.IsShowLock = tempVm.State == ECellItemState.None;
		        tempVm.SetClickAction(tempVm.State == ECellItemState.GetIng ? OnClickCellItem : null);
		        PlusScrollviewList.Add(tempVm); 
	        }
	        LevelTextStr = $"{cfg.Factor}";
	        if (curType != EPassPortType.PassportTypeChapter)
	        {
		        var passportData = DataCenter.allPassportData.GetPassportData(curType);
		        if (passportData != null)
		        {
			        isShowUnLockBtn = passportData.Lv == cfg.Factor-1;
		        }
	        }
	        else
	        {
		        isShowUnLockBtn = false;
	        }

	        UpdateBgState();
        }

        protected override void OnDispose()
        {
	        foreach (var item in NormalScrollviewList)
	        {
		        item.Dispose();
	        }
	        foreach (var item in PlusScrollviewList)
            {
                item.Dispose();
            }
	        NormalScrollviewList.Clear();
	        PlusScrollviewList.Clear();
	        base.OnDispose();
        }

        private ECellItemState GetCurCellState(EPassportRewardType rewardType)
        {
	        if (curType == EPassPortType.PassportTypeChapter)
	        {
		        if (rewardType == EPassportRewardType.PassportTypeVip && 
		            DataCenter.chapterFundData.IsBuy )
		        {
			        var isCanGet = DataCenter.chapterFundData.IsCanGetAward(curCfg.Id,curCfg.Factor, true);
			        var isGet = DataCenter.chapterFundData.IsGetAward(curCfg.Id, true);
			        return isCanGet ? ECellItemState.GetIng :(isGet ? ECellItemState.Finish : ECellItemState.None);
		        }
		         
		        if (rewardType == EPassportRewardType.PassportTypeFree)
		        {
			        var isCanGet = DataCenter.chapterFundData.IsCanGetAward(curCfg.Id,curCfg.Factor);
			        var isGet = DataCenter.chapterFundData.IsGetAward(curCfg.Id);
			        return isCanGet?  ECellItemState.GetIng :(isGet ? ECellItemState.Finish : ECellItemState.None);
		        }
		        
		        return ECellItemState.None;
	        }
	        
	        var passportData = DataCenter.allPassportData.GetPassportData(curType);
	        if (passportData == null)
	        {
		        return ECellItemState.None;
	        }

	        if (rewardType == EPassportRewardType.PassportTypeVip && !passportData.Plus)
	        {
		        return ECellItemState.None;
	        }
	        // 是否可以领取
	        if (!DataCenter.allPassportData.CheckIsCanClaim(curType, curCfg.Factor))
	        {
		        return ECellItemState.None;
	        }

	        // 是否已经领取
	        if (DataCenter.allPassportData.CheckIsClaimed(curType, rewardType, curCfg.Factor))
	        {
		        return ECellItemState.Finish;
	        }

	        return ECellItemState.GetIng;
        }

        [Command]
        private void OnClickUnLockBtn()
        {
	        onClickUnlockCallback?.Invoke(curCfg);
        }

        private void OnClickCellItem(Tuple<Vector3, Vector3> info)
        {
	        onClickClaimCallback?.Invoke(curCfg);
        }

        private void UpdateBgState()
        {
	        var normalState = GetCurCellState(EPassportRewardType.PassportTypeFree);
	        UpdateBgPath(normalState, EPassportRewardType.PassportTypeFree);
	        var plusState = GetCurCellState(EPassportRewardType.PassportTypeVip);
	        UpdateBgPath(plusState, EPassportRewardType.PassportTypeVip);
	        DHLog.Debug($"muzili log passport cfg is {curCfg.Factor}  id is {curCfg.Id} normalState is {normalState}  plusState is  {plusState} ");
        }

        private void UpdateBgPath(ECellItemState state, EPassportRewardType rewardType)
        {
	        switch (state)
	        {
		        // 不可领取
		        case ECellItemState.None:
		        {
			        int index = rewardType == EPassportRewardType.PassportTypeFree ? 2 : 5;
			        UpdateBgState(rewardType, index);
		        }  break;
		        case ECellItemState.GetIng:
		        {
			        int index = rewardType == EPassportRewardType.PassportTypeFree ? 1 : 4;
			        UpdateBgState(rewardType, index);
		        }  break;
		        case ECellItemState.Finish:
		        {
			        int index = rewardType == EPassportRewardType.PassportTypeFree ? 0 : 3;
			        UpdateBgState(rewardType, index);
		        }  break;
	        }
        }

        private void UpdateBgState(EPassportRewardType rewardType, int bgIndex)
        {
	        if(rewardType == EPassportRewardType.PassportTypeFree)
	        {
		        NormalBgPath = curType switch
		        {
			        EPassPortType.PassportTypeDiscount => bgImgPathArray[bgIndex],
			        EPassPortType.PassportTypeStone => plusBgImgPathArray[bgIndex],
			        EPassPortType.PassportTypeChapter => ChapterImgPathArray[bgIndex],
		        };
	        }
	        else if (rewardType == EPassportRewardType.PassportTypeVip)
	        {
		        PlusBgPath = curType switch
		        {
			        EPassPortType.PassportTypeDiscount => bgImgPathArray[bgIndex],
			        EPassPortType.PassportTypeStone => plusBgImgPathArray[bgIndex],
			        EPassPortType.PassportTypeChapter => ChapterImgPathArray[bgIndex],
		        };
	        }
        }

        public void UpdateState()
        {
	        foreach (var item in NormalScrollviewList)
	        {
		        item.State = GetCurCellState(EPassportRewardType.PassportTypeFree);
		        item.IsShowLock = item.State == ECellItemState.None;
		        item.SetClickAction(item.State == ECellItemState.GetIng ? OnClickCellItem : null);
	        }
	        foreach (var item in plusScrollviewList)
	        {
		        item.State = GetCurCellState(EPassportRewardType.PassportTypeVip);
		        item.IsShowLock = item.State == ECellItemState.None;
		        item.SetClickAction(item.State == ECellItemState.GetIng ? OnClickCellItem : null);
	        }
	        
	        var passportData = DataCenter.allPassportData.GetPassportData(curType);
	        if (passportData != null)
	        {
		        IsShowUnLockBtn = passportData.Lv == curCfg.Factor-1;
	        }
	       
	        UpdateBgState();
        }
    }
}