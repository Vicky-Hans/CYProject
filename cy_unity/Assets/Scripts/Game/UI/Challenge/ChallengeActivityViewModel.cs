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
using DH.UIFramework.Observables;
using UnityEngine.Scripting;
using DH.UIFramework;
using Game.UI.MainUi;
namespace DH.Game.ViewModels
{
    public partial class ChallengeActivityViewModel : ViewModelBase
    {
        [AutoNotify] private string remainingNumStr;//挑战剩余次数
        [AutoNotify] private string weekProgressNumStr;//周奖励领取进度
        [AutoNotify] private float weekProgressValue;//周奖励领取进度
        [AutoNotify] private long dailyTimeCd;//次数刷新倒计时
        [AutoNotify] private long weekTimeCd;//周刷新倒计时
        [AutoNotify] private bool isEnough;//是否充足
        [AutoNotify] private bool isShowRed;//是否充足
        [AutoNotify] private bool isShowCostView;
        [AutoNotify] private bool isShowSweeping;
        [AutoNotify] private string startBtnImgStr;//开始按钮背景
        [AutoNotify] private string sweepingBtnImgStr;//开始按钮背景
        [AutoNotify] private string killNumStr;//击杀
        [AutoNotify] private ItemPriceNodeModel battleBtnCostVm;
        [AutoNotify] private ItemPriceNodeModel sweepingCostVm;
        [AutoNotify] private CommonTopViewModel commonTopViewModel;
        [AutoNotify] private ObservableList<ChallengeBuffItemViewModel> buffItemList = new();
        [AutoNotify] private ObservableList<ChallengeBoxItemViewModel> weekAwardList = new();
        [AutoNotify] private ObservableList<ChallengeDailyBoxViewModel> dailyAwardList = new();
        private int maxWeekendAwardNum = 5;//最大周奖励个数
        private Reward battleCost;
        private float time;
        public Func<Object, object> GetDailyBoxVmCallBack => GetDailyBoxVmByIndex;
        public Func<Object, object> GetWeekBoxVmCallBack => GetWeekBoxVmByIndex;
        [Preserve]
        public ChallengeActivityViewModel()
        {
	        var costNum = 5;
	        List<GameConst.ItemIdCode> list = new(){GameConst.ItemIdCode.EnergyDrink,GameConst.ItemIdCode.Money,GameConst.ItemIdCode.Stone};
	        commonTopViewModel = new CommonTopViewModel(list);
	        var costCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.dailyStage_02);
	        if (costCfg != null && costCfg.Content.Count > 0) costNum = costCfg.Content[0];
	        battleCost = new Reward(RewardType.Lives, (int)GameConst.ItemIdCode.EnergyDrink, costNum);
	        for (var i = 0; i < DataCenter.dailyFightData.Buff.Count; i++)
	        {
		        var buff = new ChallengeBuffItemViewModel(DataCenter.dailyFightData.Buff[i]);
		        BuffItemList.Add(buff);
	        }
	        var dailyAwardCfg = ConfigCenter.DailyStageProgressRewardCfgColl.GetDataByType(1);//日奖励
	        for (var i = 0; i < dailyAwardCfg.Count; i++)
	        {
		        var vm = new ChallengeDailyBoxViewModel(i,dailyAwardCfg[i]);
		        DailyAwardList.Add(vm);
	        }
	        var weekAwardCfg = ConfigCenter.DailyStageProgressRewardCfgColl.GetDataByType(2);//周奖励
	        for (var i = 0; i < weekAwardCfg.Count; i++)
	        {
		        var boxItemVm = new ChallengeBoxItemViewModel(weekAwardCfg[i]);
		        if (i == weekAwardCfg.Count-1)
		        {
			        maxWeekendAwardNum = weekAwardCfg[i].Value2;
		        }
		        WeekAwardList.Add(boxItemVm);
	        }
	        UpdataChallengeCount();
	        UpdateBattleBtnInfo();
	        MainUiManager.Instance.PropertyChanged += OnMainUiManagerChanged;
	        DataCenter.dailyFightData.PropertyChanged += OnDailyFightDataChanged;
        }
        protected override void OnDispose()
        {
	        DataCenter.dailyFightData.PropertyChanged -= OnDailyFightDataChanged;
	        MainUiManager.Instance.PropertyChanged -= OnMainUiManagerChanged;
	        BattleBtnCostVm?.Dispose();
	        base.OnDispose();
        }
        public override void Update()
        {
	        base.Update();
	        if (!UIHelper.CalculateTime(ref time)) return;
	        if (DailyTimeCd > 0) DailyTimeCd -= 1;
	        if (WeekTimeCd > 0) WeekTimeCd -= 1;
	        if (DailyTimeCd != 0 && WeekTimeCd != 0) return;
	        if (DailyTimeCd == 0)
	        {
		        
	        }
	        UpdataChallengeCount();
	        UpdateBattleBtnInfo();
        }
        /// <summary>
        /// 刷新挑战次数
        /// </summary>
        private void UpdataChallengeCount()
        {
	        if (DataCenter.dailyFightData.DayRefreshStamp >= ServerTime.Instance.GetNowTime())
	        {
		        DailyTimeCd = DataCenter.dailyFightData.DayRefreshStamp - ServerTime.Instance.GetNowTime();
	        }
	        if (DataCenter.dailyFightData.WeekRefreshStamp >= ServerTime.Instance.GetNowTime())
	        {
		        WeekTimeCd = DataCenter.dailyFightData.WeekRefreshStamp - ServerTime.Instance.GetNowTime();
	        }
	        RemainingNumStr = $"{LocalizeHelper.GetGlobal(GlobalLanguageId.endless_04)}:{DataCenter.dailyFightData.Count}";
        }
        private void OnMainUiManagerChanged(object sender, PropertyChangedEventArgs e)
        {
	        if(e.PropertyName== nameof(MainUiManager.Instance.ShowChapterIndex))
	        {
		        UpdataChallengeCount();
		        UpdateBattleBtnInfo();
	        }
        }
        /// <summary>
        /// 监听无尽关卡数据变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDailyFightDataChanged(object sender, PropertyChangedEventArgs e)
        {
	        if(e.PropertyName== nameof(DataCenter.dailyFightData.Buff))
	        {
		        BuffItemList.Clear();
		        for (var i = 0; i < DataCenter.dailyFightData.Buff.Count; i++)
		        {
			        var buff = new ChallengeBuffItemViewModel(DataCenter.dailyFightData.Buff[i]);
			        BuffItemList.Add(buff);
		        }
	        }
	        UpdataChallengeCount();
	        UpdateBattleBtnInfo();
        }
        /// <summary>
        /// 点击规则按钮
        /// </summary>
        [Command]
        private void OnClickRuleBtn()
        {
	        var content1 = LocalizeHelper.GetGlobal(GlobalLanguageId.DailyStage_15);
	        var title = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips13);
	        UIHelper.OpenCommonRule(title,content1);
        }
        /// <summary>
        /// 点击扫荡
        /// </summary>
        [Command]
        private void OnClickSweepingBtn()
        {
	        if (!UIHelper.CheckRewardIsEnough(battleCost, isJump:true)) return;
	        if (DataCenter.dailyFightData.Count <= 0)
	        {
		        var str = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips20);
		        ToastManager.Show(str);
		        return;
	        }
	        OnSweepingClaimed();
        }
        private async void OnSweepingClaimed() 
        {
	        var result = await GameNetworkManager.Instance.SendAsync<RspDailyFightSweep>(new ReqDailyFightSweep());
	        if (result.rsp.Status != 0)
	        {
		        var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
		        ToastManager.Show(str);
		        return;
	        }
	        DataCenter.dailyFightData.Count -= 1;
	        if (DataCenter.dailyFightData.Count < 0) DataCenter.dailyFightData.Count = 0;
	        Lodash.DealRewards(result.rsp.Cost.ToList(),false);
	        DataCenter.dailyFightData.DailyKills = result.rsp.DailyKills;
	        UpdataChallengeCount();
	        UpdateBattleBtnInfo();
	        GameManager.Instance.IsEnterFight = false;
	        var tempVm = new MainStageGameResultViewModel(result.rsp.Rewards.ToList(), true);
	        UIManager.Instance.OpenDialog<MainStageGameResultView>(tempVm).Forget();
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
	        if (!UIHelper.CheckRewardIsEnough(battleCost, isJump:true)) return;
	        if (DataCenter.dailyFightData.Count <= 0)
	        {
		        var str = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips20);
		        ToastManager.Show(str);
		        return;
	        }
	        var req = new ReqDailyFightBegin();
	        var result = await GameNetworkManager.Instance.SendAsync<RspDailyFightBegin>(req);
	        if (result.rsp is not { Status: 0 })
	        {
		        if(result.rsp == null) return;
		        var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
		        ToastManager.Show(str);
		        return;
	        }
	        DataCenter.dailyFightData.Count -= 1;
	        if (DataCenter.dailyFightData.Count < 0) DataCenter.dailyFightData.Count = 0;
	        UpdataChallengeCount();
	        UpdateBattleBtnInfo();
	        if (result.rsp.Buff is { Count: > 0 })
	        {
		        DataCenter.dailyFightData.Buff.Clear();
		        for (var i = 0; i < result.rsp.Buff.Count; i++)
		        {
			        DataCenter.dailyFightData.Buff.Add(result.rsp.Buff[i]);
		        }
	        }
	        if (result.rsp.Mons is { Count: > 0 })
	        {
		        DataCenter.dailyFightData.Mons.Clear();
		        foreach (var monsterInfo in result.rsp.Mons)
		        {
			        DataCenter.dailyFightData.Mons.Add(monsterInfo.Key,monsterInfo.Value);
		        }
	        }
	        GameManager.Instance.EnterGame(EStateType.StageTypeChallenge,DataCenter.mainStageData.GetMaxPassChapter(),result.rsp.Data);
	        Lodash.DealRewards(result.rsp.Cost.ToList(),false);
        }
        /// <summary>
        /// 刷新战斗按钮状态
        /// </summary>
        private void UpdateBattleBtnInfo()
        {
	        IsEnough = DataCenter.dailyFightData.Count > 0;
	        IsShowRed = DataCenter.dailyFightData.Count > 0 && DataCenter.livesData.CheckItemIsEnough((int)GameConst.ItemIdCode.EnergyDrink, battleCost.Count);
	        StartBtnImgStr = IsEnough ? $"mainui[mainui_button_1]" : $"common[common_button_grey]";
	        SweepingBtnImgStr = IsEnough ? $"mainui[mainui_button_2]" : $"common[common_button_grey]";
	        IsShowCostView = !DataCenter.dailyFightData.HasArchive;
	        BattleBtnCostVm?.Dispose();
	        BattleBtnCostVm = new ItemPriceNodeModel(battleCost,true,null,false);
	        BattleBtnCostVm.IsShowBg = false;
	        IsShowSweeping = DataCenter.dailyFightData.Sweep;
	        WeekProgressNumStr = $"{DataCenter.dailyFightData.WeekProgress}/{maxWeekendAwardNum}";
	        WeekProgressValue = DataCenter.dailyFightData.WeekProgress*1.0f / maxWeekendAwardNum;
	        KillNumStr = $"{LocalizeHelper.GetGlobal(GlobalLanguageId.DailyStage_03)}: {DataCenter.dailyFightData.DailyKills}";
	        for (var i = 0; i < WeekAwardList.Count; i++)
	        {
		        WeekAwardList[i].UpdateBoxData();
	        }
	        for (var i = 0; i < DailyAwardList.Count; i++)
	        {
		        DailyAwardList[i].UpdateBoxState();
	        }
        }
        [Command]
        private void OnClickClose()
        {
	        UIManager.Instance.CloseDialog<ChallengeActivityView>();
        }
        private object GetDailyBoxVmByIndex(object index)
        {
	        return (int)index >= DailyAwardList.Count ? null : DailyAwardList[(int)index];
        }
        private object GetWeekBoxVmByIndex(object index)
        {
	        return (int)index >= WeekAwardList.Count ? null : WeekAwardList[(int)index];
        }
    }
}