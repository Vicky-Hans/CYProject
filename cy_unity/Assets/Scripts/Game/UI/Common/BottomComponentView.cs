using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class BottomComponentView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhButton closeBtn;
		public UICircularScrollView opScrollView;
		[AssetPath]public string opScrollViewCell;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
			opScrollView.PrefabPath = opScrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<BottomComponentView, BottomComponentViewModel>();
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
			bindingSet.Bind(opScrollView).For(v => v.Collection).To(vm => vm.OpScrollViewList);
			bindingSet.Bind(opScrollView.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowOpScrollView);
			bindingSet.Bind(opScrollView).For(v => v.m_CanDragScrollView).To(vm => vm.IsCanDragScrollView);
			
            bindingSet.Build();
        }
    }
}