using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class RechargePointView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhButton clsoeBtn;
		public UICircularScrollView infoScrollView;
		[AssetPath]public string infoScrollViewCell;
		public CommonTopView topView;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			infoScrollView.PrefabPath = infoScrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<RechargePointView, RechargePointViewModel>();
            
			bindingSet.Bind(clsoeBtn).For(v => v.onClick).To(vm => vm.OnClickClsoeBtnCommand);
			bindingSet.Bind(infoScrollView).For(v => v.Collection).To(vm => vm.InfoScrollViewList);
			bindingSet.Bind(topView.BindingContext).For(v => v.DataContext).To(vm => vm.TopVm);
            bindingSet.Build();
        }
    }
}