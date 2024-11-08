using System;
using System.Collections.Generic;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class UpgradeViewModel : ViewModelBase
    {
        
		[AutoNotify] private string levelNumsStr;
		[AutoNotify] private string defaultLevelStr;
		[AutoNotify] private string curLevelStr;
	    [AutoNotify] private ObservableList<CellItemBaseViewModel> awardsList = new();
	    private List<Reward> tempRewards = new List<Reward>();
		 public Action CloseCallback;
        [Preserve]
        public UpgradeViewModel(int curLv, int nextLv)
        {
	        LevelNumsStr = (nextLv).ToString();
	        DefaultLevelStr = $"{LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_05,curLv)}";
	        CurLevelStr = $"{LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_05,nextLv)}";
	        
	        for (int i = curLv; i < nextLv; i++)
	        {
		        var cfg = ConfigCenter.ProLevelCfgColl.GetDataById(i);
		        if (cfg != null)
		        {
			        AddRewards(cfg.Items);
		        }
	        }
	        Lodash.DealRewards(tempRewards);
        }
        
        private void AddRewards(List<Reward> rewards)
        {
	        int showIndex = 0;
	        foreach (var item in rewards)
	        {
		        tempRewards.Add(item);
		        if (item.Type == RewardType.Item||item.Type==RewardType.Lives)
		        {
			        var cellModel = CellItemBaseViewModel.Create(item);
			        cellModel.SizeIcon = Vector2.one * 120;
			        cellModel.SizeBg = Vector2.one * 167;
			        AwardsList.Add(cellModel);
		        }
		        showIndex++;
                
		        //todo:其他奖励
		        // UpgradeItemViewModel temp = new UpgradeItemViewModel(item); 
		        // AwardsList.Add(temp);
	        }
        }

        public void OnClose()
        {
	        CloseCallback?.Invoke();
	        UIManager.Instance.CloseDialog<UpgradeView>();
        }
    }
}