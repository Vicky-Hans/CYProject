using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ShopBoxRewardItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhImage bg;
		public DhImage icon;
		public ScrollRectExtend scrollView;
		[AssetPath]public string scrollViewCell;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.PrefabPath = scrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopBoxRewardItemView, ShopBoxRewardItemViewModel>();
            
			bindingSet.Bind(bg).For(v => v.sprite).To(vm => vm.BgPath).WithConversion(this);
			bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);

            bindingSet.Build();
        }
    }
}