using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;


namespace DH.Game.ViewModels
{
    public partial class AdFreePlusViewModel : ViewModelBase
    {
        [AutoNotify] private AdFreePlusPrivilegeMonthCardItemViewModel adFreePlusPrivilegeMonthCardItemVm;
        [AutoNotify] private AdFreePlusPermanentCardItemViewModel adFreePlusPermanentCardItemVm;

        [Preserve]
        public AdFreePlusViewModel()
        {
            AdFreePlusPrivilegeMonthCardItemVm = new AdFreePlusPrivilegeMonthCardItemViewModel();
            AdFreePlusPermanentCardItemVm = new AdFreePlusPermanentCardItemViewModel();
        }
        
        
        protected override void OnDispose()
        {
            AdFreePlusPrivilegeMonthCardItemVm.Dispose();
            AdFreePlusPermanentCardItemVm.Dispose();
            base.OnDispose();
        }

    }
}