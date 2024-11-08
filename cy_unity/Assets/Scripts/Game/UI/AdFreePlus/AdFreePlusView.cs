using DH.Game.ViewModels;
using DH.UIFramework;

namespace DH.Game.UIViews
{
    public partial class AdFreePlusView : BaseView
    {
        public override bool FullScreen => false;

        public AdFreePlusPrivilegeMonthCardItemView adFreePlusPrivilegeMonthCardItem;
        public AdFreePlusPermanentCardItemView adFreePlusPermanentCardItem;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<AdFreePlusView, AdFreePlusViewModel>();
            
            bindingSet.Bind(adFreePlusPrivilegeMonthCardItem.BindingContext).For(v => v.DataContext).To(vm => vm.AdFreePlusPrivilegeMonthCardItemVm);
            bindingSet.Bind(adFreePlusPermanentCardItem.BindingContext).For(v => v.DataContext).To(vm => vm.AdFreePlusPermanentCardItemVm);

            bindingSet.Build();
        }
    }
}