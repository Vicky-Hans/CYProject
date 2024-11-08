using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHUnityUtil;
using Spine;
using Spine.Unity;
using Random = UnityEngine.Random;


namespace DH.Game.ViewModels
{
    public partial class LuckEggDrawViewModel : ViewModelBase
    {
	    public override bool AutoDispose => true;
	    private string SkipTips = "LuckEggSkip";
	    
		[AutoNotify] private string timeDescStr;
		private float IntervalTime = 0;
		[AutoNotify] private bool isSkipAnimation;
		[AutoNotify] private ItemPriceNodeModel oneBtnPriceViewVm;
		[AutoNotify] private ItemPriceNodeModel tenBtnPriceViewVm;
		[AutoNotify] private int totalProgress;
		[AutoNotify] private string rewardAllProgressStr;
		 [AutoNotify] private ObservableList<LuckEggDrawTopProgressViewModel> rewardScrollViewList = new();
		[AutoNotify] private string limitDrawCntStr;
		[AutoNotify] private bool isShowSkeletonGraphic;
		private bool drawIng;

		public bool DrawIng
		{
			get => drawIng;
			set
			{
				Set(ref drawIng, value);
				ActivityUIManager.Instance.LuckEggDrawIng = drawIng;
			}
		}
		[AutoNotify] private CommonTopViewModel commonTopViewModel;

		[AutoNotify] private bool isDrawOne;
		[AutoNotify] private bool isDrawTen;

		[AutoNotify] private int startPos;
		public SkeletonGraphic skeletonGraphic;

		public SkeletonGraphic SkeletonGraphic
		{
			get => skeletonGraphic;
			set
			{
				var old = skeletonGraphic;
				skeletonGraphic = value;
				if (old != null)
				{
					old.AnimationState.Complete -= AniamtionEnd;
				}
				if (skeletonGraphic != null)
				{
					skeletonGraphic.AnimationState.Complete += AniamtionEnd;
				}
			}
		}
		
		private List<Resource> drawBackRsp;

		[AutoNotify] private int randomShowType;
		[AutoNotify] private bool startMoveAnimation;
        [Preserve]
        public LuckEggDrawViewModel()
        {
	        StartPos = GetTopIndexPos();
	        IsSkipAnimation = PlayerPrefs.GetInt(UIHelper.GetRoleTips(SkipTips)) == 1;
	        CommonTopViewModel = UIHelper.GetTopModel(GameConst.ItemIdCode.EggCoin,GameConst.ItemIdCode.EggRedHeart,GameConst.ItemIdCode.Stone);
	        CommonTopViewModel.ClickItemAction = (data, id) =>
	        {
		        if (data.Id == (int)GameConst.ItemIdCode.EggCoin)
		        {
			        ActivityUIManager.Instance.OpenBuyEggCoin();
		        }
	        };
	        RefreshTime();
	        InitBtnCntChange();
	        RefreshLimitCnt();
	        RefreshProgressReward();
	        RefreshDrawState();
	        DataCenter.itemsData.OnItemUpdate += ItemUpdate;
	        DataCenter.luckyEggData.PropertyChanged += DataChanged;
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        DataCenter.itemsData.OnItemUpdate -= ItemUpdate;
	        DataCenter.luckyEggData.PropertyChanged -= DataChanged;
	        DrawIng = false;
        }

