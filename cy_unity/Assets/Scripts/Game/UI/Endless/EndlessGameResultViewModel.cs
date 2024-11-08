using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class EndlessGameResultViewModel : ViewModelBase
    {
        [AutoNotify] private string titleBgPath;
		[AutoNotify] private Color titleColorA;
		[AutoNotify] private Color titleColorB;
		[AutoNotify] private string effectImgPath;
		[AutoNotify] private string titleTextStr;
		[AutoNotify] private bool isWin;
		[AutoNotify] private bool isNewTime;
		[AutoNotify] private bool isNewCoin;
		[AutoNotify] private bool isShowAwardNode;
		[AutoNotify] private string killCountTextStr;
		[AutoNotify] private string timeCountTextStr;
		[AutoNotify] private ObservableList<CanClaimItemViewModel> awardScrollViewList = new();
        [Preserve]
        public EndlessGameResultViewModel(List<Resource> rewards,FightStat fightStat)
        {
	        IsWin = true;
	        IsNewTime = fightStat.SurvivalTime > DataCenter.endlessData.MaxSurvival;
	        if (!IsNewTime) IsNewTime = fightStat.Kills > DataCenter.endlessData.MaxKill;
	        TimeCountTextStr = $"{UIHelper.ConvertTimeSecondToString(fightStat.SurvivalTime,ETimeFormatType.TimeFormatTypeMinuteWithUnit)}";
	        KillCountTextStr = $"{fightStat.Kills}";
	        long totalCoint = 0;
	        for (var i = 0; i < rewards.Count; i++)
	        {
		        if (rewards[i].Id == (int)GameConst.ItemIdCode.Money)
		        {
			        totalCoint += rewards[i].Count;
		        }
	        }
	        IsNewCoin = totalCoint > DataCenter.endlessData.MaxCoinCount;
	        if (IsNewCoin)
	        {
		        DataCenter.endlessData.MaxCoinCount = (int)totalCoint;
		        DataCenter.endlessData.MaxKill = fightStat.Kills;
	        }
	        if (IsNewTime || IsNewCoin)
	        {
		        TitleTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.endless_14);//新纪录
	        }
	        else
	        {
		        TitleTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips23);//胜利
	        }
	        for (var i = 0; i < AwardScrollViewList.Count; i++)
	        {
		        AwardScrollViewList[i].Dispose();
	        }
	        AwardScrollViewList.Clear();
	        if (DataCenter.endlessData.Count >= 0 && rewards.Count > 0)
	        {
		        UIHelper.SortReward(rewards);
		        rewards.Sort((a, b) => GameManager.Instance.GetRewardCompareValue(a).CompareTo(GameManager.Instance.GetRewardCompareValue(b)));
		        for (var i = 0; i < rewards.Count; i++)
		        {
			        var res = new MailRewards {id = rewards[i].Id,count = rewards[i].Count,type = rewards[i].Type};
			        var tempVm = new CanClaimItemViewModel(res, false);
			        AwardScrollViewList.Add(tempVm);
		        }
		        Lodash.DealRewards(rewards, true);
		        IsShowAwardNode = true;
	        }
	        else
	        {
		        IsNewTime= false;
		        IsNewCoin= false;
		        IsShowAwardNode = false;
	        }
        }
        [Command]
		private void OnClickHurtBtn()
		{
			var hurtDic = PlayerStats.Instance.SkillHurtsDic;
			var tempVm = new HurtViewModel(hurtDic);
			UIManager.Instance.OpenDialog<HurtView>(tempVm).Forget();
		}
		// 游戏主界面
		public void OnClickCloseBtn()
		{
			UIManager.Instance.CloseDialog<EndlessGameResultView>();
			GameManager.Instance.OnExitGame();
		}
    }
}