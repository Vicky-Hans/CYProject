using DH.Config;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Data;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{ 
    public partial class PermanentCardPropertyViewModel : ViewModelBase
    {
        
		[AutoNotify] private string textStr;
        [AutoNotify] private string nameStr;
        [AutoNotify] private bool showHeadEffect;
        [AutoNotify] private bool showNameEffect;
        [AutoNotify] private bool showFreeAdEffect;
        [AutoNotify] private ObservableList<CellItemBaseViewModel> awardScrollviewList = new();
        [Preserve]
        public PermanentCardPropertyViewModel(MonthCardEffectType effectType,string des)
        {
            ShowHeadEffect = effectType == MonthCardEffectType.DedicatedAvatar;
            ShowNameEffect = effectType == MonthCardEffectType.GoldenNickname;
            ShowFreeAdEffect = effectType == MonthCardEffectType.ADFreeForever;
            TextStr = ShowFreeAdEffect ? $"<color=#00F0FF>{des}</color>" : $"<color=#FFF8E4>{des}</color>";

            if (ShowNameEffect)
            {
                nameStr = DataCenter.charcaterData.Digest.Name;
            }
            
            if (ShowHeadEffect)
            {
                AwardScrollviewList.Clear();
                var cfg = ConfigCenter.MonthlyVipMainCfgColl.GetDataById((int)MonthType.PermanentCard);
                TextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.MonthlyVip_tips08);
                if (cfg.Reward == null)return;
                for (int i = 0; i < cfg.Reward.Count; i++)
                {
                    AwardScrollviewList.Add(CellItemBaseViewModel.Create( cfg.Reward[i],ECellItemSizeType.Size90X80));
                }
            }
        }
        
    }
}