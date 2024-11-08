using System;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using UnityEngine;
using UnityEngine.Scripting;
namespace DH.Game.ViewModels
{


    public class CostConfirmData
    {
        /// <summary>
        /// 左边按钮点击回调
        /// </summary>
        public Action LeftBtnCallback;
        /// <summary>
        /// 右边按钮点击回调
        /// </summary>
        public Action RightBtnCallback;
        
        public bool IsShowLeftBtnIcon;
        public string LeftBtnIconPath; 
        public string LeftBtnStr;
        public Color LeftBtnColor = UIHelper.HexColorStrToColor(DhHexColor.White);
        
        public bool IsShowRightBtnIcon;
        public string RightBtnIconPath; 
        public string RightBtnStr;
        public Color RightBtnColor  = UIHelper.HexColorStrToColor(DhHexColor.White);

        /// <summary>
        /// 是否展示带Icon的描述
        /// </summary>
        public bool IsShowDescWithIcon;
        /// <summary>
        /// 描述中 icon 路径
        /// </summary>
        public string CostIconImgPath;
        /// <summary>
        ///  描述中 对应icon的个数
        /// </summary>
        public string CostValueStr;

    }

    public partial class CostConfirmViewModel : ViewModelBase
    {

        private readonly CostConfirmData data;

        [AutoNotify] private string leftBtnIconPath;
        [AutoNotify] private bool isShowLeftBtnIcon;
        [AutoNotify] private string leftBtnStr;
        [AutoNotify] private Color leftBtnStrColor;
        [AutoNotify] private string rightBtnIconPath;
        [AutoNotify] private bool isShowRightBtnIcon;
        [AutoNotify] private string rightBtnStr;
        [AutoNotify] private Color rightBtnStrColor;
        [AutoNotify] private bool isShowDescWithIcon;
        [AutoNotify] private string costIconImgPath;
        [AutoNotify] private string costValueStr;
        
        
        [Preserve]
        public CostConfirmViewModel(CostConfirmData costData)
        {
            data = costData;
            UpdatePanel();
        }


        private void UpdatePanel()
        {
            if (data.IsShowLeftBtnIcon)
            {
                leftBtnIconPath = data.LeftBtnIconPath;
                IsShowLeftBtnIcon = true;
            }
            else
            {
                IsShowLeftBtnIcon = false;
            }
            LeftBtnStr = data.LeftBtnStr ?? LocalizeHelper.GetGlobal(GlobalLanguageId.setup05);
            LeftBtnStrColor = data.LeftBtnColor;
            if (data.IsShowRightBtnIcon)
            {
                rightBtnIconPath = data.RightBtnIconPath;
                IsShowRightBtnIcon = true;
            }
            else
            {
                IsShowRightBtnIcon = false;
            }
            RightBtnStr = data.RightBtnStr;
            RightBtnStrColor = data.RightBtnColor;

            if (data.IsShowDescWithIcon)
            {
                CostIconImgPath = data.CostIconImgPath;
                CostValueStr = data.CostValueStr;
            }
        }

        public void OnClickLeftBtn()
        {
            if (data.LeftBtnCallback != null)
            {
                data.LeftBtnCallback();
            }
            OnCloseBtn();
        }

        public void OnClickRightBtn()
        {
            if (data.RightBtnCallback != null)
            {
                data.RightBtnCallback();
            }
            OnCloseBtn();
        }

        public void OnCloseBtn()
        {
            UIManager.Instance.CloseDialog<CostConfirmView>();
        }
    }
}