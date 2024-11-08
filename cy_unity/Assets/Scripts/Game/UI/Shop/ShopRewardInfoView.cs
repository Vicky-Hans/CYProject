using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ShopRewardInfoView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhButton btnClose;
		public DhText name;
		public DhText descValue;
		public UICircularScrollView scrollView;
		[AssetPath]public string scrollViewCell;
		public DhButton btnOk;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.PrefabPath = scrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopRewardInfoView, ShopRewardInfoViewModel>();
            
			bindingSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnClickBtnCloseCommand);
			bindingSet.Bind(name).For(v => v.text).To(vm => vm.NameStr);
			bindingSet.Bind(descValue).For(v => v.text).To(vm => vm.DescValueStr);
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
			bindingSet.Bind(btnOk).For(v => v.onClick).To(vm => vm.OnClickBtnOkCommand);

            bindingSet.Build();
        }
    }
}