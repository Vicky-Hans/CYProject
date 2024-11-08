using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using Cysharp.Threading.Tasks;
namespace DH.Game.UIViews
{
    public partial class EndlessOpenView : BaseView
    {
        public override bool FullScreen => false;
        public DhButton btnClose;
        public override async UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<EndlessOpenView, EndlessOpenViewModel>();
            bindingSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnClickCloseCommand);
            bindingSet.Build();
        }
    }
}