using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ShopBoxInfoView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhButton btnClose;
		public DhText name;
		public DhImage icon;
		public UICircularScrollView scrollView;
		[AssetPath]public string scrollViewCell;
		public DhButton btnAd;
		public DhButton btnUseItem;
		public ItemPriceNodeView itemPriceNodeView;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.PrefabPath = scrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopBoxInfoView, ShopBoxInfoViewModel>();
            
			bindingSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnClickBtnCloseCommand);
			bindingSet.Bind(name).For(v => v.text).To(vm => vm.NameStr);
			bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
			bindingSet.Bind(btnAd).For(v => v.onClick).To(vm => vm.OnClickBtnAdCommand);
			bindingSet.Bind(btnUseItem).For(v => v.onClick).To(vm => vm.OnClickBtnUseItemCommand);
			bindingSet.Bind(itemPriceNodeView.BindingContext).For(v => v.DataContext).To(vm => vm.ItemPriceNodeModel);
			bindingSet.Bind(btnAd.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShopBuyState == ShopBuyState.Adv || vm.ShopBuyState == ShopBuyState.Free);
            bindingSet.Build();
        }
    }
}