using System;
using DH.Asset;
using DH.HotService;
using DH.Log;
using DHFramework;
using UnityEngine;

namespace DH.Launch
{
    public partial class TaskManager
    {
        public enum DlgType
        {
            ConfirmCancel = 1,
            Confirm = 2,
        }

        public GameObject MessageBoxUIObj { get; private set; }
        
        private Action confirmCallback;
        private Action cancelCallback;

        public void OpenMessageBox(string title, string desc, Action yesCallback, Action noCallback = null,
            string yesBtnText = "", string noBtnText = "")
        {
            confirmCallback = yesCallback;
            cancelCallback = noCallback;
            
            InitMessageDlg();
            DlgType uiType = DlgType.Confirm;

            if (yesCallback != null && noCallback != null)
            {
                uiType = DlgType.ConfirmCancel;
            }
            
            MessageDlg.Instance.InitUI(uiType, title, desc, yesBtnText, noBtnText, OnButtonClick);
        }

        private void OnButtonClick(bool confirm)
        {
            if (confirm)
            {
                confirmCallback?.Invoke();
            }
            else
            {
                cancelCallback?.Invoke();
            }
            
            DestroyMessageDlg();
        }
        
        /// <summary>
        /// 显示启动界面，用于显示初始化的文字描述和进度条
        /// </summary>
        private void InitMessageDlg()
        {
            if (!MessageBoxUIObj)
            {
                var path = StartupEntry.Instance.StartupConfig.MessageBoxUIPath;
                MessageBoxUIObj = AssetsManager.InstantiateWithParentSync(path, StartupEntry.Instance.CanvasRootTrans, false);
                if (MessageBoxUIObj)
                {
                    RectTransform rect = MessageBoxUIObj.GetComponent<RectTransform>();
                    rect.anchoredPosition = Vector2.zero;
                }
            }
        }

        /// <summary>
        /// 删除启动界面
        /// </summary>
        private void DestroyMessageDlg()
        {
            if (MessageBoxUIObj)
            {
                AssetsManager.Release(MessageBoxUIObj);
                MessageBoxUIObj = null;
            }
        }
    }
}