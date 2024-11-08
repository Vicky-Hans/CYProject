using Cysharp.Threading.Tasks;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
namespace DH.Game.UIViews
{
    public partial class MagicFundView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhButton closeBtn;
		public UICircularScrollView scrollView;
		[AssetPath]public string scrollViewCell;
		public DhButton opBtn;
		public DhText opBtnText;
		public BtnPriceNode opBtnPrice;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
			scrollView.PrefabPath = scrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<MagicFundView, MagicFundViewModel>();
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
			bindingSet.Bind(scrollView).For(v => v.DefaultJumpIndex).To(vm => vm.ShowIndex);
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
			bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
			bindingSet.Bind(opBtn).For(v => v.interactable).To(vm => vm.IsCanClickOpBtn);
			bindingSet.Bind(opBtnText).For(v => v.text).To(vm => vm.OpBtnTextStr);
			bindingSet.Bind(opBtnText.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowOpBtnText);
			bindingSet.Bind(opBtnPrice.BindingContext).For(v => v.DataContext).To(vm => vm.OpBtnPriceVm);
			bindingSet.Bind(opBtnPrice.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowCostNode);
			bindingSet.Bind(this).For(v => v.ShowIndex).To(vm => vm.ShowIndex);

            bindingSet.Build();
        }
        
        private int showIndex;
        public int ShowIndex
        {
        	get => showIndex;
        	set => DelayJumpIndex(value).Forget();
        }

        private async UniTaskVoid DelayJumpIndex(int index)
        {
        	await UniTask.Delay(100);
	        scrollView.Jump2SpecificItem(index);
        }
    }
}