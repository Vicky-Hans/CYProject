using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using DH.Config;
using DH.UIFramework;

namespace DH.Game.ViewModels
{
    public partial class TriggerGiftTypeShowItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string nameStr;
        [AutoNotify] private string icon;
        [AutoNotify] private bool select;
        [AutoNotify] private bool red;
        
        public int ShowType;
        
        private bool isSelect;
        public bool IsSelect
        {
            get => isSelect;
            set
            {
                isSelect = value;
                Select = isSelect;
            }
        }
        
        private readonly Action<int> callback;
        
        [Preserve]
        public TriggerGiftTypeShowItemViewModel(int mType,bool isSelect,Action<int> mCallback)
        {
            ShowType = mType;
            IsSelect = isSelect;
            Icon = ShowType switch
            {
                (int)TriggerGiftViewShowType.Gift => "specialshop[specialshop_icon_1]",
                (int)TriggerGiftViewShowType.DayGift => "specialshop[specialshop_icon_4]",
                (int)TriggerGiftViewShowType.WeekGift => "specialshop[specialshop_icon_2]",
                (int)TriggerGiftViewShowType.MonthGift => "specialshop[specialshop_icon_3]",
                 _ => UIHelper.NoneImagePath(),
            };
            NameStr = LocalizeHelper.GetGlobal(ShowType switch
            {
                (int)TriggerGiftViewShowType.Gift => GlobalLanguageId.Trigger05,
                (int)TriggerGiftViewShowType.DayGift => GlobalLanguageId.Trigger12,
                (int)TriggerGiftViewShowType.WeekGift => GlobalLanguageId.Trigger06,
                (int)TriggerGiftViewShowType.MonthGift => GlobalLanguageId.Trigger09,
            });
            if (ShowType == (int)TriggerGiftViewShowType.Gift)
            {
                Red = TriggerGiftManager.Instance.IsRed();
            }
            callback = mCallback;
        }
        
        
        [Command]
        private void OnClickBut()
        {
            callback?.Invoke(ShowType);
        }
        
    }
}