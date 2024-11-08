using System;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using Spine.Unity;
using UnityEngine;
using Object = System.Object;


namespace DH.Game.ViewModels
{
	
	public partial class LuckDrawViewModel : ViewModelBase
    {
		[AutoNotify] private CommonTopViewModel commonTopItemsVm;
		[AutoNotify] private ObservableList<LuckProgressAwardCellViewModel> rewardScrollViewList = new();
		[AutoNotify] private int rewardScrollViewJumpIndex;
		[AutoNotify] private ItemPriceNodeModel leftPriceViewVm;
		[AutoNotify] private ItemPriceNodeModel rightPriceViewVm;
		[AutoNotify] private string adTipsStr;
		[AutoNotify] private BottomComponentViewModel bottomComponentVm;
		[AutoNotify] private bool isShowSkipBtnIcon;
		[AutoNotify] private ObservableDictionary<int, LuckDrawCellViewModel> randomInfoDictionary = new();
		[AutoNotify] private string leftCountStr;
		[AutoNotify] private string progressStr;
		[AutoNotify] private string timeStr;
		[AutoNotify] private string adBtnImgPath;
		[AutoNotify] private bool isCanOpAdBtn;
		[AutoNotify] private SkeletonGraphic actionSkele;
		[AutoNotify] private Transform numberEffectNode;
		[AutoNotify] private string leftOpBtnImgPath;
		[AutoNotify] private bool isCanOpLeftBtn;
		[AutoNotify] private string rightOpBtnImgPath;
		[AutoNotify] private bool isCanOpRightBtn;
		public CommonAdvIconViewModel CommonAdvVm;
		public Func<Object, object> GetInfoCellVmCallBack => GetRandomCellVmByIndex;
		private int  curActionIndex = 0;
		private int totalProgress;
		

        [Preserve]
        public LuckDrawViewModel()
        {
	        List<GameConst.ItemIdCode> list = new(){GameConst.ItemIdCode.Stone};
	        CommonTopItemsVm = new(list);
	        BottomComponentVm = new(OnClickCloseBtn);
	        var cfgs = ConfigCenter.StageRewardCfgColl.DataItems;
	        List<StageRewardCfg> cfgList = new();
	        foreach (var cfg in cfgs)
	        {
		        if(cfg.Type != 1) continue;
		        cfgList.Add(cfg);
	        }
	        cfgList.Sort((a, b) => a.Level - b.Level);
	        // 进度奖励
	        for (int i = 0; i < cfgList.Count; ++i)
	        {
		        LuckProgressAwardCellViewModel tempVm= new(cfgList[i], OnClickRecordItem,i == cfgList.Count - 1,EDrawState.DrawLucky);
		        RewardScrollViewList.Add(tempVm);
		        totalProgress = cfgList[i].Level;
	        }
	        // 抽奖奖励
	        var activeCfg = ConfigCenter.ActivityCfgColl.GetDataById((int)EActivityType.ActivityTypeLuckyDraw);
	        if (activeCfg is { JackpotId: { Count: > 0 } })
	        {
		        var jackpotId = activeCfg.JackpotId[0];
		        var jackpotCfg = ConfigCenter.JackpotCfgColl.GetDataById(jackpotId);
		        if (jackpotCfg is { RandomReward: { Count: > 0 } })
		        {
			        for (int i = 0; i < jackpotCfg.RandomReward.Count; ++i)
			        {
				        var itemId = jackpotCfg.RandomReward[i];
				        LuckDrawCellViewModel tempVm = new(i, itemId,true);
				        RandomInfoDictionary.Add(i, tempVm);
			        }
		        }
	        }

	        UpdateRecordJumpIndex();
	        
	        DataCenter.luckyDrawData.ClaimRecord.CollectionChanged += OnClaimRecordChanged;
	        DataCenter.luckyDrawData.PropertyChanged += OnLuckyDrawDataChanged;

	        UpdateSkipState();
	        UpdateInfo();
	        UpdateTimeStr();
	        UpdateCostInfo();
	        UpdateBtnState();
	        CommonAdvVm = new CommonAdvIconViewModel();
        }


        private void UpdateCostInfo()
        {
	        var leftCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Turntable_01);
	        if(leftCfg !=null && leftCfg.Reward!=null && leftCfg.Reward.Count>0)
	        {
		        var item = leftCfg.Reward[0];
		        LeftPriceViewVm = new(item);
		        LeftPriceViewVm.IsShowBg = false;
	        }
	        
