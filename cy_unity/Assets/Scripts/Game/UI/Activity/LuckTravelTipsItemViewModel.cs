using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.ComponentModel;
using DH.Config;
using DH.Data;
using DH.Proto;
using DH.UIFramework;
using DHFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class LuckTravelTipsItemViewModel : ViewModelBase
    {
        public int Index;
		[AutoNotify] private string descStr;
        [AutoNotify] private float alphaValue;
        [AutoNotify] private Vector3 localScale;
        [AutoNotify] private bool isShowBg;
        [Preserve]
        public LuckTravelTipsItemViewModel(LuckyTripRecord record,int index)
        {
            Index = index;

            string key;
            if (record.Level == 1)
            {
                key = GlobalLanguageId.LuckyJourney_04;;
            }else if (record.Level == 2)
            {
                key = GlobalLanguageId.LuckyJourney_07;;
            }else
            {
                key = GlobalLanguageId.LuckyJourney_08;;
            }

            DescStr = LocalizeHelper.GetGlobal(key, $"<color=#6EF11B>{record.Nick}</color>", $"<color=#FFE65A>{UIHelper.GetRewardName(new ResourceData(record.Reward))}x{record.Reward.Count}</color>");
            RefreshState();
            DataCenter.luckyTravelData.PropertyChanged += DataPropertyChanged;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            DataCenter.luckyTravelData.PropertyChanged -= DataPropertyChanged;
        }

        private void DataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LuckyTravelData.CurTipsInfoSelectIndex))
            {
                RefreshState();
            }
        }

        private void RefreshState()
        {
            var temp = 0;
            if (DataCenter.luckyTravelData.SuperRewardNum >= 4)
            {
                temp = DataCenter.luckyTravelData.CurTipsInfoSelectIndex + 3;
            }
            else
            {
                temp = DataCenter.luckyTravelData.SuperRewardNum-1;
            }

            if (temp >= DataCenter.luckyTravelData.SuperRewardNum)
            {
                temp = DataCenter.luckyTravelData.SuperRewardNum-1;
            }
            

            DHLog.Debug($"CurTipsInfoSelectIndex End: index{DataCenter.luckyTravelData.CurTipsInfoSelectIndex} Num:{DataCenter.luckyTravelData.SuperRewardNum}   temp:{temp}");
            if (Math.Abs(temp - Index) > 2)
            {
                AlphaValue = 0.7f;
                LocalScale = Vector3.one * 0.85f;
                IsShowBg = false;
            }else if (Math.Abs(temp - Index) > 0)
            {
                AlphaValue = 1f;
                LocalScale = Vector3.one * 0.85f;
                IsShowBg = false;
            }
            else
            {
                IsShowBg = true;
                AlphaValue = 1f;
                LocalScale = Vector3.one;
            }
        }
    }
}