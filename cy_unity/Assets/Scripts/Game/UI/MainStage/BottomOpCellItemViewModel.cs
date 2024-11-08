using System;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
	public partial class BottomOpCellData:ObservableObject
    {
        /// <summary>
        /// 传中的资源图 必传
        /// </summary>
        [AutoNotify] private string chooseIconPath;
        /// <summary>
        /// 未选中的icon图 不传 默认使用 chooseIconPath
        /// </summary>
        [AutoNotify] private string unChooseIconPath;
        /// <summary>
        /// 选中的背景图 必传
        /// </summary>
        [AutoNotify] private string chooseBgPath;
        /// <summary>
        /// 未选中的背景图 不传，默认用chooseBgPath
        /// </summary>
        [AutoNotify] private string unChooseBgPath;
        /// <summary>
        /// 按钮类型，根据需要自己转换类型
        /// </summary>
        [AutoNotify] private Object opType;
        /// <summary>
        /// 名字 如果是 string.Empty 代表没有名字 不显示
        /// </summary>
        [AutoNotify] private string opName;
        /// <summary>
        /// 是否显示锁 默认不显示
        /// </summary>
        [AutoNotify] private bool isShowLock;
        /// <summary>
        /// 是否显示红点 默认不显示
        /// </summary>
        [AutoNotify] private bool isShowRedDot;
        /// <summary>
        /// 点击的回调 回传 参数是 opType
        /// </summary>
        [AutoNotify] private Action<object> onClickCallback;
        /// <summary>
        /// 当前是否选中
        /// </summary>
        [AutoNotify] private bool isChoose;
		/// <summary>
		/// 当前是否可点击 默认true
		/// </summary>
        [AutoNotify] private bool isCanClick = true;

        public BottomOpCellData()
        {
        }

        public BottomOpCellData(string chooseIconImg, string chooseBgImg, object opType, Action<Object> callback, string nameStr)
        {
            ChooseIconPath = chooseIconImg;
            ChooseBgPath = chooseBgImg;
            OpType = opType;
            OnClickCallback = callback;
            OpName = nameStr;
            IsChoose = false;
        }
        public BottomOpCellData(string chooseIconImg, string unChooseIconImg, string chooseBgImg,string unChooseBgImg, object opType, Action<Object> callback, string nameStr)
        {

	        ChooseIconPath = chooseIconImg;
			UnChooseIconPath = unChooseIconImg;
			ChooseBgPath = chooseBgImg;
			UnChooseBgPath = unChooseBgImg;
			OpType = opType;
			OnClickCallback = callback;
			OpName = nameStr;      
        }
    }
	
    public partial class BottomOpCellItemViewModel : ViewModelBase
    {
		[AutoNotify] private BottomOpCellData curOpCellData;
		[AutoNotify] bool isGrayIcon;
		public bool isUseChoose = true;
        [Preserve]
        public BottomOpCellItemViewModel(BottomOpCellData data)
        {
	        curOpCellData = data;
        }

        [Command]
        private void OnClickOpBtn()
        {
	        curOpCellData.OnClickCallback?.Invoke(curOpCellData.OpType);
	        if (isUseChoose)
	        {
		        curOpCellData.IsChoose = true;
	        }

        }

        [Command]
        private void OnClickLockBtn()
        {
	        curOpCellData.OnClickCallback?.Invoke(curOpCellData.OpType);
        }
    }
}