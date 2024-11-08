using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
namespace DH.Game.UIViews
{
    public partial class ChallengeTipsView : BaseView
    {
        public override bool FullScreen => false;
        public Button opBtn;
        public DhText tipsText;
        public ClickToClose clickToClose;
        public override async UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<ChallengeTipsView, ChallengeTipsViewModel>();
            bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
            bindingSet.Bind(clickToClose).For(v=>v.CloseCallback).To(vm=>vm.OnClickCloseBtn);
            bindingSet.Bind(tipsText).For(v => v.text).To(vm => vm.RefreshTimeStr);
            bindingSet.Build();
        }
    }
}