using System;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class BottomOpCellItemView : BaseItemView
    {
        public override bool FullScreen => false;
		public DhButton opBtn;
		public DhImage icon;
		public DhText opText;
        public GameObject redDotNode;
        public GameObject lockNode;

        private bool  isGrayIcon;

        public bool IsGrayIcon
        {
            get => isGrayIcon;
            set
            {
                isGrayIcon = value;
                UIHelper.SetGray(icon.gameObject,isGrayIcon);
            }
        }

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<BottomOpCellItemView, BottomOpCellItemViewModel>();
            bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
            bindingSet.Bind(opBtn.image).For(v => v.enabled).To(vm => vm.CurOpCellData.IsChoose);
            bindingSet.Bind(opBtn).For(v => v.interactable).ToExpression(vm => !vm.CurOpCellData.IsChoose);
            bindingSet.Bind(opBtn.image).For(v => v.sprite).ToExpression(vm => GetOpBgPath(vm.CurOpCellData, vm.CurOpCellData.IsChoose)).WithConversion(this);
            
            bindingSet.Bind(icon).For(v => v.sprite).ToExpression(vm => GetOpIconPath(vm.CurOpCellData, vm.CurOpCellData.IsChoose)).WithConversion(this);
            bindingSet.Bind(opText).For(v => v.text).To(vm => vm.CurOpCellData.OpName);
            bindingSet.Bind(opText.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.CurOpCellData.OpName != String.Empty);
            bindingSet.Bind(redDotNode).For(v => v.activeSelf).To(vm => vm.CurOpCellData.IsShowRedDot);
            bindingSet.Bind(lockNode).For(v => v.activeSelf).To(vm => vm.CurOpCellData.IsShowLock);
            bindingSet.Bind(this).For(v => v.IsGrayIcon).ToExpression(vm => vm.IsGrayIcon);
            bindingSet.Build();
        }

        private string GetOpIconPath(BottomOpCellData curInfo, bool isChoose)
        {
            if (isChoose)
                return curInfo.ChooseIconPath;
            if (curInfo.UnChooseIconPath == null)
                return curInfo.ChooseIconPath;
            return curInfo.UnChooseIconPath;
        }

        private string GetOpBgPath(BottomOpCellData curInfo, bool isChoose)
        {
            if (isChoose)
                return curInfo.ChooseBgPath;
            if (curInfo.UnChooseBgPath == null)
                return curInfo.ChooseBgPath;
            return curInfo.UnChooseBgPath;
        }
    }
}