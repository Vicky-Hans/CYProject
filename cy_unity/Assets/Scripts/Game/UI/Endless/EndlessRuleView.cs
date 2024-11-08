using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using Cysharp.Threading.Tasks;
namespace DH.Game.UIViews
{
    public partial class EndlessRuleView : BaseView
    {
        public DhButton closeBtn;
        public override async UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<EndlessRuleView, EndlessRuleViewModel>();
            bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
            bindingSet.Build();
        }
    }
}