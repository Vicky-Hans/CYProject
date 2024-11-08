using System;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class SubTitleCellViewModel : ViewModelBase
    {

	    [AutoNotify] private string subTitleBtnImgPath;
		[AutoNotify] private bool isShowLockImg;
		[AutoNotify] private string subTitleTextStr;
		[AutoNotify] private Color subTitleTextColor;
		[AutoNotify] private bool isChoose;
		private ESubTitleBtnState curBtnState = ESubTitleBtnState.Normal;

		public ESubTitleBtnState CurBtnState
		{
			get=> curBtnState;
			set
			{
				Set(ref curBtnState, value);
				UpdatePanel();
			}
		}
		private Action clickSubTitleBtnCallback;
		private Action lockCallback;
        [Preserve]
        public SubTitleCellViewModel(string nameStr, Action callback, Action lockCallback = null)
        {
	        clickSubTitleBtnCallback = callback;
	        SubTitleTextStr = nameStr;
	        InitPanel();
        }

        private void InitPanel()
        {
	        curBtnState = ESubTitleBtnState.Normal;
	        
        }

        private void UpdatePanel()
        {
	        switch (CurBtnState)
	        {
		        case ESubTitleBtnState.Lock:
		        {
			        IsShowLockImg = true;
			        SubTitleTextColor = Color.gray;
			        SubTitleBtnImgPath = "common[common_button_grey]";
		        } break;
		        case ESubTitleBtnState.Normal:
		        {
			        IsShowLockImg = false;
			        SubTitleTextColor = Color.white;
			        SubTitleBtnImgPath = "common[commom_button_blue]";
		        } break;
		        case ESubTitleBtnState.Choose:
		        {
			        IsShowLockImg = false;
			        SubTitleTextColor = Color.cyan;
			        SubTitleBtnImgPath = "common[commom_button_blue]";
		        } break;
	        }
        }

        [Command]
        private void OnClickSubTitleBtn()
        {
	        if (ESubTitleBtnState.Lock == CurBtnState)
	        {
		        lockCallback?.Invoke();
		        return;
	        } 
	        clickSubTitleBtnCallback?.Invoke();
        }
    }
}