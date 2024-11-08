using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using DH.Config;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class CollegeRankRewardItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string rankTextStr;
		[AutoNotify] private string rankIconPath;
		public Func<object, object> GetItemGridCellCallback => GetItemGridCellCallbackByIndex;
		[AutoNotify] private ObservableDictionary<int,CellItemBaseViewModel> itemGridDictionary = new();

        [Preserve]
        public CollegeRankRewardItemViewModel(ActivityRankingCfg cfg)
        {
	        rankTextStr = cfg==null?LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips23):string.Empty;
	        rankIconPath = UIHelper.NoneImagePath();

	        if (cfg != null)
	        {
		        if (cfg.Id is 1 or 2 or 3)
		        {
			        RankIconPath = $"school[school_number_{cfg.Id}]";
		        }
		        else
		        {
			        if (cfg.Ranking.Count == 1)
			        {
				        rankTextStr = $"{cfg.Ranking[0]}+";
			        }
			        else
			        {
				        rankTextStr = $"{cfg.Ranking[0]}-{cfg.Ranking[1]}";
			        }
		        }

		        for (int i = cfg.Reward.Count-1; i >= 0; i--)
		        {
			        var model = CellItemBaseViewModel.Create(cfg.Reward[i], ECellItemSizeType.Size117X76);
			        model.IsOpenMask = true;
			        itemGridDictionary.Add(cfg.Reward.Count-1 -i,model);
		        }
	        }
        }
        
        
		[Command]
		private void OnClickOpBtn()
		{}
		private object GetItemGridCellCallbackByIndex(object index)
		{
			if (itemGridDictionary.TryGetValue((int)index, out CellItemBaseViewModel ret))
			{
				return ret;
			}

			return null;
		}

        
    }
}