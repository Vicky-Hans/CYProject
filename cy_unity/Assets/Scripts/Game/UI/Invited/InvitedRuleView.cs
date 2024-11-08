using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine.UI;
using Extend;
namespace DH.Game.UIViews
{
    public partial class InvitedRuleView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhButton closeBtn;
		public UICircularScrollView scrollView;
        [AssetPath] public string scrollViewPath; 

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            scrollView.PrefabPath = scrollViewPath;
            await base.Create();
            var bindingSet = this.CreateBindingSet<InvitedRuleView, InvitedRuleViewModel>();
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
            bindingSet.Build();
        }
    }
}