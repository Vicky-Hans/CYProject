using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ShopDrawClothesInfoView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhButton btnClose;
		public UICircularScrollView scrollView;
		[AssetPath]public string scrollViewCell;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.PrefabPath = scrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopDrawClothesInfoView, ShopDrawClothesInfoViewModel>();
            
			bindingSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnClickBtnCloseCommand);
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);

            bindingSet.Build();
        }
    }
}