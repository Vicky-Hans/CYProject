using System;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using Game.UI.MainUi;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class ActivityCellViewModel : ViewModelBase
    {
        
		[AutoNotify] private string cellBgPath;
		[AutoNotify] private string titleBgPath;
		[AutoNotify] private bool isShowLockTitle;
		[AutoNotify] private string nameTextStr;
		[AutoNotify] private bool isShowLeftTime;
		[AutoNotify] private string leftTimeTextStr;
		// [AutoNotify] private Vector3 awardScrollviewPos = ; 
	    [AutoNotify] private ObservableList<CellItemBaseViewModel> awardScrollviewList = new();
		[AutoNotify] private bool isShowRedDotNode;
		[AutoNotify] private string leftCountTextStr;
		[AutoNotify] private bool isShowLockNode;
		[AutoNotify] private string lockDescTextStr;
		[AutoNotify] private bool isShowLeftCountNode;

		private EActivityShowType curType;
		private Action<EActivityShowType> clickOpBtnCallback; 
        [Preserve]
        public ActivityCellViewModel(EActivityShowType type, Action<EActivityShowType> callback)
        {
	        curType = type;
	        clickOpBtnCallback = callback;
	        InitPanel();
	        
        }

        public override void Update()
        {
	        base.Update();
	        UpdateLeftTime();
        }


        [Command]
        private void OnClickOpBtn()
        {
	        clickOpBtnCallback.Invoke(curType);
        }

        private void InitPanel()
        {
	        switch (curType)
	        {
		        case EActivityShowType.DailyChallenge: InitDailyChallenge(); break;
		        case EActivityShowType.Endless: InitEndLess(); break;
		        case EActivityShowType.Secret: InitSecret(); break;
	        }
        }
        
        private void InitDailyChallenge()
        {
	        IsShowLeftCountNode = true;
	        CellBgPath = "daily[daily_banner_02]";
	        TitleBgPath = "daily[daily_panel_7]";
	        IsShowLockTitle = !MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionDailyFight);
	        var functionOpenCfg = ConfigCenter.FunctionOpenLanguageCfgColl.GetDataById((int)EFunctionOpenType.FunctionDailyFight);
	        NameTextStr = functionOpenCfg.Name;
	        IsShowLockNode = IsShowLockTitle;
	        if (!IsShowLockTitle)
	        {
		        UpdateLeftTime();
	        }
	        else
	        {
		        LockDescTextStr = MainUiManager.Instance.GetFunctionUnLockTips(EFunctionOpenType.FunctionDailyFight);
	        }
	        var dailyFightAwardsCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.dailyStage_09);
	        AwardScrollviewList.Clear();
	        if (dailyFightAwardsCfg!= null && dailyFightAwardsCfg.Content.Count > 0)
	        {
		        for (var i = 0; i < dailyFightAwardsCfg.Content.Count; i++)
		        {
			        var vm = new CellItemBaseViewModel(dailyFightAwardsCfg.Content[i], (int)RewardType.Item, 1, ECellItemSizeType.Size90X80, false, false);
			        AwardScrollviewList.Add(vm);
		        }
	        }
        }
        private void InitEndLess()
        {
	        IsShowLeftCountNode = true;
	        CellBgPath = "daily[daily_banner_01]";
	        TitleBgPath = "daily[daily_panel_6]";
	        IsShowLockTitle = !MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionEndless);
	        var functionOpenCfg = ConfigCenter.FunctionOpenLanguageCfgColl.GetDataById((int)EFunctionOpenType.FunctionEndless);
	        NameTextStr = functionOpenCfg.Name;
	        IsShowLockNode = IsShowLockTitle;
	        if (IsShowLockTitle != true)
	        {
		        UpdateLeftTime();
	        }
	        else
	        {
		        LockDescTextStr = MainUiManager.Instance.GetFunctionUnLockTips(EFunctionOpenType.FunctionEndless); 
	        }
	        var endlessAwardsCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.endless_12);
	        AwardScrollviewList.Clear();
	        if (endlessAwardsCfg!= null && endlessAwardsCfg.Content.Count > 0)
	        {
		        for (var i = 0; i < endlessAwardsCfg.Content.Count; i++)
		        {
			        var vm = new CellItemBaseViewModel(endlessAwardsCfg.Content[i], (int)RewardType.Item, 1, ECellItemSizeType.Size90X80, false, false);
			        AwardScrollviewList.Add(vm);
		        }
	        }
        }
        private void InitSecret()
        {
	        IsShowLeftCountNode = false;
	        CellBgPath = "secret[secret_banner_2]";
	        TitleBgPath = "secret[secret_panel_3]";
	        IsShowLockTitle = !MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionSecret);
	        var functionOpenCfg = ConfigCenter.FunctionOpenLanguageCfgColl.GetDataById((int)EFunctionOpenType.FunctionSecret);
	        if (functionOpenCfg == null)
	        {
		        DHLog.Error($"没有 多语言 配置 请检查表 FunctionOpenLanguageCfg Id {(int)EFunctionOpenType.FunctionSecret}");
		        NameTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Secret_01);
	        }
	        else
	        {
		        NameTextStr = functionOpenCfg.Name;
	        }
	        IsShowLockNode = IsShowLockTitle;
	        if (!IsShowLockTitle)
	        {
		        UpdateLeftTime();
	        }
	        else
	        {
		        LockDescTextStr = MainUiManager.Instance.GetFunctionUnLockTips(EFunctionOpenType.FunctionSecret);
                
	        }

	    
	        
	        AwardScrollviewList.Clear();
	        var rewards = DataCenter.secretData.GetSecretRewardList();
	        for (var i = 0; i < rewards.Count; i++)
	        {
		        var vm = new CellItemBaseViewModel(rewards[i], (int)RewardType.Item, 1, ECellItemSizeType.Size90X80, false, false);
		        AwardScrollviewList.Add(vm);
	        }
        }
        private void UpdateLeftTime()
        {
	        switch (curType)
	        {
		        case EActivityShowType.DailyChallenge: UpdateDailyChallengeLeftTime(); break;
		        case EActivityShowType.Endless: UpdateEndlessLeftTime(); break;
		        case EActivityShowType.Secret: UpdateSecretLeftTime(); break;
	        }
        }

        private void UpdateDailyChallengeLeftTime()
        {
	        if (!MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionDailyFight))
	        {
		        IsShowLeftTime = false;
		        return;
	        }
	        
	        if (DataCenter.dailyFightData.DayRefreshStamp >= ServerTime.Instance.GetNowTime())
	        {
		        var cd = DataCenter.dailyFightData.DayRefreshStamp - ServerTime.Instance.GetNowTime();
		        var str = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.endless_02).Name;
		        LeftTimeTextStr = $"{str}{UIHelper.ConvertTimeSecondToString(cd, ETimeFormatType.TimeFormatChampion)}";
	        }
	        LeftCountTextStr = $"{LocalizeHelper.GetGlobal(GlobalLanguageId.endless_04)}:{DataCenter.dailyFightData.Count}";
	        
	        IsShowRedDotNode =!IsShowLockTitle && DataCenter.dailyFightData.CheckIsShowRedDot();
	        IsShowLeftTime = true;

        }

        private void UpdateEndlessLeftTime()
        {
	        if (!MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionEndless))
	        {
		        IsShowLeftTime = false;
		        return;
	        }

	        if (DataCenter.endlessData.RefreshStamp >= ServerTime.Instance.GetNowTime())
	        {
		        var cd = DataCenter.endlessData.RefreshStamp - ServerTime.Instance.GetNowTime();
		        var str = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.endless_02).Name;
		        LeftTimeTextStr = $"{str}{UIHelper.ConvertTimeSecondToString(cd, ETimeFormatType.TimeFormatChampion)}";
	        }

	        var leftNum = DataCenter.endlessData.Count;
	        if (leftNum < 0) leftNum = 0;
	        LeftCountTextStr = $"{LocalizeHelper.GetGlobal(GlobalLanguageId.endless_17)}{leftNum}";
	        IsShowRedDotNode = IsShowLockTitle && DataCenter.endlessData.Count > 0;
	        IsShowLeftTime = true;
        }

        private void UpdateSecretLeftTime()
        {
	        if (!MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionSecret))
	        {
		        IsShowLeftTime = false;
		        return;
	        }
	        // 这个活动没有倒计时
	        IsShowLeftTime = false;
        }

        public void UpdatePanel()
        {
	        
        }
    }
}