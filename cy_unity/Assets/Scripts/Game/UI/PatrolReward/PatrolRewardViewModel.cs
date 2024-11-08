using System;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using Game.UI.MainUi;


namespace DH.Game.ViewModels
{
    public partial class PatrolRewardViewModel : ViewModelBase
    {
        
		[AutoNotify] private string timesDesStr;
		[AutoNotify] private string goldNumsTextStr;
		[AutoNotify] private string powerTextStr;
	    [AutoNotify] private ObservableList<CellItemBaseViewModel> scrollViewList = new();
	    [AutoNotify] private ObservableList<CellItemBaseViewModel> monthCardScrollViewList = new();
		[AutoNotify] private bool isShowCardView;
		[AutoNotify] private string maxPatrolTime;
		[AutoNotify] private string allLastNums;
		[AutoNotify] private string monthCardAttrText;
        public CopyCfg CopyThreadCfg;
        [AutoNotify] private bool redGo;
        private HangupData Data => DataCenter.mainStageData.Hangup;

        /// <summary>
        /// 巡逻结算时间（单位秒）
        /// </summary>
		private int patrolCompletion  => ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.PatrolIncome_04).Content[0];
        
        [Preserve]
        public PatrolRewardViewModel()
        {
	        CopyThreadCfg = ConfigCenter.CopyCfgColl.GetDataById(DataCenter.mainStageData.GetMaxPassChapter());
	        
	        InitRewardCount();
	        TimeDes();
	        RedGo = Data.IsRed() && MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctioonTypeFarm);
	        //最大保留时长
	        var maxTimes = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.PatrolIncome_01).Content[0];
	        var monthCardTimes = ConfigCenter.MonthlyVipEffectCfgColl.GetDataById((int)MonthCardEffectType.PatrolTimes).Value[0];
	        var otherNums = maxTimes*(monthCardTimes / 100f);
	        MaxPatrolTime = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips47, maxTimes,(int) otherNums);
	        
	        //特权月卡收益描述
	        var monthCard = ConfigCenter.MonthlyVipEffectCfgColl.GetDataById((int)MonthCardEffectType.PatrolGains15).Value[0];
	        var monthCardLanguage = ConfigCenter.MonthlyVipEffectLanguageCfgColl.GetDataById((int)MonthCardEffectType.PatrolGains15).Dec;
	        MonthCardAttrText = string.Format(monthCardLanguage,$"<color=#64D32E>{monthCard}</color>");
		        
	        DataCenter.monthCardData.PropertyChanged += MonthCardChange;
	        Data.PropertyChanged += MonthCardChange;
	        PlayerInfoManager.Instance.PropertyChanged += SecondDay;
        }

        #region 按钮事件
        
        /// <summary>
        /// 快速巡逻
        /// </summary>
        [Command]
        private void OnClickFastBtn()
        {
	        UIManager.Instance.OpenDialog<PatrolFastRewardView>(
		        new PatrolFastRewardViewModel()).Forget();
        }
		/// <summary>
		/// 领取
		/// </summary>
        [Command]
        private async void OnClickGettingBtn()
        {
	        if (GetAwardGary)return;
	        var req = new ReqMainHangupClaim();
	        req.Op = 1;
	        var result = await GameNetworkManager.Instance.SendAsync<RspMainHangupClaim>(req);
	        if (NetHelper.CheckNetErrorMessage(result.rsp, true))
	        {
				Lodash.DealRewards(result.rsp.Reward.ToList());
				UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList());
				Data.ClaimedTime = ServerTime.Instance.GetNowTime();
				if (MinEquip!=-1)
					Data.HeroEquipDailyUnit += MinEquip;
				InitRewardCount();
				TimeDes();
	        }
        }

        [Command]
        private void OnClickGoTOCard()
        {
	        UIManager.Instance.OpenDialog<MonthCardView,MonthCardViewModel>().Forget();
        }
        

        #endregion



        #region 收益奖励

        private int MinEquip = -1;
        void InitRewardCount()
        {
	        GoldNumsTextStr = (RewardCount1) + "/" +LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime02);//*Data.GetPatrolEarningsAttr()
	        PowerTextStr =  (RewardCount2) + "/" +LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime02);//*Data.GetPatrolEarningsAttr()

	        IsShowCardView = !DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.PatrolGains15);
	        
	        var adNums = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.PatrolIncome_06).Content[0];
	        var powerNums = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.PatrolIncome_03).Content[0];
	        AllLastNums = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips24) + (Data.FreeCount + Data.AdFreeCount);//+ "/"+ (adNums+powerNums + Data.MonthAddPatrolNums());
	        
	        ScrollViewList.Clear();
	        
	        var hours = Math.Max((int)Data.GetTimesPatrol()/patrolCompletion, 1);
	        for (int i = 0; i < CopyThreadCfg.Hangup.Count; i++)
	        {
		        var AwardItem = new Reward(CopyThreadCfg.Hangup[i].Type,CopyThreadCfg.Hangup[i].Id,(CopyThreadCfg.Hangup[i].Count*hours*Data.GetPatrolEarningsAttr())/100);
		        ScrollViewList.Add(CellItemBaseViewModel.Create(AwardItem,ECellItemSizeType.Size150X135));
	        }
	        
	        if (CopyThreadCfg.Special is {Count: > 0})
		        for (int i = 0; i < CopyThreadCfg.Special.Count; i++)
		        {
			        var maxnums = Math.Min( CopyThreadCfg.Special[i].Count * hours, CopyThreadCfg.SpecialLimit[i]);
			        var AwardItem = new Reward(CopyThreadCfg.Special[i].Type,CopyThreadCfg.Special[i].Id,maxnums);
			        ScrollViewList.Add(CellItemBaseViewModel.Create(AwardItem,ECellItemSizeType.Size150X135));
		        }

	        MinEquip = -1;
	        if (CopyThreadCfg.HeroEquip is {Count: > 0} )
		        for (int i = 0; i < CopyThreadCfg.HeroEquip.Count; i++)
		        {
			        if (Data.HeroEquipDailyUnit >= CopyThreadCfg.HeroEquipLimit[1])
						break;
			        var temp = CopyThreadCfg.HeroEquipLimit[1] - Data.HeroEquipDailyUnit;
			        MinEquip = Math.Min(hours, temp);
			        var maxnums = CopyThreadCfg.HeroEquip[i].Count * MinEquip;
			        var AwardItem = new Reward(CopyThreadCfg.HeroEquip[i].Type,CopyThreadCfg.HeroEquip[i].Id,maxnums);
			        ScrollViewList.Add(CellItemBaseViewModel.Create(AwardItem,ECellItemSizeType.Size150X135));
		        }
	        
	        
	        MonthCardScrollViewList.Clear();
	        
	        for (int i = 0; i < CopyThreadCfg.Hangup.Count; i++)
	        {
		        var AwardItem = new Reward(CopyThreadCfg.Hangup[i].Type,CopyThreadCfg.Hangup[i].Id,(CopyThreadCfg.Hangup[i].Count*hours*Data.GetPatrolEarningsAttr(true))/100);
		        MonthCardScrollViewList.Add(CellItemBaseViewModel.Create(AwardItem,ECellItemSizeType.Size120X100));
	        }
	        
        }
        public int RewardCount1
        {
	        get
	        {
		        var per = 1;
		        if (CopyThreadCfg != null && CopyThreadCfg.Hangup.Count > 0) per = (int)CopyThreadCfg.Hangup[0].Count;
		        var count = per;
		        return count;
	        }
        }
        
        public int RewardCount2
        {
	        get
	        {
		        var per = 1;
		        if (CopyThreadCfg != null && CopyThreadCfg.Hangup.Count > 0) per = (int)CopyThreadCfg.Hangup[1].Count;
		        var count = per;
		        return count;
	        }
        }
        
        #endregion
        
        #region 倒计时
        [AutoNotify] private bool getAwardGary;
        private int nowIndex = -1;
        public void TimeDes()
        {
	        GetAwardGary = Data.GetTimesPatrol() < patrolCompletion;
	        TimesDesStr = ServerTime.Instance.SecondsDHms(Data.GetTimesPatrol());
	        var temp = (int)(Data.GetTimesPatrol() / patrolCompletion);
	        if (nowIndex == -1)
	        {
		        nowIndex = temp;
	        }
	        else
	        {
		        if (nowIndex == temp) return;
		        nowIndex = temp;
		        InitRewardCount();
	        }
        }
        private float time;
        public override void Update()
        {
	        base.Update();
	        if (!UIHelper.CalculateTime(ref time)) return;
	        TimeDes();
	        RedGo = Data.IsRed() && MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctioonTypeFarm);;
        }

        #endregion

        private void MonthCardChange(object sender, PropertyChangedEventArgs e)
        {
	        InitRewardCount();
        }
        private void SecondDay(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(PlayerInfoManager.Instance.SecondDay))
				InitRewardCount();
        }

        public void CloseUI()
        {
	        UIManager.Instance.CloseDialog<PatrolRewardView>();
        }

        protected override void OnDispose()
        {
	        DataCenter.monthCardData.PropertyChanged -= MonthCardChange;
	        Data.PropertyChanged -= MonthCardChange;
	        PlayerInfoManager.Instance.PropertyChanged -= SecondDay;
	        base.OnDispose();
        }

    }
}