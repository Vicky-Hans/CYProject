using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class LimitCellView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public MagicDrawRewardCellView magicDrawRewardCell;
		public CellItemBaseView cellBaseView;
		public DhText nameText;
		public DhText ratioText;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<LimitCellView, LimitCellViewModel>();
            bindingSet.Bind(magicDrawRewardCell.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowCellItem);
			bindingSet.Bind(magicDrawRewardCell.BindingContext).For(v => v.DataContext).To(vm => vm.MagicDrawRewardCellVm);
			bindingSet.Bind(cellBaseView.gameObject).For(v => v.activeSelf).To(vm => !vm.IsShowCellItem);
			bindingSet.Bind(cellBaseView.BindingContext).For(v=>v.DataContext).To(vm=>vm.CellBaseViewVm);
			bindingSet.Bind(nameText).For(v => v.text).To(vm => vm.NameTextStr);
			bindingSet.Bind(ratioText).For(v => v.text).To(vm => vm.RatioTextStr);
            bindingSet.Build();
        }
    }
}