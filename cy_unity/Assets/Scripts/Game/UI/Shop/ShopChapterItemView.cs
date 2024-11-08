using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ShopChapterItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhText name;
		public DhText tipsDesc;
		public ScrollRectExtend scrollViewItem;
		[AssetPath]public string scrollViewItemCell;
		public DhButton btnBuy;
		public BtnPriceNode btnPriceNode;
		public DhText oldPrice;
		public DhText discountValue;
		public DhButton btnUnLock;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollViewItem.PrefabPath = scrollViewItemCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopChapterItemView, ShopChapterItemViewModel>();
            
			bindingSet.Bind(name).For(v => v.text).To(vm => vm.NameStr);
			bindingSet.Bind(scrollViewItem).For(v => v.Collection).To(vm => vm.ScrollViewItemList);
			bindingSet.Bind(btnBuy).For(v => v.onClick).To(vm => vm.OnClickBtnBuyCommand);
			bindingSet.Bind(btnPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceNodeVm);
			bindingSet.Bind(discountValue).For(v => v.text).To(vm => vm.DiscountValueStr);
			bindingSet.Bind(btnUnLock.gameObject).For(v => v.activeSelf).To(vm => vm.IsLock);
			bindingSet.Bind(btnBuy.gameObject).For(v => v.activeSelf).ToExpression(vm =>!vm.IsLock);
            bindingSet.Build();
        }
    }
}