using System.Collections.Generic;
using System.Linq;
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
using Game.UI.MainUi;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class MainStageGameResultViewModel : ViewModelBase
    {
        
		[AutoNotify] private string titleBgPath;
		[AutoNotify] private Color titleColorA;
		[AutoNotify] private Color titleColorB;
		[AutoNotify] private string effectImgPath;
		[AutoNotify] private string titleTextStr;
	    [AutoNotify] private ObservableList<CanClaimItemViewModel> awardScrollViewList = new();
		[AutoNotify] private string leftCountTextStr;
		[AutoNotify] private bool isWin;
		[AutoNotify] private bool isCanClickDoubleBtn;
		[AutoNotify] private string doubleBtnSpritePath;
		[AutoNotify] private string killNumStr;
		[AutoNotify] private bool isShowDouble;
		
		public CommonAdvIconViewModel CommonAdvVm;
		private readonly  string[] titleBgNameArray = new[] { "fight[fight_panel_5]", "fight[fight_panel_6]" };
		private readonly string[] titleEffectNameArray = new[] { "fight[fight_img_1]", "fight[fight_img_2]" };
		private readonly string[] colorAArray = new[] {"#FFF892FF",  "#B0B2B0FF"}; 
		private readonly string[] colorBArray = new[] { "#FCFEE2FF", "#B0B2B0FF"};
		private readonly string[] doubleBtnImgArray = new[] { "common[commom_button_blue]", "common[common_button_grey]"};
		private readonly string[] titleStrArray = new[] { LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips23),
			LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips19) };
		
        [Preserve]
        public MainStageGameResultViewModel(List<Resource> rewards, bool win)
        {
	        IsWin = win;
	        int index = win? 0 : 1;
	        TitleBgPath = titleBgNameArray[index];
	        TitleColorA = UIHelper.HexColorStrToColor(colorAArray[index]);
	        TitleColorB = UIHelper.HexColorStrToColor(colorBArray[index]);
	        EffectImgPath = titleEffectNameArray[index];
	        TitleTextStr = titleStrArray[index];
	        
	        IsCanClickDoubleBtn = DataCenter.mainStageData.AdCount > 0;
	        DoubleBtnSpritePath = doubleBtnImgArray[IsCanClickDoubleBtn ? 0 : 1];
	       
	        UpdateAwardScrollViewList(rewards);
	        Lodash.DealRewards(rewards, true);
	        UpdateLeftCountTextStr();
	        CommonAdvVm = new CommonAdvIconViewModel();
	        if (IsWin)
	        {   
		        AudioManager.Instance.Play(AudioType.GameWin);
	        }
	        else
	        {
		        AudioManager.Instance.Play(AudioType.GameFail);
	        }

	        if (GameDataManager.Instance.CurStageType == EStateType.StageTypeChallenge ||
	            GameDataManager.Instance.CurStageType == EStateType.StageTypeSecret || 
	            MainUiManager.Instance.CurTabType == ETabType.TabTypeActivity)
	        {
		        IsShowDouble = false;
		        
	        }
	        else
	        {
		        IsShowDouble = true;
	        }
	        KillNumStr = $"{LocalizeHelper.GetGlobal(GlobalLanguageId.DailyStage_08)}{PlayerStats.Instance.KillCount}";
        }
        
        protected override void OnDispose()
        {
	        CommonAdvVm?.Dispose();
	        base.OnDispose();
        }
        
        [Command]
        private async void OnClickDoubleBtn()
        {
	        if(!IsCanClickDoubleBtn) return;
	        
	        UIHelper.ShowRewardAds(() =>
	        {
		        RequestDouble().Forget();
	        });
        }

        [Command]
        private void OnClickConfirmBtn()
        {
	        // 游戏主界面
	        UIManager.Instance.CloseDialog<MainStageGameResultView>();
	        GameManager.Instance.OnExitGame();
        }

        private async UniTaskVoid RequestDouble()
        {
	        var req = new ReqMainFightAdDouble();
	        req.ChapterId = GameManager.Instance.CurChapterId;
	        req.Uid = GameDataManager.Instance.Uid;
	        req.Stat = GameManager.Instance.GetFightStat();
	        var result = await GameNetworkManager.Instance.SendAsync<RspMainFightAdDouble>(req);
	        if (result.rsp is not { Status: 0 })
	        {
		        if(result.rsp == null) return;
		        var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
		        ToastManager.Show(str);
		        return;
	        }

	        IsCanClickDoubleBtn = false;
	        DoubleBtnSpritePath = doubleBtnImgArray[IsCanClickDoubleBtn ? 0 : 1];
	        DataCenter.mainStageData.AdCount -= 1;
	        var tempList = result.rsp.Rewards.ToList();
	        UIHelper.SortReward(tempList);
	        // 加奖励
	        Lodash.DealRewards(tempList, true);
	        // 这里播双倍的动画
	        tempList.Sort((a, b) =>
	        {
		        return GameManager.Instance.GetRewardCompareValue(a).CompareTo(GameManager.Instance.GetRewardCompareValue(b));
	        });

	        UpdateLeftCountTextStr();
	        foreach (var tempResource in tempList)
	        {
		        foreach (var curItem in awardScrollViewList)
		        {
			        if (curItem.CheckIsSameReward(tempResource))
			        {
				        curItem.DoDoubleAction((int)tempResource.Count);
				        await UniTask.Delay(180);
				        break;
			        }
		        }
	        }
        }

        [Command]
		private void OnClickHurtBtn()
		{
			var hurtDic = PlayerStats.Instance.SkillHurtsDic;
			var tempVm = new HurtViewModel(hurtDic);
			UIManager.Instance.OpenDialog<HurtView>(tempVm).Forget();
		}

		public void OnClickCloseBtn()
		{
			// 游戏主界面
			UIManager.Instance.CloseDialog<MainStageGameResultView>();
			GameManager.Instance.OnExitGame();
		}

		public void UpdateAwardScrollViewList(List<Resource> rewards)
		{
			foreach (var item in AwardScrollViewList)
			{
				item.Dispose();
			}
			AwardScrollViewList.Clear();
			UIHelper.SortReward(rewards);
			rewards.Sort((a, b) =>
			{
				return GameManager.Instance.GetRewardCompareValue(a).CompareTo(GameManager.Instance.GetRewardCompareValue(b));
			});
			foreach (var item in rewards)
			{
				var res = new MailRewards
				{
					id = item.Id,
					count = item.Count,
					type = item.Type,
				};
				var tempVm = new CanClaimItemViewModel(res, false);
				AwardScrollViewList.Add(tempVm);
			}
		}

		private void UpdateLeftCountTextStr()
		{
			var leftCount = DataCenter.mainStageData.AdCount;
			var defCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.copy_DoubleSettlement);
			if(defCfg == null || defCfg.Content == null || defCfg.Content.Count <= 0) return;

			var str = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips39);
			LeftCountTextStr = $"<color=#FFF8E4>{str}</color><color=#64D32E>{leftCount}/{defCfg.Content[0]}</color>";
			
		}
    }
}