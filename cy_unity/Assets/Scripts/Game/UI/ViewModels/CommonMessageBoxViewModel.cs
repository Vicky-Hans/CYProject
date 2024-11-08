using System;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using UnityEngine;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class CommonMessageBoxViewModel : ViewModelBase
    {
        [AutoNotify] private Vector2 bgSize = new Vector2(950, 840);
        private Action closeCallback;
        private Action cancelCallback;
        private Action confirmCallback;
        private Func<UniTaskVoid> asyncCloseCallback;
        private Func<UniTaskVoid> asyncCancelCallback;
        private Func<UniTaskVoid> asyncConfirmCallback;
        private bool enableCloseBtn;
        private string msgTitle;
        public string MsgTitle
        {
            get => msgTitle;
            set => Set(ref msgTitle, value);
        }
        
        private string msgContent;
        public string MsgContent
        {
            get => msgContent;
            set => Set(ref msgContent, value);
        }
        
        private string cancelTxt;
        public string CancelTxt
        {
            get => cancelTxt;
            set => Set(ref cancelTxt, value);
        }
        
        private bool hasCancelBtn;
        public bool HasCancelBtn
        {
            get => hasCancelBtn;
            set => Set(ref hasCancelBtn, value);
        }

        private string confirmTxt;
        public string ConfirmTxt
        {
            get => confirmTxt;
            set => Set(ref confirmTxt, value);
        }
        
        private bool hasConfirmBtn;
        public bool HasConfirmBtn
        {
            get => hasConfirmBtn;
            set => Set(ref hasConfirmBtn, value);
        }

        [Preserve]
        public CommonMessageBoxViewModel()
        {
            
        }
        
        public CommonMessageBoxViewModel(string title, string content, string cancelTxt, string confirmTxt, Action cancelAction, Action confirmAction, Action closeAction)
        {
            MsgTitle = title;
            MsgContent = content;
            HasCancelBtn = cancelAction != null;
            HasConfirmBtn = confirmAction != null;
            CancelTxt = cancelTxt;
            ConfirmTxt = confirmTxt;

            closeCallback = closeAction;
            cancelCallback = cancelAction;
            confirmCallback = confirmAction;

            enableCloseBtn = closeAction != null;
        }
        
        public CommonMessageBoxViewModel(string title, string content, string cancelTxt, string confirmTxt, Func<UniTaskVoid> cancelAction, Func<UniTaskVoid> confirmAction, Func<UniTaskVoid> closeAction)
        {
            MsgTitle = title;
            MsgContent = content;
            HasCancelBtn = cancelAction != null;
            HasConfirmBtn = confirmAction != null;
            CancelTxt = cancelTxt;
            ConfirmTxt = confirmTxt;

            asyncCloseCallback = closeAction;
            asyncCancelCallback = cancelAction;
            asyncConfirmCallback = confirmAction;

            enableCloseBtn = closeAction != null;
        }

        public static CommonMessageBoxViewModel CreateConfirmButtonOnly(string content, Action confirmAction)
        {
            string title = LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_Title);
            string confirmTxt = LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_ConfirmTxt);
            var messageBox = new CommonMessageBoxViewModel(title, content, "", confirmTxt, null,
                confirmAction, null);
            return messageBox;
        }
        
        public static CommonMessageBoxViewModel CreateConfirmButtonOnly(string content, Func<UniTaskVoid> confirmAction)
        {
            string title = LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_Title);
            string confirmTxt = LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_ConfirmTxt);
            var messageBox = new CommonMessageBoxViewModel(title, content, "", confirmTxt, null,
                confirmAction, null);
            return messageBox;
        }
        
        public static CommonMessageBoxViewModel CreateCommonMsgBox(string content, Action confirmAction,Action cancelAction)
        {
            string title = LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_Title);
            string confirmTxt = LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_ConfirmTxt);
            string cancelTxt = LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_CancelTxt);
            var messageBox = new CommonMessageBoxViewModel(title, content, cancelTxt, confirmTxt, cancelAction,
                confirmAction, null);
            return messageBox;
        }

        public void HandleCancel()
        {
            UIManager.Instance.CloseDialog<CommonMessageBox>();
            cancelCallback?.Invoke();
            asyncCancelCallback?.Invoke().Forget();
        }

        public void HandleConfirm()
        {
            UIManager.Instance.CloseDialog<CommonMessageBox>();
            confirmCallback?.Invoke();
            asyncConfirmCallback?.Invoke().Forget();
        }

        public void HandleClose()
        {
            if (!enableCloseBtn)
            {
                return;
            }
            
            UIManager.Instance.CloseDialog<CommonMessageBox>();
            closeCallback?.Invoke();
            asyncCloseCallback?.Invoke().Forget();
        }
        
    }
}