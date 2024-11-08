using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
namespace DH.Game.UIViews
{
    public partial class CollegeShopView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public CommonTopView commonTopItems;
		public UICircularScrollView scrollView;
		[AssetPath]public string scrollViewCell;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.PrefabPath = scrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<CollegeShopView, CollegeShopViewModel>();
            
			bindingSet.Bind(commonTopItems.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopItemsVm);
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
			bindingSet.Bind(this).For(v => v.scrollView).To(vm => vm.CollectionView).OneWayToSource();

            bindingSet.Build();
        }
    }
}