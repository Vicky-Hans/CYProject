using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Game.UI.MainUi;
namespace DH.Game.ViewModels
{
    public partial class EndlessActivityViewModel : ViewModelBase
    {
        [AutoNotify] private string remainingNumStr;//挑战剩余次数
        [AutoNotify] private string maxKillNumStr;//最高杀敌数
        [AutoNotify] private string maxCoinNumStr;//最高金币数
        [AutoNotify] private long timeCd;//次数刷新倒计时
        [AutoNotify] private bool isEnough;//是否充足
        [AutoNotify] private bool isShowCostView;
        [AutoNotify] private bool isShowSweeping;
        [AutoNotify] private bool isShowSweepingTips;
        [AutoNotify] private string sweepingBtnImgStr;//开始按钮背景
        [AutoNotify] private ItemPriceNodeModel sweepingCostVm;
        [AutoNotify] private ItemPriceNodeModel battleBtnCostVm;
        [AutoNotify] private CommonButViewModel rankBtnItemVm;
        [AutoNotify] private CommonTopViewModel commonTopViewModel;
        private readonly Reward battleCost;
        private readonly Reward coinCost;
        private float time;
        [Preserve]
        public EndlessActivityViewModel()
        {
	        var costNum = 5;
	        List<GameConst.ItemIdCode> list = new(){GameConst.ItemIdCode.EnergyDrink,GameConst.ItemIdCode.Money,GameConst.ItemIdCode.Stone};
	        commonTopViewModel = new CommonTopViewModel(list);
	        var costCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.endless_04);
	        if (costCfg != null && costCfg.Content.Count > 0) costNum = costCfg.Content[0];
	        battleCost = new Reward(RewardType.Lives, (int)GameConst.ItemIdCode.EnergyDrink, costNum);
	        MaxKillNumStr = DataCenter.endlessData.MaxKill.ToString();
	        MaxCoinNumStr = DataCenter.endlessData.MaxCoinCount.ToString();
	        var name = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips12);
	        rankBtnItemVm = new CommonButViewModel(MainStageInfoNodeRightButType.Rank,"secret[secret_icon_1]", name, (objs) =>{
		        var tempVm= new RankViewModel(ERankType.RankItemEndless);
		        UIManager.Instance.OpenDialog<RankView>(tempVm).Forget();
	        }, () => false);
	        SweepingCostVm = new ItemPriceNodeModel(new Reward(RewardType.Item, (int)GameConst.ItemIdCode.Money, 0),true,null,false);
	        UpdataChallengeCount();
	        UpdateBattleBtnInfo();
	        MainUiManager.Instance.PropertyChanged += OnMainUiManagerChanged;
	        DataCenter.endlessData.PropertyChanged += OnEndlessDataChanged;
        }
	    
        protected override void OnDispose()
        {
	        DataCenter.endlessData.PropertyChanged -= OnEndlessDataChanged;
	        MainUiManager.Instance.PropertyChanged -= OnMainUiManagerChanged;
	        BattleBtnCostVm?.Dispose();
	        base.OnDispose();
        }
        
        public override void Update()
        {
	        base.Update();
	        if (!UIHelper.CalculateTime(ref time)) return;
	        if (TimeCd > 0) TimeCd -= 1;
	        if (TimeCd == 0)
	        {
		        UpdataChallengeCount();
		        UpdateBattleBtnInfo();
	        }
        }
        /// <summary>
        /// 刷新挑战次数
        /// </summary>
        private void UpdataChallengeCount()
        {
	        if (DataCenter.endlessData.RefreshStamp >= ServerTime.Instance.GetNowTime())
	        {
		        TimeCd = DataCenter.endlessData.RefreshStamp - ServerTime.Instance.GetNowTime();
	        }
	        if (DataCenter.endlessData.Count > 0)
	        {
		        RemainingNumStr = $"{LocalizeHelper.GetGlobal(GlobalLanguageId.endless_17)}{DataCenter.endlessData.Count}";
	        }
	        else
	        {
		        RemainingNumStr = $"{LocalizeHelper.GetGlobal(GlobalLanguageId.endless_17)}<color=red>0</color>";
	        }
        }
        private void OnMainUiManagerChanged(object sender, PropertyChangedEventArgs e)
        {
	        if(e.PropertyName== nameof(MainUiManager.Instance.ShowChapterIndex))
	        {
		        UpdateBattleBtnInfo();
	        }
        }
        /// <summary>
        /// 监听无尽关卡数据变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEndlessDataChanged(object sender, PropertyChangedEventArgs e)
        {
	        if(e.PropertyName is nameof(DataCenter.endlessData.MaxKill) or nameof(DataCenter.endlessData.MaxCoinCount))
	        {
		        MaxKillNumStr = DataCenter.endlessData.MaxKill.ToString();
		        MaxCoinNumStr = DataCenter.endlessData.MaxCoinCount.ToString();
	        }
	        else if (e.PropertyName is nameof(DataCenter.endlessData.Count))
	        {
		        UpdataChallengeCount();
		        UpdateBattleBtnInfo();
	        }
        }
        /// <summary>
        /// 点击规则按钮
        /// </summary>
        [Command]
        private void OnClickRuleBtn()
        {
	        var vm = new EndlessRuleViewModel();
	        UIManager.Instance.OpenDialog<EndlessRuleView>(vm).Forget();
        }
        /// <summary>
        /// 点击挑战
        /// </summary>
        [Command]
        private async void OnClickBattleBtn()
        {
	        if (EquipManager.Instance.IsExistNoneSlots())
	        {
		        var titleStr = LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_Title);
		        var contentStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips32);
		        var cancleStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_CancelTxt);
		        var conformStr = LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_ConfirmTxt);
		        Action cancleFunc = () => { };
		        Action confirmFunc = () =>
		        {
			        JumpManager.Instance.Jump(FunctionJumpCfgId.go_MainEquip);
		        };
		        CommonMessageBoxViewModel tempVm = new(titleStr,contentStr,cancleStr,conformStr,cancleFunc,confirmFunc, null);
		        UIManager.Instance.OpenDialog<CommonMessageBox>(tempVm).Forget();
		        return;
	        }

	        if (DataCenter.endlessData.Count > 0)//次数未用完，需要消耗体力
	        {
		        if (!UIHelper.CheckRewardIsEnough(battleCost, true)) return;
		        var req = new ReqEndlessBegin();
		        var result = await GameNetworkManager.Instance.SendAsync<RspEndlessBegin>(req);
		        if (result.rsp is not { Status: 0 })
		        {
			        if(result.rsp == null) return;
			        var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
			        ToastManager.Show(str);
			        return;
		        }
		        DataCenter.endlessData.Count -= 1;
		        UpdataChallengeCount();
		        UpdateBattleBtnInfo();
		        var maxPassChapter = DataCenter.mainStageData.GetMaxPassChapter();
		        GameManager.Instance.EnterGame(EStateType.StageTypeEndless,maxPassChapter,result.rsp.Data);
		        Lodash.DealRewards(result.rsp.Cost.ToList(),false);
	        }
	        else//次数用完仍然可继续挑战，且不消耗体力
	        {
		        var req = new ReqEndlessBegin();
		        var result = await GameNetworkManager.Instance.SendAsync<RspEndlessBegin>(req);
		        if (result.rsp is not { Status: 0 })
		        {
			        if(result.rsp == null) return;
			        var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
			        ToastManager.Show(str);
			        return;
		        }
		        DataCenter.endlessData.Count -= 1;
		        UpdataChallengeCount();
		        UpdateBattleBtnInfo();
		        var maxPassChapter = DataCenter.mainStageData.GetMaxPassChapter();
		        GameManager.Instance.EnterGame(EStateType.StageTypeEndless,maxPassChapter,result.rsp.Data);
	        }
        }
        /// <summary>
        /// 刷新战斗按钮状态
        /// </summary>
        private void UpdateBattleBtnInfo()
        {
	        IsEnough = DataCenter.endlessData.Count > 0;
	        if (DataCenter.endlessData.HasArchive)
	        {
		        IsShowCostView = false;
		        BattleBtnCostVm?.Dispose();
	        } 
	        else if (DataCenter.endlessData.Count > 0)
	        {
		        IsShowCostView = true;
		        BattleBtnCostVm?.Dispose();
		        BattleBtnCostVm = new ItemPriceNodeModel(battleCost,true,null,false);
		        BattleBtnCostVm.IsShowBg = false;
	        }
	        else
	        {
		        IsShowCostView = false;
		        BattleBtnCostVm?.Dispose();
	        }
	        SweepingBtnImgStr = IsEnough ? $"mainui[mainui_button_2]" : $"common[common_button_grey]";
	        SweepingCostVm.Reward = new Reward(RewardType.Item, (int)GameConst.ItemIdCode.Money, DataCenter.endlessData.MaxDayCoin);
	        SweepingCostVm.RewardCount = DataCenter.endlessData.MaxDayCoin;
	        IsShowSweeping = DataCenter.endlessData.Count > 0 && DataCenter.endlessData.Count < 3 && DataCenter.endlessData.MaxDayCoin > 0;
	        isShowSweepingTips = DataCenter.endlessData.Count >= 3;
        }
        /// <summary>
        /// 点击扫荡
        /// </summary>
        [Command]
        private void OnClickSweepingBtn()
        {
	        if (!UIHelper.CheckRewardIsEnough(battleCost, isJump:true)) return;
	        if (DataCenter.endlessData.Count <= 0)
	        {
		        var str = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips20);
		        ToastManager.Show(str);
		        return;
	        }
	        OnSweepingClaimed();
        }
        private async void OnSweepingClaimed() 
        {
	        var result = await GameNetworkManager.Instance.SendAsync<RspEndlessSweep>(new ReqEndlessSweep());
	        if (result.rsp.Status != 0)
	        {
		        var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
		        ToastManager.Show(str);
		        return;
	        }
	        DataCenter.endlessData.Count -= 1;
	        Lodash.DealRewards(result.rsp.Cost.ToList(),false);
	        UpdataChallengeCount();
	        UpdateBattleBtnInfo();
	        GameManager.Instance.IsEnterFight = false;
	        PlayerStats.Instance.KillCount = 0;
	        var tempVm = new MainStageGameResultViewModel(result.rsp.Rewards.ToList(), true);
	        UIManager.Instance.OpenDialog<MainStageGameResultView>(tempVm).Forget();
        }
        [Command]
        private void OnClickClose()
        {
	        UIManager.Instance.CloseDialog<EndlessActivityView>();
        }
    }
}