using System;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class MagicDrawRewardCellView : BaseItemView, IViewKey
    {
        public override bool FullScreen => false;
        public RectTransform bgRect;
        public DhImage bg;
        public RectTransform iconRect;
        public DhImage icon;
        public DhText itemCount;
        public DhButton iconBtn;
		public GameObject limitNode;
        public GameObject effectNode1;
        public GameObject effectNode2;
        public int index;
        public object Key => index;
        private bool isGray;
        
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
	        await base.Create();
            var bindingSet = this.CreateBindingSet<MagicDrawRewardCellView, MagicDrawRewardCellViewModel>();
            // bindingSet.Bind(bgRect).For(v => v.sizeDelta).To(vm => vm.BgSize);
            bindingSet.Bind(bg).For(v => v.sprite).To(vm => vm.BgPath).WithConversion(this);
            bindingSet.Bind(this).For(v => v.IsGray).To(vm => vm.IsGray);
            bindingSet.Bind(bg.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowBg);
            // bindingSet.Bind(iconRect).For(v => v.sizeDelta).To(vm => vm.IconSize);
            bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
            bindingSet.Bind(icon.gameObject.transform).For(v => v.localPosition).To(vm => vm.IconPos);
			bindingSet.Bind(limitNode).For(v => v.activeSelf).To(vm => vm.IsShowLimitNode);
            bindingSet.Bind(iconBtn).For(v => v.onClick).To(vm => vm.OnClickIconBtnCommand).CommandParameter(() => new Tuple<Vector3, Vector3>(icon.transform.position, new Vector3(0, 20, 0)));
            bindingSet.Bind(itemCount.gameObject.transform).For(v => v.localPosition).To(vm => vm.ItemCountPos);
            bindingSet.Bind(itemCount).For(v => v.text).To(vm => vm.ItemCountStr);
            bindingSet.Bind(itemCount.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowBg);
            bindingSet.Bind(effectNode1).For(v=>v.activeSelf).To(vm=>vm.IsShowEffectNode1);
            bindingSet.Bind(effectNode2).For(v=>v.activeSelf).To(vm=>vm.IsShowEffectNode2);
            bindingSet.Build();
        }

        public bool IsGray
        {
            get=> isGray;
            set => UpdateItemColor(value);
        }

        private void UpdateItemColor(bool isGray)
        {
            bg.SetGrayActive(isGray,true,false);
        }
    }
}