using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ClothesMergeItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public CellItemBaseView cellItemBaseView;
		public DhImage useIng;
		public DhImage select;
		public DhImage lockNode;
		public RectTransform rect; //提示框位置
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<ClothesMergeItemView, ClothesMergeItemViewModel>();
            
			bindingSet.Bind(cellItemBaseView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemBaseViewVm);
			bindingSet.Bind(useIng.gameObject).For(v => v.activeSelf).To(vm => vm.UseIngState);
			bindingSet.Bind(select.gameObject).For(v => v.activeSelf).To(vm => vm.SelectState);
			bindingSet.Bind(lockNode.gameObject).For(v => v.activeSelf).To(vm => vm.LockState);
			bindingSet.Bind(this).For(v => v.rect).To(vm => vm.rect).OneWayToSource();

            bindingSet.Build();
        }
    }
}