using System.ComponentModel;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.ViewModels;

using UnityEngine.Scripting;


namespace DH.Game.ViewModels
{
    public partial class AdFreeGiftViewModel : ViewModelBase
    {
        [AutoNotify] private AdFreeGiftTriggerGiftItemViewModel triggerGiftItemVm;
        [AutoNotify] private AdFreeGiftPermanentCardItemViewModel adFreeGiftPermanentCardItemVm;
        [AutoNotify] private AdFreeGiftPrivilegeMonthCardItemViewModel adFreeGiftPrivilegeMonthCardItemVm;

        [Preserve]
        public AdFreeGiftViewModel()
        {
            TriggerGiftItemVm = new AdFreeGiftTriggerGiftItemViewModel();
            AdFreeGiftPermanentCardItemVm = new AdFreeGiftPermanentCardItemViewModel();
            adFreeGiftPrivilegeMonthCardItemVm = new AdFreeGiftPrivilegeMonthCardItemViewModel();
            TriggerGiftManager.Instance.SaveTriggerTypeFirst(6);
            TriggerGiftItemVm.PropertyChanged += BuysChange;
            AdFreeGiftPermanentCardItemVm.PropertyChanged += BuysChange;
            adFreeGiftPrivilegeMonthCardItemVm.PropertyChanged += BuysChange;
        }
        
        private void BuysChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(TriggerGiftItemVm.IsShowSelf) or nameof(TriggerGiftItemVm.IsShowSelf) or nameof(TriggerGiftItemVm.IsShowSelf))
            {
                if (!TriggerGiftItemVm.IsShowSelf && !AdFreeGiftPermanentCardItemVm.IsShowSelf && !adFreeGiftPrivilegeMonthCardItemVm.IsShowSelf) 
                {
                    UIManager.Instance.CloseDialog<AdFreeGiftView>();
                }
            }
        }
        
        protected override void OnDispose()
        {
            triggerGiftItemVm.Dispose();
            adFreeGiftPermanentCardItemVm.Dispose();
            adFreeGiftPrivilegeMonthCardItemVm.Dispose();
            TriggerGiftItemVm.PropertyChanged -= BuysChange;
            AdFreeGiftPermanentCardItemVm.PropertyChanged -= BuysChange;
            adFreeGiftPrivilegeMonthCardItemVm.PropertyChanged -= BuysChange;                
            base.OnDispose();
        }
        
    }
}