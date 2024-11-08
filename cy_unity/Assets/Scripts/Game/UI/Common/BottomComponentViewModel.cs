using System;
using System.Collections.Generic;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class BottomComponentViewModel : ViewModelBase
    {
	    [AutoNotify] private ObservableList<BottomOpCellItemViewModel> opScrollViewList = new();
	    [AutoNotify] private bool isShowOpScrollView;
	    [AutoNotify] private bool isCanDragScrollView;
	    private readonly Action onClickCallback;
	    private readonly int maxOpCount = 4;
        [Preserve]
        public BottomComponentViewModel(Action closeCallback,ObservableList<BottomOpCellItemViewModel> opList =null)
        {
	        if (opList != null)
	        {

		        IsShowOpScrollView = opList.Count > 0;
		        OpScrollViewList = opList;
		        IsCanDragScrollView = OpScrollViewList.Count > maxOpCount;
	        }
	        else
	        {
		        IsShowOpScrollView = false;
		        IsCanDragScrollView = false;
	        }

	        onClickCallback = closeCallback;
	        

        }

        protected override void OnDispose()
        {
	        foreach (var item in opScrollViewList)
	        {
		        item.Dispose();
	        }
	        base.OnDispose();
        }


        [Command]
        private void OnClickCloseBtn()
        {
	        onClickCallback?.Invoke();
        }
    }
}