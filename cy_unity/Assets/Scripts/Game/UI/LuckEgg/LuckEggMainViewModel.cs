using System;
using System.Collections.Specialized;
using System.ComponentModel;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public enum LuckEggShowView
    {
        Main, 
        Task,
        Fund,
        Exchange,

    }
    
    public partial class LuckEggMainViewModel : ViewModelBase
    {
	    public override bool AutoDispose => true;
	    [AutoNotify] private ObservableList<BottomOpCellItemViewModel> opBtnScrollViewList = new();
	    [AutoNotify] private BottomComponentViewModel bottomComponentVm;
	    public ActivityUIManager Manager = ActivityUIManager.Instance;
	    public LuckyEggData Data => DataCenter.luckyEggData;
	    public LuckEggExchangeViewModel LuckEggExchangeVm;
	    public LuckEggFundViewModel LuckEggFundVm;
	    public LuckEggDrawViewModel LuckEggDrawVm = new();
	    public LuckEggTaskViewModel LuckEggTaskVm;
        [Preserve]
        public LuckEggMainViewModel()
        {
	        InitBottomComponent();
	        LuckEggExchangeVm = new LuckEggExchangeViewModel();
	        LuckEggFundVm = new LuckEggFundViewModel();
	        LuckEggTaskVm = new LuckEggTaskViewModel();
	        RefreshTimeDesc();
	        DataCenter.luckyEggData.PropertyChanged += DataPropertyChanged;
	        DataCenter.luckyEggData.StageClaimed.CollectionChanged += StageClaimedChanged;
	        Manager.PropertyChanged += TabPropertyChanged;
        }
        protected override void OnDispose()
        {
	        base.OnDispose();
	        DataCenter.luckyEggData.PropertyChanged -= DataPropertyChanged;
	        DataCenter.luckyEggData.StageClaimed.CollectionChanged -= StageClaimedChanged;
	        Manager.PropertyChanged -= TabPropertyChanged;
        }

        private void StageClaimedChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
	        RefreshRedDot();
        }

        private void DataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
	        RefreshRedDot();
        }
        private void TabPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
	        OnChooseSubPassportBtn(Manager.EggTabType);
        }
        #region 底部页签初始化
	    private void InitBottomComponent()
        {
	        OpBtnScrollViewList.Clear();
	        foreach (LuckEggShowView tabType in Enum.GetValues(typeof(LuckEggShowView)))
	        {
				BottomOpCellData tempData = new();
		        switch (tabType)
		        {
			        case LuckEggShowView.Main:
			        {
				        tempData.ChooseIconPath = "mainui[icon_home_16]";
				        tempData.ChooseBgPath = "common[common_panel_10]";
				        tempData.OpType = tabType;
				        tempData.OpName = LocalizeHelper.GetGlobal(GlobalLanguageId.niudan05);
				        tempData.OnClickCallback = OnChooseSubPassportBtn;
			        } break;
			        case LuckEggShowView.Task:
			        {
				        tempData.ChooseIconPath = "school[school_icon_1]";
				        tempData.ChooseBgPath = "common[common_panel_10]";
				        tempData.OpType = tabType;
				        tempData.OpName = LocalizeHelper.GetGlobal(GlobalLanguageId.niudan10);
				        tempData.OnClickCallback = OnChooseSubPassportBtn;
				        tempData.IsShowRedDot = Data.IsTaskRed();
			        } break;
			        case LuckEggShowView.Fund:
			        {
				        tempData.ChooseIconPath = "school[school_icon_4]";
				        tempData.ChooseBgPath = "common[common_panel_10]";
				        tempData.OpType = tabType;
				        tempData.OpName = LocalizeHelper.GetGlobal(GlobalLanguageId.niudan11);
				        tempData.OnClickCallback = OnChooseSubPassportBtn;
				        tempData.IsShowRedDot = Data.IsFundRed();
			        } break;
			        case LuckEggShowView.Exchange:
			        {
				        tempData.ChooseIconPath = "school[school_icon_3]";
				        tempData.ChooseBgPath = "common[common_panel_10]";
				        tempData.OpType = tabType;
				        tempData.OpName = LocalizeHelper.GetGlobal(GlobalLanguageId.niudan12);
				        tempData.OnClickCallback = OnChooseSubPassportBtn;
			        } break;
		        }
		        BottomOpCellItemViewModel tempOpCell = new(tempData);
		        tempOpCell.isUseChoose = false;
		        OpBtnScrollViewList.Add(tempOpCell);
	        }
            BottomComponentVm = new BottomComponentViewModel(OnClickCloseBtn,OpBtnScrollViewList);
            OnChooseSubPassportBtn(Data.IsExchangeTime?LuckEggShowView.Exchange:LuckEggShowView.Main);
            RefreshRedDot();
        }
        private void OnChooseSubPassportBtn(Object type)
        {
	        if (Data.IsExchangeTime)
	        {
		        if ((LuckEggShowView)type != LuckEggShowView.Exchange)
		        {
			        ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.niudan17));
		        }
		        type = LuckEggShowView.Exchange;
	        }
	        foreach (var item in OpBtnScrollViewList)
	        {
		        item.CurOpCellData.IsChoose = (int)item.CurOpCellData.OpType == (int)type;
	        }
	        Manager.EggTabType = (LuckEggShowView)type;
        }

        private void RefreshRedDot()
        {
	        foreach (var item in OpBtnScrollViewList)
	        {
		        if ((LuckEggShowView)item.CurOpCellData.OpType == LuckEggShowView.Main)
		        {
			        item.CurOpCellData.IsShowRedDot = ActivityUIManager.Instance.CheckLuckEggDrawRed();
		        }
		        if ((LuckEggShowView)item.CurOpCellData.OpType == LuckEggShowView.Fund)
			        item.CurOpCellData.IsShowRedDot = Data.IsFundRed();
		        if ((LuckEggShowView)item.CurOpCellData.OpType == LuckEggShowView.Task)
			        item.CurOpCellData.IsShowRedDot = Data.IsTaskRed();
	        }
        }

        private void OnClickCloseBtn()
        {
	        UIManager.Instance.CloseDialog<LuckEggMainView>();
        }

        #endregion

        #region 倒计时

        private float interval;
        private void RefreshTimeDesc()
        {
	        if (Data.IsExchangeTime)
	        {
		        OnChooseSubPassportBtn(LuckEggShowView.Exchange);
		        foreach (var item in OpBtnScrollViewList)
		        {
			        if ((LuckEggShowView)item.CurOpCellData.OpType != LuckEggShowView.Exchange)
			        {
				        item.IsGrayIcon = true;
			        }
		        }
		        return;
	        }

	        if (Data.EndExchangeStamp <= ServerTime.Instance.GetNowTime())
	        {
		        UIManager.Instance.CloseDialog<LuckEggMainView>();
	        }
	        
        }
        public override void Update()
        {
	        if (UIHelper.CalculateTime(ref interval))
	        {
		        RefreshTimeDesc();
		        RefreshRedDot();
	        }
        }

        #endregion
    }
}