        private void DataChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(LuckyEggData.DayDrawCount))
	        {
		        RefreshDrawState();
		        RefreshLimitCnt();
	        }else if (e.PropertyName == nameof(LuckyEggData.Count))
	        {
		        RefreshProgressValue();
	        }

        }

        private void ItemUpdate(ResourceData data)
        {
	        RefreshDrawState();
        }

        private void InitBtnCntChange()
        {
	        OneBtnPriceViewVm = new ItemPriceNodeModel(new Reward(RewardType.Item, (int)GameConst.ItemIdCode.EggCoin,1));
	        TenBtnPriceViewVm = new ItemPriceNodeModel(new Reward(RewardType.Item, (int)GameConst.ItemIdCode.EggCoin,10));
        }

        private void RefreshDrawState()
        {
	        var limit = UIHelper.GetDefinesInt(DefineCfgId.Gachapon_01);
	        IsDrawOne = limit >= DataCenter.luckyEggData.DayDrawCount+1;
	        IsDrawTen = limit >= DataCenter.luckyEggData.DayDrawCount+10;
        }

        private void RefreshLimitCnt()
        {
	        var cfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Gachapon_01);
	        LimitDrawCntStr = $"{LocalizeHelper.GetGlobal(GlobalLanguageId.xingyun_02)}{cfg.Content[0] - DataCenter.luckyEggData.DayDrawCount}";
        }
        
        private void RefreshProgressReward()
        {
	        RewardScrollViewList.ClearAndDispose();
	        var list = UIHelper.GetAllStageRewardList(ActivityStageType.LuckEgg);
	        for (int i = 0; i < list.Count; i++)
	        {
		        RewardScrollViewList.Add(new LuckEggDrawTopProgressViewModel(list[i],i==list.Count-1));
		        TotalProgress = list[i].Level;
	        }

	        RefreshProgressValue();
        }
        
        private void RefreshProgressValue()
        {
	        var tempPro = DataCenter.luckyEggData.Count;
	        RewardAllProgressStr = $"<color=#FFED29><size=50>{tempPro}/</size></color><color=#F5F2CC><size=24>{totalProgress}</size></color>";
        }
        
        private void RefreshTime()
        {
	        var times = Math.Max(0,
		        DataCenter.luckyEggData.EndStamp - ServerTime.Instance.GetNowTime());
	        TimeDescStr =  ServerTime.Instance.SecondsDHAndMS(times);
        }
        
        public override void Update()
        {
	        if (UIHelper.CalculateTime(ref IntervalTime))
	        {
		        RefreshTime();
		        if (!ActivityUIManager.Instance.CheckLuckEggShowTime())
		        {
			        UIManager.Instance.CloseDialog<LuckEggMainView>();
		        }
	        }
        }
        
        private int GetTopIndexPos()
        {
	        var list = UIHelper.GetAllStageRewardList(ActivityStageType.LuckEgg);
	        for (int i = 0; i < list.Count; i++)
	        {
		        if (!DataCenter.luckyEggData.CheckProgressFinish(list[i].Id) )
		        {
			        return i;
		        }
	        }
	        return list.Count;
        }


    


        [Command]
        private void OnClickBtnTips()
        {
	        var activityCfg  = ConfigCenter.ActivityCfgColl.GetDataById((int)EActivityType.ActivityLuckEgg);
	        UIHelper.OpenCommonRuleProbability(LocalizeHelper.GetGlobal(GlobalLanguageId.niudan06),activityCfg?.JackpotId[0] ?? 0);
        }

		[Command]
		private void OnClickSkipBtn()
		{
			if(DrawIng) return;
			IsSkipAnimation = !isSkipAnimation;
			PlayerPrefs.SetInt(UIHelper.GetRoleTips(SkipTips),IsSkipAnimation?1:0);
		}

		[Command]
		private void OnClickBtnDrawFree()
		{
			
		}

		[Command]
		private void OnClickBtnDrawOne()
		{
			if (!UIHelper.CheckRewardIsEnough(OneBtnPriceViewVm.Reward))
			{
				ActivityUIManager.Instance.OpenBuyEggCoin();
				return;
			}

			SendEggDraw(1);
		}

		[Command]
		private void OnClickBtnDrawOneUnUse()
		{
		}

		[Command]
		private void OnClickBtnDrawBatch()
		{
			if (!UIHelper.CheckRewardIsEnough(TenBtnPriceViewVm.Reward))
			{
				ActivityUIManager.Instance.OpenBuyEggCoin();
				return;
			}
			SendEggDraw(2);
		}

		[Command]
		private void OnClickBtnDrawBatchUnUse()
		{
			
		}

		private void SendEggDraw(int op)
		{
			DrawIng = true;
			ActivityUIManager.Instance.SendLuckEggDraw(op, (rewards) =>
			{
				if (!IsSkipAnimation && SkeletonGraphic != null)
				{
					drawBackRsp = rewards;
					SkeletonGraphic.AnimationState.SetAnimation(0, "jiqi", false);
				}
				else
				{
					DealDrawResult(rewards);
				}
			}, () =>
			{
				DrawIng = false;
			}).Forget();
		}

		private void AniamtionEnd(TrackEntry trackentry)
		{
			if (trackentry.Animation.Name == "jiqi")
			{
				RandomShowType = Random.Range(1, 5);
				SkeletonGraphic.AnimationState.SetAnimation(0, "chuqiu_"+randomShowType, false);
			}
			else
			{
				DealDrawResult(drawBackRsp);
			}
		}

		private void DealDrawResult( List<Resource> rewards)
		{
			if(rewards==null) return;
			DrawIng = false;
			RefreshLimitCnt();
			UIHelper.OpenCommonRewardView(rewards, () =>
			{
				//奖励关闭回调'
				startMoveAnimation = false;
				StartMoveAnimation = true;
			});
		}
    }
}