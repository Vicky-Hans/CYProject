using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
namespace DH.Game.UIViews
{
    public partial class FreeBuyActivityView : BaseView
    {
        public override bool FullScreen => false;
        
		public UICircularScrollView scrollView;
		[AssetPath]public string scrollViewCell;
		public DhButton btnAd;
		public BtnPriceNode btnPriceNode;
		public DhText btnTips;
		public CommonTopView commonTopView;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.PrefabPath = scrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<FreeBuyActivityView, FreeBuyActivityViewModel>();
            
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
			bindingSet.Bind(scrollView).For(v => v.DefaultJumpIndex).To(vm => vm.StartPos);
			bindingSet.Bind(btnAd).For(v => v.onClick).To(vm => vm.OnClickBtnAdCommand);
			bindingSet.Bind(btnPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceNodeVm);
			bindingSet.Bind(btnAd.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsBuy);
			bindingSet.Bind(btnTips.gameObject).For(v => v.activeSelf).To(vm => vm.IsBuy);
			bindingSet.Bind(commonTopView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopViewModel);
            bindingSet.Build();
        }
    }
}