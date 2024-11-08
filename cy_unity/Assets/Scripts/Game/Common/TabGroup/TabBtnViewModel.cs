using System;
using DH.Game.UI;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using UnityEngine;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class TabBtnViewModel : ViewModelBase
    {
        [AutoNotify]
        private int pos;
        [AutoNotify]
        private int selectPos;
        [AutoNotify]
        private string name;

        [AutoNotify]
        private string onIconPath;
        [AutoNotify]
        private string offIconPath;

        [AutoNotify] private Vector2 iconSize;
        
        [AutoNotify] private bool showRedDot;

        public Action<int> ClickEvent;

        public bool IsLock;
        public string LockDesc;
        [Preserve]
        public TabBtnViewModel(TabBtnInfo info)
        {
            Pos = info.Pos;
            Name = info.Name;
            OnIconPath = string.IsNullOrEmpty(info.OnPath)?UIHelper.NoneImagePath():info.OnPath;
            OffIconPath = string.IsNullOrEmpty(info.OffPath)?UIHelper.NoneImagePath():info.OffPath;
            iconSize = new Vector2(125, 125);
            IsLock = info.IsLock;
            LockDesc = info.LockDesc;
        }

        public void SetIconSize(Vector2 size)
        {
            IconSize = size;
        }


        [Command]
        private void OnClick()
        {
            if (IsLock)
            {
                ToastManager.Show(LockDesc);
                return;
            }
            ClickEvent?.Invoke(Pos);
        }
    }
}