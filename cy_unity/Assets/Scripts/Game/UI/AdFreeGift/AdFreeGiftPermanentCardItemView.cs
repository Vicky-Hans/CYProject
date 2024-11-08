using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class AdFreeGiftPermanentCardItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public ScrollRectExtend scrollViewLock;
		[AssetPath]public string scrollViewLockCell;
		public DhButton buyButton;
		public BtnPriceNode priceNode;
		private bool showSelf;

		public bool ShowSelf
		{
			get => showSelf;
			set
			{
				showSelf = value;
				gameObject.SetActive(showSelf);
			}
		}
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollViewLock.PrefabPath = scrollViewLockCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<AdFreeGiftPermanentCardItemView, AdFreeGiftPermanentCardItemViewModel>();
            bindingSet.Bind(scrollViewLock).For(v => v.Collection).To(vm => vm.ScrollViewLockList);
			bindingSet.Bind(buyButton).For(v => v.onClick).To(vm => vm.OnClickBuyButtonCommand);
			bindingSet.Bind(priceNode.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceNodeVm);
			bindingSet.Bind(this).For(v => v.ShowSelf).To(vm => vm.IsShowSelf);
            bindingSet.Build();
        }
    }
}