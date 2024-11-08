using System;
using System.ComponentModel;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using Game.UI.MainUi;
using UnityEngine.Scripting;


namespace DH.Game.ViewModels
{
    public partial class PassportsMainViewModel : ViewModelBase
    {
        [AutoNotify] private BottomComponentViewModel bottomComponentVm;
        [AutoNotify] private ObservableList<BottomOpCellItemViewModel> opBtnScrollviewList = new();
        [AutoNotify] private EPassPortType curPassPortType;
        [AutoNotify] private PassportsViewModel passportsVm;
        [AutoNotify] private ChapterFundViewModel chapterFundVm;
        
        [Preserve]
        public PassportsMainViewModel()
        {
	        PassportsVm = new();
	        PassportsVm.OpBtnScrollviewList = OpBtnScrollviewList;
	        chapterFundVm = new(); 
	        chapterFundVm.OpBtnScrollviewList = OpBtnScrollviewList;

	        InitBottomComponent();
	        
            DataCenter.chapterFundData.PropertyChanged += ChapterFundDataChange;
        }

        private void InitBottomComponent()
        {
	        OpBtnScrollviewList.Clear();
	        foreach (EPassPortType tabType in Enum.GetValues(typeof(EPassPortType)))
	        {
				BottomOpCellData tempData = new();
		        switch (tabType)
		        {
			        case EPassPortType.PassportTypeDiscount:
			        {
				        tempData.ChooseIconPath = "mainui[icon_home_5]";
				        tempData.ChooseBgPath = "common[common_panel_10]";
				        tempData.OpType = tabType;
				        tempData.OpName = LocalizeHelper.GetGlobal(GlobalLanguageId.Tehuijijin_09);;
				        tempData.OnClickCallback = OnChooseSubPassportBtn;
			        } break;
			        case EPassPortType.PassportTypeStone:
			        {
				        tempData.ChooseIconPath = "pass[pass_icon_3]";
				        tempData.ChooseBgPath = "common[common_panel_10]";
				        tempData.OpType = tabType;
				        tempData.OpName = LocalizeHelper.GetGlobal(GlobalLanguageId.Tehuijijin_10);
				        tempData.OnClickCallback = OnChooseSubPassportBtn;
				        
			        } break;
			        case EPassPortType.PassportTypeChapter:
			        {
				        tempData.ChooseIconPath = "pass[pass_icon_4]";
				        tempData.ChooseBgPath = "common[common_panel_10]";
				        tempData.OpType = tabType;
				        tempData.OpName = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips27);
				        tempData.OnClickCallback = OnChooseSubPassportBtn;
				        tempData.IsShowRedDot = DataCenter.chapterFundData.IsRed();
			        } break;
		        }

		        if (EPassPortType.PassportTypeChapter == tabType && (!MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionChapterFund) ||
			            DataCenter.chapterFundData.IsGetAwardOver()))
					continue;
		        BottomOpCellItemViewModel tempOpCell = new(tempData);
		        OpBtnScrollviewList.Add(tempOpCell);
	        }
            // 显示根据玩家对应的轮次来显示
            BottomComponentVm = new BottomComponentViewModel(OnClickCloseBtn,OpBtnScrollviewList);
            OnChooseSubPassportBtn(EPassPortType.PassportTypeDiscount);
            PassportsVm.UpdateOpBtnState();
        }

        private void ChapterFundDataChange(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName is nameof( DataCenter.chapterFundData.GetAwards))
	        {
		        if (DataCenter.chapterFundData.IsGetAwardOver())
		        {
			        InitBottomComponent();
		        }
	        }
        }

        private void OnChooseSubPassportBtn(Object type)
        {
	        foreach (var item in opBtnScrollviewList)
	        {
		        item.CurOpCellData.IsChoose = (int)item.CurOpCellData.OpType == (int)type;
	        }
	        CurPassPortType = (EPassPortType)type;
	        if (CurPassPortType != EPassPortType.PassportTypeChapter)
	        {
		        PassportsVm.CurPassPortType = (EPassPortType)type;
		        PassportsVm.UpdatePanel();
	        }
	        else
	        {
		        ChapterFundVm.ParseShowIndex();
	        }
        }
        
        private void OnClickCloseBtn()
		{
			UIManager.Instance.CloseDialog<PassportsMainView>();
		}
		protected override void OnDispose()
		{
			foreach (var item in opBtnScrollviewList)
			{
				item.Dispose();
			}
			DataCenter.chapterFundData.PropertyChanged -= ChapterFundDataChange;
			BottomComponentVm.Dispose();
			PassportsVm?.Dispose();
			ChapterFundVm?.Dispose();
			base.OnDispose();
		}
    }
}