using System;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class TabBtnItemView : BaseItemView
    {
        public override bool FullScreen => true;

        public DhButton opBtn;
		public DhImage icon;
		public DhText opBtnText;
        public GameObject lockNode;
        public GameObject redDotNode;
        public GameObject tipsNode;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<TabBtnItemView, TabBtnItemViewModel>();
            bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OpBtnClick).CommandParameter(() => new Tuple<Vector3, Vector3>(icon.transform.position, Vector3.zero));
            bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
            bindingSet.Bind(icon.gameObject.transform).For(v => v.localScale).To(vm => vm.IconScale);
            bindingSet.Bind(this).For(v => v.icon).To(vm => vm.CellIcon).OneWayToSource();
            bindingSet.Bind(opBtnText.gameObject).For(v=>v.activeSelf).To(vm=>vm.IsShowOpBtnText);
			bindingSet.Bind(opBtnText).For(v => v.text).To(vm => vm.OpBtnTextStr);
            bindingSet.Bind(lockNode).For(v => v.activeSelf).To(vm => vm.IsLocked);
            bindingSet.Bind(redDotNode).For(v => v.activeSelf).To(vm => vm.IsShowRedDot);
            bindingSet.Bind(tipsNode).For(v => v.activeSelf).To(vm => vm.IsShowTips);
            bindingSet.Build();
        }
    }
}