using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class InvitedSuccessView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhText tipsDesc;
		public DhButton opBtn;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<InvitedSuccessView, InvitedSuccessViewModel>();
			bindingSet.Bind(tipsDesc).For(v => v.text).To(vm => vm.TipsDescStr);
			bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
            bindingSet.Build();
        }
    }
}