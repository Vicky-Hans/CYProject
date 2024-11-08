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
using Game.UI.MainUi;
using DH.UIFramework.Observables;
namespace DH.Game.ViewModels
{
    public partial class ActivityViewModel : ViewModelBase
    {

	    [AutoNotify] private ObservableList<ActivityCellViewModel> activityScrollviewList =new ();
	    [Preserve]
        public ActivityViewModel()
        {
	        
	        ActivityScrollviewList.Clear();
	        foreach (EActivityShowType showType in Enum.GetValues(typeof(EActivityShowType)))
	        {
		        switch (showType)
		        {
			        case EActivityShowType.DailyChallenge:
			        {
				        // bool isOpen = MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionDailyFight);
				        // if(isOpen != true) continue;
				        
				        // var dailyFightOpenTime = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.dailyStage_05);
				        // if (dailyFightOpenTime == null || dailyFightOpenTime.Content.Count <= 0)
				        // {
					       //  continue;
				        // }
				        var tempVm = new ActivityCellViewModel(showType, OnClickActivityCellBtn);
				        ActivityScrollviewList.Add(tempVm);
			        } break;
			        case EActivityShowType.Endless:
			        {
				        // bool isOpen = MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionEndless);
				        // if(isOpen != true) continue;
				        var tempVm = new ActivityCellViewModel(showType, OnClickActivityCellBtn);
				        ActivityScrollviewList.Add(tempVm);
			        } break;
			        case EActivityShowType.Secret:
			        {
				        continue;
				        // bool isOpen = MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionSecret);
				        // if(isOpen != true) continue;
				        var tempVm = new ActivityCellViewModel(showType, OnClickActivityCellBtn);
				        ActivityScrollviewList.Add(tempVm);
			        } break;

		        }
	        }
	        DataCenter.dailyFightData.PropertyChanged += OnDailyFightDataChanged;
	        DataCenter.endlessData.PropertyChanged += OnEndlessDataChanged;
	        MainUiManager.Instance.PropertyChanged -= OnMainUiManagerChanged;
        }
	    
        protected override void OnDispose()
        {
	        DataCenter.dailyFightData.PropertyChanged -= OnDailyFightDataChanged;
	        DataCenter.endlessData.PropertyChanged -= OnEndlessDataChanged;
	        MainUiManager.Instance.PropertyChanged -= OnMainUiManagerChanged;
	        base.OnDispose();
        }

        private void OnClickActivityCellBtn(EActivityShowType type)
        {
	        switch (type)
	        {
		        case EActivityShowType.DailyChallenge: OnClickChallengeBtn() ;break;		        
		        case EActivityShowType.Endless: OnClickEndlessBtn() ;break;	
		        case EActivityShowType.Secret:OnClickSecretBtn();break;
	        }
        }

        private void OnMainUiManagerChanged(object sender, PropertyChangedEventArgs e)
        {
	        if(e.PropertyName== nameof(MainUiManager.Instance.ShowChapterIndex))
	        {
		        foreach (var item in ActivityScrollviewList)
		        {
			        item.UpdatePanel();
		        }
	        }
        }
        /// <summary>
        /// 监听无尽关卡数据变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEndlessDataChanged(object sender, PropertyChangedEventArgs e)
        {
	        foreach (var item in ActivityScrollviewList)
	        {
		        item.UpdatePanel();
	        }
        }
        /// <summary>
        /// 监听无尽关卡数据变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDailyFightDataChanged(object sender, PropertyChangedEventArgs e)
        {
	        foreach (var item in ActivityScrollviewList)
	        {
		        item.UpdatePanel();
	        }
        }
        /// <summary>
        /// 点击进入每日挑战
        /// </summary>
        private void OnClickChallengeBtn()
        {
	        bool isOpen = MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionDailyFight);
	        if (isOpen != true)
	        {
		        var tips = MainUiManager.Instance.GetFunctionUnLockTips(EFunctionOpenType.FunctionDailyFight);
		        ToastManager.Show(tips);
		        return;
	        }
				        
	        var dailyFightOpenTime = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.dailyStage_05);
	        if (dailyFightOpenTime == null || dailyFightOpenTime.Content.Count <= 0)
	        {
				
		        return;
	        }
	        UIManager.Instance.OpenDialog<ChallengeActivityView,ChallengeActivityViewModel>().Forget();
        }
        /// <summary>
        /// 点击进入无尽关卡
        /// </summary>
        private void OnClickEndlessBtn()
        {
	        bool isOpen = MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionEndless);
	        if (isOpen != true)
	        {
		        var tips = MainUiManager.Instance.GetFunctionUnLockTips(EFunctionOpenType.FunctionEndless);
		        ToastManager.Show(tips);
		        return;
	        }
	        UIManager.Instance.OpenDialog<EndlessActivityView,EndlessActivityViewModel>().Forget();
        }

        private void OnClickSecretBtn()
        {
	        bool isOpen = MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionSecret);
	        if (isOpen != true)
	        {
		        var tips = MainUiManager.Instance.GetFunctionUnLockTips(EFunctionOpenType.FunctionSecret);
		        ToastManager.Show(tips);
		        return;
	        }
	        UIManager.Instance.OpenDialog<SecretView,SecretViewModel>().Forget();
        }
    }
}