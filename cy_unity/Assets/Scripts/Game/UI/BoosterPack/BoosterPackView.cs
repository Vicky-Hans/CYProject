using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class BoosterPackView : BaseView
    {
        public override bool FullScreen => false;
		public DhText timesDes;
		public UICircularScrollView scrollView;
		[AssetPath]public string scrollViewCell;
		public DhButton closeBtn;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.PrefabPath = scrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<BoosterPackView, BoosterPackViewModel>();
			bindingSet.Bind(timesDes).For(v => v.text).To(vm => vm.TimesDesStr);
			bindingSet.Bind(this).For(v => v.ScrollPos).ToExpression(vm => vm.ScrollViewPos);
			bindingSet.Bind(scrollView).For(v => v.DefaultJumpIndex).To(vm => vm.ScrollViewPos);
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.BoosterPackItemList);
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);

            bindingSet.Build();
        }
        public int ScrollPos
        {
	        get => 0;
	        set => DelayScrollToPos(value);
        }
        private  void DelayScrollToPos(int pos)
        {
	        if(scrollView ==null) return;
	        scrollView.Jump2SpecificItem(pos);
        }
    }
}