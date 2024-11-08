using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;

namespace DH.Game.UIViews
{
    public partial class TriggerGiftDialogView : BaseView
    {
        public override bool FullScreen => false;
		public TriggerGiftItemView triggerGiftItemView;
        public DhButton btnClose;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<TriggerGiftDialogView, TriggerGiftDialogViewModel>();
			bindingSet.Bind(triggerGiftItemView.BindingContext).For(v => v.DataContext).To(vm => vm.TriggerGiftItemViewVm);
            bindingSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnClickCloseCommand);
            bindingSet.Build();
        }
    }
}