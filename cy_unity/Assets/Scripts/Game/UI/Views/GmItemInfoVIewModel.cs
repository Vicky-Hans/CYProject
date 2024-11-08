using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using DH.UIFramework;


namespace DH.Game.ViewModels
{
    public partial class GmItemInfoVIewModel : ViewModelBase
    {
        [AutoNotify] private int id;
		[AutoNotify] private string textStr;

        private Action<int> SelectAction;

        [Preserve]
        public GmItemInfoVIewModel(int id, string desc, Action<int> click)
        {
            Id = id;
            textStr = desc;
            SelectAction = click;
        }

        [Command]
        private void OnClick()
        {
            SelectAction?.Invoke(Id);
        }

    }
}