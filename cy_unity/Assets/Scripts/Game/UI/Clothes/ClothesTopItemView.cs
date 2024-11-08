using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ClothesTopItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhImage icon;
		public CellItemBaseView cellItemBaseView;
        public GameObject upTips;
        public RectTransform rect; //提示框位置
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<ClothesTopItemView, ClothesTopItemViewModel>();
            
			bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
            bindingSet.Bind(cellItemBaseView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.CellItemBaseViewVm != null);
			bindingSet.Bind(cellItemBaseView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemBaseViewVm);
            bindingSet.Bind(upTips).For(v => v.activeSelf).To(vm => vm.IsUpTips);
            bindingSet.Bind(this).For(v => v.rect).To(vm => vm.RectNode).OneWayToSource();

            bindingSet.Build();
        }
    }
}