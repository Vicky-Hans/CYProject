using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using DH.UIFramework;

namespace DH.Game.ViewModels
{
    public struct PullDownItemInfo
    {
        public string Name;
        public string IconPath;
        public string BgPath;
    }

    public partial class PullDownItemViewModel : ViewModelBase
    {
        [AutoNotify] public int pos;
        [AutoNotify] public bool isSelect;
		[AutoNotify] private string titleNameStr;
        public Action<int> ClickSelectAction;
        [Preserve]
        public PullDownItemViewModel(int pos,PullDownItemInfo info,Action<int> clickSelect)
        {
            Pos = pos;
            TitleNameStr = info.Name;
            ClickSelectAction = clickSelect;
        }

        public void RefreshSelect(int selectPos)
        {
            IsSelect = selectPos == Pos;
        }

        [Command]
        private void OnClickSelect()
        {
            ClickSelectAction?.Invoke(pos);
        }
    }
}