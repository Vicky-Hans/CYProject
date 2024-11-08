using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class RankPlayerInfoWeaponItemView : BaseItemView
    {
        public override bool FullScreen => false;
        public RectTransform rectTransform;
        public Transform trans;
		public DhImage icon;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<RankPlayerInfoWeaponItemView,RankPlayerInfoWeaponItemViewModel>();
            bindingSet.Bind(rectTransform).For(v => v.anchoredPosition).To(vm => vm.CellPos);
            // bindingSet.Bind(trans).For(v => v.localPosition).To(vm => vm.LocalPos);
            bindingSet.Bind(this).For(v=>v.icon).To(vm=>vm.IconImg).OneWayToSource();
            bindingSet.Bind(icon.transform).For(v => v.localScale).To(vm => vm.IconScale);
            bindingSet.Build();
        }
    }
}