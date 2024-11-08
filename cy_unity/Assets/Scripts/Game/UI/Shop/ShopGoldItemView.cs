using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ShopGoldItemView : BaseItemView
    {
        public override bool FullScreen => false;
        public DhImage bg;
		public DhImage icon;
		public DhImage priceBg;
		public DhText cntValue;
		public DhButton btnAd;
		public DhButton btnFree;
		public DhButton btnUseItem;
		public ItemPriceNodeView itemPriceNodeView;
		public CommonAdvIconView commonAdv;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopGoldItemView, ShopGoldItemViewModel>();
            
            bindingSet.Bind(bg).For(v => v.sprite).To(vm => vm.BgPath).WithConversion(this);
			bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
			bindingSet.Bind(priceBg).For(v => v.sprite).To(vm => vm.PriceBgPath).WithConversion(this);
			bindingSet.Bind(cntValue).For(v => v.text).To(vm => vm.CntValueStr);
			bindingSet.Bind(itemPriceNodeView.BindingContext).For(v => v.DataContext).To(vm => vm.ItemPriceNodeModel);
			bindingSet.Bind(btnAd).For(v => v.onClick).To(vm => vm.OnClickBtnAdCommand);
			bindingSet.Bind(btnFree).For(v => v.onClick).To(vm => vm.OnClickBtnFreeCommand);
			bindingSet.Bind(btnUseItem).For(v => v.onClick).To(vm => vm.OnClickBtnUseItemCommand);
			bindingSet.Bind(cntValue).For(v => v.color).To(vm => vm.PriceColor);
			bindingSet.Bind(btnAd.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShopBuyState==ShopBuyState.Adv);
			bindingSet.Bind(btnFree.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShopBuyState==ShopBuyState.Free);
			bindingSet.Bind(btnUseItem.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShopBuyState==ShopBuyState.Item);
			bindingSet.Bind(commonAdv.BindingContext).For(v => v.DataContext).To(vm => vm.CommonAdvVm);
            bindingSet.Build();
        }
    }
}