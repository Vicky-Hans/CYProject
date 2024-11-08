using System;
using System.ComponentModel;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using Game.UI.MainUi;
using TMPro;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class SubTitleBtnCellViewModel : ViewModelBase
    {

	    [AutoNotify] private bool isChoose;
		[AutoNotify] private string titleTextStr;
		private readonly ERankPageType curPageType;
		private readonly Action<ERankPageType> onClickSubBtnCallback;
		[AutoNotify] private Color color1;
		[AutoNotify] private Color color2;
		[AutoNotify] private TMP_ColorGradient titleTextColor;
	
        [Preserve]
        public SubTitleBtnCellViewModel(ERankPageType pageType, Action<ERankPageType> callback)
        {
	        curPageType = pageType;
	        onClickSubBtnCallback = callback;
	        InitBtnText();
	        MainUiManager.Instance.PropertyChanged += ProperChange;
        }

        private void InitBtnText()
        {
	        switch (curPageType)
	        {
		        case ERankPageType.MainStageRankTypeGlobal:
			        if (MainUiManager.Instance.CurRankType == ERankType.RankItemMainStage)
			        {
				        TitleTextStr = LocalizeHelper.GetGlobal(MainUiManager.Instance.IsNewcomer()
					        ? GlobalLanguageId.FunctionTips_02
					        : GlobalLanguageId.General_tips09);
			        }
			        else
			        {
				        TitleTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips09);
			        }
			        break;
		        case ERankPageType.MainStageRankTypeLocal:
		        {
			        if (DataCenter.charcaterData.AreaName.Equals("unknown"))
			        {
				        TitleTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips14);
			        }
			        else
			        {
				        TitleTextStr = DataCenter.charcaterData.AreaName;
			        }
		        } break;
	        }
        }

        public void ProperChange(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName is nameof(MainUiManager.Instance.CurRankType))
	        {
		        InitBtnText();
	        }
        }
        
        [Command]
        private void OnClickSubTitleBtn()
        {
	        onClickSubBtnCallback.Invoke(curPageType);
        }

        public void UpdatePanel(ERankPageType pageType)
        {
	        IsChoose = pageType == curPageType;
	        
        }
		
        protected override void OnDispose()
        {
	        MainUiManager.Instance.PropertyChanged -= ProperChange;
	        base.OnDispose();
        }

    }
}