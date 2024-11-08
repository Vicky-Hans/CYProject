using DH.Game.ViewModels;
using DH.UIFramework;
namespace DH.Game.UIViews
{
    public partial class AdFreeGiftView : BaseView
    {
        public override bool FullScreen => false;

        public AdFreeGiftTriggerGiftItemView triggerGiftItem;
        public AdFreeGiftPermanentCardItemView adFreeGiftPermanentCardItem;
        public AdFreeGiftPrivilegeMonthCardItemView adFreeGiftPrivilegeMonthCardItem;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<AdFreeGiftView, AdFreeGiftViewModel>();
            
            bindingSet.Bind(triggerGiftItem.BindingContext).For(v => v.DataContext).To(vm => vm.TriggerGiftItemVm);
            bindingSet.Bind(adFreeGiftPermanentCardItem.BindingContext).For(v => v.DataContext).To(vm => vm.AdFreeGiftPermanentCardItemVm);
            bindingSet.Bind(adFreeGiftPrivilegeMonthCardItem.BindingContext).For(v => v.DataContext).To(vm => vm.AdFreeGiftPrivilegeMonthCardItemVm);

            bindingSet.Build();
        }
    }
}