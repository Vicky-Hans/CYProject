using System;
using DH.Game.UI;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class TabBtnTitleModel : ViewModelBase
    {
        [AutoNotify]
        private int pos;
        [AutoNotify]
        private int selectPos;
        [AutoNotify]
        private string name;
        [AutoNotify]
        private bool showRedDot;
        public Action<int> ClickEvent;
        public bool IsLock;
        public string LockDesc;
        [Preserve]
        public TabBtnTitleModel(TabBtnInfo info)
        {
            Pos = info.Pos;
            Name = info.Name;
            IsLock = info.IsLock;
            LockDesc = info.LockDesc;
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