using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
	public enum MagicBingoShowType
	{
		Main,
		Task,
		Exchange,
		Gift
	}

	public partial class MagicBingoViewModel : ViewModelBase
    {
	    public override bool AutoDispose => true;
	    public MagicBingoGiftViewModel MagicBingoGiftVm;
	    public MagicBingoExchangeViewModel MagicBingoExchangeVm;
	    public MagicBingoTaskViewModel MagicBingoTaskVm;
	    public MagicBingoBGViewModel MagicBingoBgVm;
	    public ActivityUIManager Manager = ActivityUIManager.Instance;
	    private MagicBingoData Data => DataCenter.mgicBingoData;
        [Preserve]
        public MagicBingoViewModel()
        {
	        MagicBingoGiftVm = new MagicBingoGiftViewModel();
	        MagicBingoExchangeVm = new MagicBingoExchangeViewModel();
	        MagicBingoTaskVm = new MagicBingoTaskViewModel();
	        MagicBingoBgVm = new MagicBingoBGViewModel();
	        InitBottomComponent();
        }
        
	    #region 底部页签初始化
	    [AutoNotify] private ObservableList<BottomOpCellItemViewModel> opBtnScrollViewList = new();
	    [AutoNotify] private BottomComponentViewModel bottomComponentVm;
	    
	    
	    private void InitBottomComponent()
        {
	        OpBtnScrollViewList.Clear();
	        foreach (MagicBingoShowType tabType in Enum.GetValues(typeof(MagicBingoShowType)))
	        {
				BottomOpCellData tempData = new();
		        switch (tabType)
		        {
			        case MagicBingoShowType.Main:
			        {
				        tempData.ChooseIconPath = "bingo[bingo_icon_1]";
				        tempData.ChooseBgPath = "common[common_panel_10]";
				        tempData.OpType = tabType;
				        tempData.OpName = LocalizeHelper.GetGlobal(GlobalLanguageId.Bingo_02);
				        tempData.OnClickCallback = OnChooseSubPassportBtn;
				        tempData.IsShowRedDot = Data.BinGoCountAwardRed();
			        } break;
			        case MagicBingoShowType.Task:
			        {
				        tempData.ChooseIconPath = "school[school_icon_1]";
				        tempData.ChooseBgPath = "common[common_panel_10]";
				        tempData.OpType = tabType;
				        tempData.OpName = LocalizeHelper.GetGlobal(GlobalLanguageId.Bingo_03);
				        tempData.OnClickCallback = OnChooseSubPassportBtn;
				        tempData.IsShowRedDot = Data.BinGoTaskRed();
			        } break;
			        case MagicBingoShowType.Exchange:
			        {
				        tempData.ChooseIconPath = "bingo[bingo_icon_2]";
				        tempData.ChooseBgPath = "common[common_panel_10]";
				        tempData.OpType = tabType;
				        tempData.OpName = LocalizeHelper.GetGlobal(GlobalLanguageId.Bingo_04);
				        tempData.OnClickCallback = OnChooseSubPassportBtn;
				        //tempData.IsShowRedDot = Data.IsFundRed();
			        } break;
			        case MagicBingoShowType.Gift:
			        {
				        tempData.ChooseIconPath = "school[school_icon_3]";
				        tempData.ChooseBgPath = "common[common_panel_10]";
				        tempData.OpType = tabType;
				        tempData.OpName = LocalizeHelper.GetGlobal(GlobalLanguageId.Bingo_05);
				        tempData.OnClickCallback = OnChooseSubPassportBtn;
			        } break;
		        }
		        BottomOpCellItemViewModel tempOpCell = new(tempData);
		        tempOpCell.isUseChoose = false;
		        OpBtnScrollViewList.Add(tempOpCell);
		        if (Data.IsTimeOver())
		        {
			        OnChooseSubPassportBtn(MagicBingoShowType.Exchange);
		        }
		        else
		        {
			        OnChooseSubPassportBtn(MagicBingoShowType.Main);
		        }

	        }
            BottomComponentVm = new BottomComponentViewModel(OnClickCloseBtn,OpBtnScrollViewList);
            RefreshRedDot();
        }
        private void OnChooseSubPassportBtn(Object type)
        {
	        
	        if (Data.IsExchangeTime)
	        {
		        if ((MagicBingoShowType)type != MagicBingoShowType.Exchange)
		        {
			        ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.niudan17));
		        }
		        type = MagicBingoShowType.Exchange;
	        }

	        foreach (var item in OpBtnScrollViewList)
	        {
		        item.CurOpCellData.IsChoose = (int)item.CurOpCellData.OpType == (int)type;
	        }
	        Manager.BagicBingo = (MagicBingoShowType)type;
        }

        private void RefreshRedDot()
        {
	        foreach (var item in OpBtnScrollViewList)
	        {
		        if ((MagicBingoShowType)item.CurOpCellData.OpType == MagicBingoShowType.Main)
		        {
			        item.CurOpCellData.IsShowRedDot = Data.BinGoCountAwardRed();
		        }
		        if ((MagicBingoShowType)item.CurOpCellData.OpType == MagicBingoShowType.Task)
			        item.CurOpCellData.IsShowRedDot = Data.BinGoTaskRed();
	        }
        }

        private void OnClickCloseBtn()
        {
	        UIManager.Instance.CloseDialog<MagicBingoView>();
        }

        #endregion
        
        #region 倒计时

        private float interval;
        private void RefreshTimeDesc()
        {
	        if (Data.IsExchangeTime)
	        {
		        OnChooseSubPassportBtn(MagicBingoShowType.Exchange);
		        foreach (var item in OpBtnScrollViewList)
		        {
			        if ((MagicBingoShowType)item.CurOpCellData.OpType != MagicBingoShowType.Exchange)
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