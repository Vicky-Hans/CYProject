using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
namespace DH.Game.UIViews
{
    public partial class LuckDrawCellView : BaseItemView,IViewKey
    {
        public override bool FullScreen => false;
		public GameObject chooseNode;
		public CellItemBaseView cellItemBaseView;
        public int index;
        public object Key => index;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<LuckDrawCellView, LuckDrawCellViewModel>();
			bindingSet.Bind(chooseNode).For(v => v.activeSelf).To(vm => vm.IsShowChooseNode);
			bindingSet.Bind(cellItemBaseView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemBaseViewVm);
            bindingSet.Build();
        }
    }
}