using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
namespace DH.Game.UIViews
{
    public partial class PassportsBuyConfirmView : BaseView
    {
        public override bool FullScreen => false;
		public DhButton confirmBtn;
		public ItemPriceNodeView btnPriceNode;
		public DhButton closeBtn;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<PassportsBuyConfirmView, PassportsBuyConfirmViewModel>();
			bindingSet.Bind(confirmBtn).For(v => v.onClick).To(vm => vm.OnClickConfirmBtnCommand);
			bindingSet.Bind(btnPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceNodeVm);
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
            bindingSet.Build();
        }
    }
}