	        var rightCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Turntable_02);
	        if (rightCfg != null && rightCfg.Reward != null && rightCfg.Reward.Count > 0)
	        {
		        var item = rightCfg.Reward[0];
		        RightPriceViewVm = new(item);
		        RightPriceViewVm.IsShowBg = false;
	        }
        }

        protected override void OnDispose()
        {
	        DataCenter.luckyDrawData.PropertyChanged -= OnLuckyDrawDataChanged;
	        DataCenter.luckyDrawData.ClaimRecord.CollectionChanged -= OnClaimRecordChanged;
	        CommonAdvVm?.Dispose();
	        base.OnDispose();
        }

        public override void Update()
        {
	        base.Update();
	        if(UIHelper.CalculateTime(ref curTime)) return;
	        UpdateTimeStr();
        }

        private void OnLuckyDrawDataChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(DataCenter.luckyDrawData.Progress) ||
	            e.PropertyName == nameof(DataCenter.luckyDrawData.DrawDayTimes))
	        {
		        // 更新进度
		        UpdateRecordListState();
		        UpdateInfo();
	        }
        }

        private void OnClaimRecordChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
	        // 更新进度
	        UpdateRecordListState();
        }

        public void UpdateRecordListState()
        {
	        foreach (var item in RewardScrollViewList)
	        {
		        item.UpdateState();
	        }
	        UpdateRecordJumpIndex();
        }

        private void UpdateRecordJumpIndex()
        {
	        RewardScrollViewJumpIndex = 0;
	        for (int i = 0; i < RewardScrollViewList.Count; i++)
	        {
		        var item = RewardScrollViewList[i];
		        if (item.CurState == ECellItemState.GetIng || item.CurState == ECellItemState.None)
		        {
			        RewardScrollViewJumpIndex = i;
			        break;
		        }
	        }
        }

        private void UpdateBtnState()
        {
	        if (!DataCenter.luckyDrawData.CheckIsCanOp(ELuckDrawOpType.LuckDrawOpOneTimes))
	        {
		        LeftOpBtnImgPath = "common[common_button_grey]";
		        IsCanOpLeftBtn = false;
	        }
	        else
	        {
		        LeftOpBtnImgPath = "common[commom_button_blue]";
		        IsCanOpLeftBtn = true;
	        }

	        if (!DataCenter.luckyDrawData.CheckIsCanOp(ELuckDrawOpType.LuckDrawOpFiveTimes))
	        {
		        RightOpBtnImgPath = "common[common_button_grey]";
		        IsCanOpRightBtn = false;
	        }
	        else
	        {
		        RightOpBtnImgPath = "common[commom_button_yellow]";
		        IsCanOpRightBtn = true;
	        }
	        
	        if (!DataCenter.luckyDrawData.IsClickAdBtn())
	        {
		        AdBtnImgPath = "common[common_button_grey3]";
		        IsCanOpAdBtn = false;
	        }
	        else
	        {
		        AdBtnImgPath = "turntable[turntable_btn_1]";
		        IsCanOpAdBtn = true;
	        }
	        
        }

        private void UpdateSkipState()
        {
	        IsShowSkipBtnIcon = DataCenter.luckyDrawData.IsSkip;
        }

        private void UpdateInfo()
        {
	        var tipsStr = LocalizeHelper.GetGlobal(GlobalLanguageId.xingyun_02);
	        var endCount = DataCenter.luckyDrawData.GetCurDayEndCount();
	        LeftCountStr = $"{tipsStr}{endCount}";
            
	        var tempPro = DataCenter.luckyDrawData.Progress;
	        ProgressStr = $"<color=#FFED29><size=50>{tempPro}/</size></color><color=#F5F2CC><size=24>{totalProgress}</size></color>";
        }

        private float curTime = 0;
        private void UpdateTimeStr()
        {
	        var endTime = ServerTime.Instance.RemainTime(DataCenter.luckyDrawData.EndStamp);
	        if (endTime <= 0)
	        {
		        UIManager.Instance.CloseDialog<LuckDrawView>();
	        }
	        if (endTime >= 86400)
	        {
		        TimeStr = UIHelper.ConvertTimeSecondToString(endTime, ETimeFormatType.TimeFormatTypeHourMinuteWithUnit);
	        }
	        else
	        {
		        TimeStr =  ServerTime.Instance.Seconds2Hhmmss(endTime); 
	        }
        }

        [Command]
        private void OnClickInfoBtn()
        {
	        var ruleStr = LocalizeHelper.GetGlobal(GlobalLanguageId.xingyun_08);
	        List<LimitCellRatioData> limitList = new();
	        List<LimitCellRatioData> normalList = new();
	        var activeCfg = ConfigCenter.ActivityCfgColl.GetDataById((int)EActivityType.ActivityTypeLuckyDraw);
	        if (activeCfg is { JackpotId: { Count: > 0 } })
	        {
		        var jackpotId = activeCfg.JackpotId[0];
		        var jackpotCfg = ConfigCenter.JackpotCfgColl.GetDataById(jackpotId);
		        if (jackpotCfg is { RandomReward: { Count: > 0 } })
		        {
			        for (int i = 0; i < jackpotCfg.RandomReward.Count; ++i)
			        {
				        var item = jackpotCfg.RandomReward[i];
				        var tempReward = new Reward(item.Type, item.Id, item.Count);
				        UIHelper.GetRewardInfo(tempReward,out string nameStr,out string descStr);
				        var ratioStr = UIHelper.ParseValueToStringByType(item.Weight, GameConst.ENumType.PercentTypeValue,1,false,2);
				        LimitCellRatioData tempData = new(tempReward, ratioStr, normalList.Count);
				        normalList.Add(tempData);
			        }
		        }
	        }
	        
	        ActivityRuleAndRatioData curData = new(ruleStr, limitList,normalList);
	        ActivityRuleAndRatioViewModel tempVm = new(curData);
	        UIManager.Instance.OpenDialog<ActivityRuleAndRatioView>(tempVm).Forget();
        }

        [Command]
        private void OnClickLeftOpBtn()
        {
	        // 检查是否在有效期
	        if (!DataCenter.luckyDrawData.CheckIsValid())
	        {
		        DHLog.Debug("活动结束");
		        return;
	        }

	        // 检查资产是否足够
	        var costCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Turntable_01);
	        if(costCfg ==null || costCfg.Reward ==null || costCfg.Reward.Count == 0)
	        {
		        DHLog.Debug("抽奖配置错误");
		        return;
	        }

	        // 检查资产是否足够
	        if (!UIHelper.CheckRewardIsEnough(costCfg.Reward[0], true))
	        {
		        return;
	        }
	        RequestLuckyDraw(ELuckDrawOpType.LuckDrawOpOneTimes).Forget();
        }

        [Command]
        private void OnClickRightOpBtn()
        {
	        // 检查是否在有效期
	        if (!DataCenter.luckyDrawData.CheckIsValid())
	        {
		        DHLog.Debug("活动结束");
		        return;
	        }

	        // 检查资产是否足够
	        var costCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Turntable_02);
	        if(costCfg ==null || costCfg.Reward ==null || costCfg.Reward.Count == 0)
	        {
		        DHLog.Debug("抽奖配置错误");
		        return;
	        }

	        // 检查资产是否足够
	        if (!UIHelper.CheckRewardIsEnough(costCfg.Reward[0], true))
	        {
		        return;
	        }
	        
	        RequestLuckyDraw(ELuckDrawOpType.LuckDrawOpFiveTimes).Forget();
        }

        [Command]
		private void OnClickAdBtn()
		{
			// 检查是否在有效期
			if (!DataCenter.luckyDrawData.CheckIsValid())
			{
				DHLog.Debug("活动结束");
				return;
			}
			
			if(!DataCenter.luckyDrawData.IsClickAdBtn())
			{
				DHLog.Debug("广告次数上");
				return;
			}
			
			UIHelper.ShowRewardAds(() =>
			{
				RequestLuckyDraw(ELuckDrawOpType.LuckDrawOpAd).Forget();
			});
		}

		[Command]
		private void OnClickSkipBtn()
		{
			DataCenter.luckyDrawData.IsSkip = !DataCenter.luckyDrawData.IsSkip;
			UpdateSkipState();
		}

		private void OnClickCloseBtn()
		{
			UIManager.Instance.CloseDialog<LuckDrawView>();
		}
		
		private object GetRandomCellVmByIndex(object index)
		{
			if (RandomInfoDictionary.TryGetValue((int)index, out LuckDrawCellViewModel ret))
			{
				return ret;
			}
			return null;
		}

		private async UniTaskVoid RequestLuckyDraw(ELuckDrawOpType opType)
		{
			ChangeAllOpBtnClick(false);
			ReqLuckyDraw req = new();
			req.Op = (int)opType;
			var result = await GameNetworkManager.Instance.SendAsync<RspLuckyDraw>(req);
			if (result.rsp is not { Status: 0 })
			{
				if(result.rsp == null) return;
				var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
				ToastManager.Show(str);
				ChangeAllOpBtnClick(true);
				UpdateBtnState();
				return;
			}
			
			// 给表现 给奖励 扣钱
			// 1 先扣钱
			Lodash.DealRewards(result.rsp.Costs.ToList(), false);
			// 更新进度
			DataCenter.luckyDrawData.Progress = result.rsp.Progress;
			// 更新次数
			DataCenter.luckyDrawData.DrawDayTimes += ELuckDrawOpType.LuckDrawOpFiveTimes == opType ? 5 : 1;
			
			if (ELuckDrawOpType.LuckDrawOpAd == opType)
			{
				DataCenter.luckyDrawData.AdCount += 1;
			}
		

			if(DataCenter.luckyDrawData.IsSkip)
			{
				// 2 更新数据
				Lodash.DealRewards(result.rsp.Rewards.ToList());
				
				UIEffectManager.Instance.PlayLuckDrawFlyEffect(result.rsp.Ps.ToList(), numberEffectNode,
					() =>
					{
						ChangeAllOpBtnClick(true);
						UpdateBtnState();
						// 直接给奖励动画
						UIHelper.OpenCommonRewardView(result.rsp.Rewards.ToList());
					}).Forget();
			}
			else
			{
				// 播动画
				PlayeLuckDrawAction(result.rsp.Rewards.ToList(), result.rsp.Index.ToList(), opType, result.rsp.Ps.ToList()).Forget();
			}
		}

		private async void OnClickRecordItem(int cfgId)
		{
			var list = DataCenter.luckyDrawData.GetCanClaimIdList();
			ReqLuckyClaim req = new();
			foreach (var id in list)
			{
				req.Ids.Add(id);
			}
		
			var result = await GameNetworkManager.Instance.SendAsync<RspLuckyClaim>(req);
			if (result.rsp is not { Status: 0 })
			{
				if(result.rsp == null) return;
				var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
				ToastManager.Show(str);
				return;
			}
			// 直接给奖励
			UIHelper.OpenCommonRewardView(result.rsp.Rewards.ToList());
			// 1 更新数据
			Lodash.DealRewards(result.rsp.Rewards.ToList());
			DataCenter.luckyDrawData.UpdateRecordList(list);
			// 2 刷新状态
			UpdateRecordListState();
			
		}

		private void ChangeAllOpBtnClick(bool isCanOp)
		{
			IsCanOpAdBtn = isCanOp;
			IsCanOpLeftBtn = isCanOp;
			IsCanOpRightBtn = isCanOp;
		}

		/// <summary>
		/// 播动画
		/// </summary>
		/// <param name="rewards"></param>
		/// <param name="index"></param>
		private async UniTaskVoid PlayeLuckDrawAction(List<Resource> rewards, List<int> index, ELuckDrawOpType opType, List<int> posList)
		{
			curActionIndex = 0;
			var actionName = "wulian";
			int delayTime = 2000;
			if (opType != ELuckDrawOpType.LuckDrawOpFiveTimes)
			{
				actionName = "danchou";
				delayTime = 1767;
				AudioManager.Instance.Play(AudioType.LuckyOnce);
			}
			else
			{
				AudioManager.Instance.Play(AudioType.LuckyFive);
			}

			// 2 更新数据
			Lodash.DealRewards(rewards);
			// TO-DO 播放动画 给奖励 更新数据
			if (actionSkele != null)
			{
				
				actionSkele.AnimationState.SetAnimation(0, actionName, false);
				await UniTask.Delay(delayTime);
				UIEffectManager.Instance.PlayLuckDrawFlyEffect(posList,
					numberEffectNode,
					() =>
					{
						ChangeAllOpBtnClick(true);
						UpdateBtnState();
						// 直接给奖励
						UIHelper.OpenCommonRewardView(rewards);
					}).Forget();
			}
			else
			{
				ChangeAllOpBtnClick(true);
				UpdateBtnState();
				// 直接给奖励
				UIHelper.OpenCommonRewardView(rewards);
			}
		}
    }
}