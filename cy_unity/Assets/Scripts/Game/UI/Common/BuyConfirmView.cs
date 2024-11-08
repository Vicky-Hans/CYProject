using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
namespace DH.Game.UIViews
{
    public partial class BuyConfirmView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhButton btnClose;
		// public DhText desc;
		public DhButton btnItem;
		public ItemPriceNodeView itemPriceNode;
		public DhButton btnMoney;
		public BtnPriceNode btnPriceNode;
		public CommonTopView topItemView;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<BuyConfirmView, BuyConfirmViewModel>();
            
			bindingSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnClickBtnCloseCommand);
			// bindingSet.Bind(desc).For(v => v.text).To(vm => vm.DescStr);
			bindingSet.Bind(btnItem).For(v => v.onClick).To(vm => vm.OnClickBtnItemCommand);
			bindingSet.Bind(itemPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.ItemPriceNodeVm);
			bindingSet.Bind(btnMoney).For(v => v.onClick).To(vm => vm.OnClickBtnMoneyCommand);
			bindingSet.Bind(btnPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceNodeVm);
			bindingSet.Bind(topItemView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopItemsVm);
            bindingSet.Build();
        }
    }
}