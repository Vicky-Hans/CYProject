using System;
using System.Collections.Generic;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class LimitRatioCellViewModel : ViewModelBase
    {
        [AutoNotify] private ObservableList<LimitCellViewModel> limitLayoutList = new();
        private List<LimitCellRatioData> curRatioCellList;
        [Preserve]
        public LimitRatioCellViewModel(List<LimitCellRatioData> ratioCellList)
        {
            curRatioCellList = ratioCellList;
            foreach (var item in curRatioCellList)
            {
                if (item.CurCfg != null)
                {
                    UIHelper.GetRewardInfo(item.CurCfg.Reward[0],out string nameStr,out string descStr);
                    var ratioStr = Math.Round(item.CurCfg.Weight / 100f, 2, MidpointRounding.AwayFromZero).ToString("0.##");
                    limitLayoutList.Add(new LimitCellViewModel(item.CurCfg, $"{nameStr}*{item.CurCfg.Reward[0].Count}",$"{ratioStr}%"));  
                }
                else
                {
                    UIHelper.GetRewardInfo(item.CurReward,out string nameStr,out string descStr);
                    limitLayoutList.Add(new LimitCellViewModel(item.CurReward, $"{nameStr}*{item.CurReward.Count}",item.CurRatioStr));  
                }
            }
        }
    }
}