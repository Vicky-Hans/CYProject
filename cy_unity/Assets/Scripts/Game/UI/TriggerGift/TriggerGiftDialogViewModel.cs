using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using DH.Config;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;


namespace DH.Game.ViewModels
{
    public partial class TriggerGiftDialogViewModel : ViewModelBase
    {
        public override bool AutoDispose => true;
		[AutoNotify] private TriggerGiftItemViewModel triggerGiftItemViewVm;

        [Preserve]
        public TriggerGiftDialogViewModel(int showId)
        {
            if (showId != 0)
            {
                var cfg = ConfigCenter.TriggerGiftCfgColl.GetDataById(showId);
                if (cfg != null)
                {
                    TriggerGiftManager.Instance.SaveTriggerTypeFirst(cfg.Type);
                    triggerGiftItemViewVm = new TriggerGiftItemViewModel(cfg, () =>
                    {
                        UIManager.Instance.CloseDialog<TriggerGiftDialogView>();
                    });
                }
            }
        }

        [Command]
        private void OnClickClose()
        {
            UIManager.Instance.CloseDialog<TriggerGiftDialogView>();
        }
    }
}