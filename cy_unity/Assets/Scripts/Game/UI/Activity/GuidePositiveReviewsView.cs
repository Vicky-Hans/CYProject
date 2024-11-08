using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;

namespace DH.Game.UIViews
{
    public partial class GuidePositiveReviewsView : BaseView
    {
        public override bool FullScreen => false;
        public DhButton closeButton;
        public DhButton gotoAppraiseButton;
        
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<GuidePositiveReviewsView, GuidePositiveReviewsViewModel>();
            bindingSet.Bind(closeButton).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
            bindingSet.Bind(gotoAppraiseButton).For(v => v.onClick).To(vm => vm.OnClickGotoAppraiseBtnCommand);
            bindingSet.Build();
        }
    }
}
