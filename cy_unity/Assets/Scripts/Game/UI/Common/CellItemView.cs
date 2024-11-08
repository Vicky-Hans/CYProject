using System;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
namespace DH.Game.UIViews
{
    public partial class CellItemView : BaseItemView,IViewKey
    {
        public override bool FullScreen => false;
        public RectTransform itemRf;
		public CellItemBaseView cellItemBaseView;
		public GameObject effect;
		public GameObject get;
		public GameObject lockNode;
		public GameObject getIconNode;
		public GameObject selectImg;

		public ParticleSystem advancedEffect;

		public int index;
		public object Key => index;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<CellItemView, CellItemViewModel>();
			bindingSet.Bind(cellItemBaseView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemBaseViewVm);
			bindingSet.Bind(itemRf).For(v => v.sizeDelta).To(vm => vm.CellItemBaseViewVm.SizeBg);
			bindingSet.Bind(effect.transform).For(v => v.localScale).ToExpression(vm => GetSize(vm.CellItemBaseViewVm.SizeBg));
			bindingSet.Bind(effect).For(v => v.activeSelf).ToExpression(vm => vm.State == ECellItemState.GetIng);
			bindingSet.Bind(get).For(v => v.activeSelf).ToExpression(vm => vm.State == ECellItemState.Finish);
			bindingSet.Bind(lockNode).For(v => v.activeSelf).To(vm => vm.IsShowLock);
			bindingSet.Bind(lockNode.transform).For(v => v.localScale).ToExpression(vm => GetSize(vm.CellItemBaseViewVm.SizeBg));
			bindingSet.Bind(getIconNode.transform).For(v => v.localScale).ToExpression(vm => GetSize(vm.CellItemBaseViewVm.SizeBg));
			bindingSet.Bind(selectImg).For(v => v.activeSelf).ToExpression(vm => vm.State == ECellItemState.Select);
			bindingSet.Bind(selectImg.transform).For(v => v.localScale).ToExpression(vm => GetSize(vm.CellItemBaseViewVm.SizeBg));
			bindingSet.Bind(this).For(v => v.advancedEffect).To(vm =>vm.EffectParticleSystem).OneWayToSource();
            bindingSet.Build();
        }

        private Vector3 GetSize(Vector2 sizeBg)
        {
	        return new Vector3(sizeBg.x / 166, sizeBg.y / 166, 1);
        }
    }
}