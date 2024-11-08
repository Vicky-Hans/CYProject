using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using DHUnityUtil;
using Spine;
using Spine.Unity;

namespace DH.Game.ViewModels
{
    public partial class LuckTravelViewModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        [AutoNotify] private string endTimeValueStr;
        private float IntervalTime = 0;
        public Func<object, object> GetSuperRewardCellCallback => GetSuperRewardListCellCallbackByIndex;
        [AutoNotify] private ObservableDictionary<int,CellItemBaseViewModel> superRewardList = new();
        
        [AutoNotify] private ObservableList<LuckTravelTipsItemViewModel> scrollViewInfoList = new();
        public FlipPageCircularScrollView ScrollViewInfo;
        private float IntervalTimeTips = 0;

        public Func<object, object> GetRewardCellCallback => GetScrollViewRewardListCellCallbackByIndex;
        [AutoNotify] private ObservableDictionary<int,LuckTravelDrawItemViewModel> scrollViewRewardList = new();
        [AutoNotify] private bool isSkipAnimation;
        [AutoNotify] private bool drawIng;
        [AutoNotify] private int countTextCnt = 1;//转动次数
        [AutoNotify] private ItemPriceNodeModel itemPriceNodeModel;
        [AutoNotify] private bool isCanFreeDraw;
        [AutoNotify] private bool isShowProgree;
        [AutoNotify] private string rewardAllProgressStr;

        [AutoNotify] private ObservableList<LuckTravelProgressAwardItemViewModel> rewardScrollViewList = new();
        [AutoNotify] private int totalProgress;
        [AutoNotify] private BottomComponentViewModel bottomComponentViewModel;
        [AutoNotify] private CommonAdvIconViewModel commonAdvIconViewModel=new ();
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

        private RspLuckyTrip drawBackRsp;

        private UICircularScrollView rewardScrollView;

        public UICircularScrollView RewardScrollView
        {
            get => rewardScrollView;
            set
            {
                rewardScrollView = value;
            }
        }

        [AutoNotify] private int topIndex;

        [AutoNotify] private string limitDrawCntStr;
        [AutoNotify] private CommonTopViewModel topViewModel;
        [AutoNotify] private bool freeDrawRed;
        [AutoNotify] private bool progressRed;

        private string SkipTips = "LuckTravelSkip";
        private string LuckTravelCnt = "LuckTravelDraw";

        [AutoNotify] private bool batchCanDraw;
        [Preserve]
        public LuckTravelViewModel()
        {
            bottomComponentViewModel = new BottomComponentViewModel(() =>
            {
                UIManager.Instance.CloseDialog<LuckTravelView>();
            });
            topViewModel = new CommonTopViewModel(new List<GameConst.ItemIdCode>()
            {
                GameConst.ItemIdCode.Stone
            });
            RefreshRewardList();
            RefreshTipsList();
            RefreshTime();
            RefreshFreeDrawState();
            RefreshProgressReward();
            RefreshBtnCntChange();
            RefreshLimitCnt();
            RefreshRed();
            TopIndex = GetTopIndexPos();
            IsSkipAnimation = PlayerPrefs.GetInt(UIHelper.GetRoleTips(SkipTips)) == 1;
            CountTextCnt = PlayerPrefs.GetInt(UIHelper.GetRoleTips(LuckTravelCnt),1);
            RefreshDrawCnt();
            DataCenter.luckyTravelData.PropertyChanged += DataPropertyChanged;
            DataCenter.luckyTravelData.ClaimRecord.CollectionChanged += ClaimRecordCollectionChanged;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            DataCenter.luckyTravelData.PropertyChanged -= DataPropertyChanged;
            DataCenter.luckyTravelData.ClaimRecord.CollectionChanged -= ClaimRecordCollectionChanged;
        }

        private void ClaimRecordCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
             RefreshRed();
        }

        private void DataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LuckyTravelData.AdCount))
            {
                RefreshFreeDrawState();
            }else if (e.PropertyName == nameof(LuckyTravelData.Progress))
            {
                RefreshProgressValue();
            }
            RefreshRed();
            RefreshDrawCnt();
            RefreshLimitCnt();
        }

        private void RefreshRed()
        {
            FreeDrawRed = ActivityUIManager.Instance.CheckFreeBuyRed();
            ProgressRed = ActivityUIManager.Instance.CheckLuckTravelProgressRed();
        }

        private void RefreshLimitCnt()
        {
            var cfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.LuckyJourney_04);
            LimitDrawCntStr = $"{LocalizeHelper.GetGlobal(GlobalLanguageId.xingyun_02)}{cfg.Content[0] - DataCenter.luckyTravelData.TodayDraw}";
        }

        private void RefreshProgressReward()
        {
            RewardScrollViewList.ClearAndDispose();
            var list = UIHelper.GetAllStageRewardList(ActivityStageType.LuckTravel);
            for (int i = 0; i < list.Count; i++)
            {
                RewardScrollViewList.Add(new LuckTravelProgressAwardItemViewModel(list[i],i==list.Count-1));
                TotalProgress = list[i].Level;
            }

            RefreshProgressValue();
        }
        
        private void RefreshProgressValue()
        {
            var tempPro = DataCenter.luckyTravelData.Progress;
            RewardAllProgressStr = $"<color=#FFED29><size=50>{tempPro}/</size></color><color=#F5F2CC><size=24>{totalProgress}</size></color>";
        }

        private void RefreshFreeDrawState()
        {
            IsCanFreeDraw = DataCenter.luckyTravelData.AdCount == 0;
        }

        private void RefreshRewardList()
        {
            ScrollViewRewardList.Clear();
            var defineCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.LuckyJourney_13);
            var jackpotCfg = ConfigCenter.JackpotCfgColl.GetDataById(defineCfg.Content[0]);
            if (jackpotCfg != null)
            {
                for (int i = 0; i < jackpotCfg.RandomReward.Count; i++)
                {
	                ScrollViewRewardList.Add(i,new LuckTravelDrawItemViewModel(UIHelper.RandomRewardToReward(jackpotCfg.RandomReward[i])));
                }
            }
        }

        private void RefreshTipsList()
        {
            ActivityUIManager.Instance.SendLuckTravelTipseRank((list,superRewardList) =>
            {
                scrollViewInfoList.ClearAndDispose();
                DataCenter.luckyTravelData.SuperRewardNum = list.Count;
                for (int i = 0; i < list.Count; i++)
                {
	                scrollViewInfoList.Add(new LuckTravelTipsItemViewModel(list[i],i));
                }
                if(ScrollViewInfo!=null)
                    ScrollViewInfo.Refresh();

                RefreshTipsListPos().Forget();
                SuperRewardList.Clear();
                for (int i = 0; i < superRewardList.Count; i++)
                {
                    var model = CellItemBaseViewModel.Create(superRewardList[i]);
                    model.BgPath = $"lucky[lucky_limitpanel_4]";
                    SuperRewardList.Add(i,model);
                }
                
            }, () =>
            {
                scrollViewInfoList.ClearAndDispose();
            }).Forget();
          
        }

        private async UniTaskVoid RefreshTipsListPos()
        {
            await UniTask.Delay(100);
            if (ScrollViewInfo != null)
            {
                var temp = scrollViewInfoList.Count - 4;
                ScrollViewInfo.CurPageIndex = Math.Max(temp,0);
                DHLog.Debug($"CurTipsInfoSelectIndex Set:  RefreshTipsListPos{temp}");
                // if (scrollViewInfoList.Count >= 4)
                // {
                //     ScrollViewInfo.CurPageIndex = scrollViewInfoList.Count-4;
                //     DHLog.Debug($"CurTipsInfoSelectIndex Set:  RefreshTipsListPos{scrollViewInfoList.Count-4}");
                // }
                // else
                // {
                //     ScrollViewInfo.CurPageIndex = 0;
                //     DHLog.Debug($"CurTipsInfoSelectIndex Set:  RefreshTipsListPos{0}");
                // }
                
                // ScrollViewInfo.Refresh();
            }
        }

        private void RefreshBtnCntChange()
        {
            if (ItemPriceNodeModel==null)
            {
                var cfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.LuckyJourney_02);
                ItemPriceNodeModel = new ItemPriceNodeModel(cfg?.Reward?[0] ?? UIHelper.GetDiamond(100));
            }

            ItemPriceNodeModel.BuyNum = CountTextCnt;
        }

        public override void Update()
        {
            if (UIHelper.CalculateTime(ref IntervalTimeTips,300))
            {
                RefreshTipsList();
            }
            if (UIHelper.CalculateTime(ref IntervalTime))
            {
                RefreshTime();
                if (!ActivityUIManager.Instance.CheckLuckTravelOpen())
                {
                    UIManager.Instance.CloseDialog<LuckTravelView>();
                }
            }
        }

        private void RefreshTime()
        {
            EndTimeValueStr = UIHelper.GetRefreshDayTime(DataCenter.luckyTravelData.EndStamp);
        }

        private object GetScrollViewRewardListCellCallbackByIndex(object index)
        {
            if (ScrollViewRewardList.TryGetValue((int)index, out LuckTravelDrawItemViewModel ret))
            {
                return ret;
            }
            return null;
        }
        
        private object GetSuperRewardListCellCallbackByIndex(object index)
        {
            if (SuperRewardList.TryGetValue((int)index, out CellItemBaseViewModel ret))
            {
                return ret;
            }
            return null;
        }
        
        private int GetTopIndexPos()
        {
            var list = UIHelper.GetAllStageRewardList(ActivityStageType.LuckTravel);
            for (int i = 0; i < list.Count; i++)
            {
                if (!DataCenter.luckyTravelData.CheckProgressFinish(list[i].Id) )
                {
                    return i;
                }
            }
            return list.Count;
        }

        private void RefreshDrawCnt()
        {
            var cfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.LuckyJourney_04);
            var limit = cfg.Content[0] - DataCenter.luckyTravelData.TodayDraw;
            if (limit > 0 && limit < countTextCnt)
            {
                CountTextCnt = limit;
                PlayerPrefs.SetInt(UIHelper.GetRoleTips(LuckTravelCnt),CountTextCnt);
            }

            BatchCanDraw = limit > 0;
        }

        public bool IsCanDraw()
        {
            var cfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.LuckyJourney_04);
            var limit = cfg.Content[0] - DataCenter.luckyTravelData.TodayDraw;
            return limit >= countTextCnt;
        }


        [Command]
        private void OnClickBtnTips()
        {
            UIHelper.OpenCommonRuleProbability(LocalizeHelper.GetGlobal(GlobalLanguageId.LuckyJourney_06),UIHelper.GetDefinesInt(DefineCfgId.LuckyJourney_13));
        }

        
        
        [Command]
        private void OnClickSkipBtn()
        {
            if(DrawIng) return;
            IsSkipAnimation = !isSkipAnimation;
            PlayerPrefs.SetInt(UIHelper.GetRoleTips(SkipTips),IsSkipAnimation?1:0);
        }

        [Command]
        private void OnClickBtnDel()
        {
            if (countTextCnt > 1)
            {
                CountTextCnt -= 1;
            }
            PlayerPrefs.SetInt(UIHelper.GetRoleTips(LuckTravelCnt),CountTextCnt);
            RefreshBtnCntChange();
        }

        [Command]
        private void OnClickBtnAdd()
        {
            var cfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.LuckyJourney_03);
            if (countTextCnt < cfg.Content[0])
            {
                CountTextCnt += 1;
            }

            RefreshDrawCnt();
            PlayerPrefs.SetInt(UIHelper.GetRoleTips(LuckTravelCnt),CountTextCnt);
            RefreshBtnCntChange();
        }

        [Command]
        private void OnClickBtnDrawFree()
        {
            if (!BatchCanDraw)
            {
                return;
            }

            UIHelper.ShowRewardAds(() =>
            {
                var cfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.LuckyJourney_01);
                SendDrawLuckTravel(1,cfg?.Content?[0] ?? 0);
            });
        }

        [Command]
        private void OnClickBtnDrawBatch()
        {
            if (!BatchCanDraw)
            {
                return;
            }
            if (!UIHelper.CheckRewardIsEnough(itemPriceNodeModel.Reward, true, countTextCnt)) return;
            SendDrawLuckTravel(2,CountTextCnt);
        }

        [Command]
        private void OnClickBtnProgress()
        {
	        IsShowProgree = !IsShowProgree;
            if(RewardScrollView!=null)
                RewardScrollView.Jump2SpecificItem(topIndex);
        }

        #region 网络相关

        public void SendDrawLuckTravel(int op,int count,Action succeedAction=null)
        {
            DrawIng = true;
            drawBackRsp = null;
	        ActivityUIManager.Instance.SendDrawLuckTravel(op,count, (rsp) =>
            {
                if (!IsSkipAnimation && SkeletonGraphic != null)
                {
                    drawBackRsp = rsp;
                    SkeletonGraphic.AnimationState.SetAnimation(0, "animation", false);
                }
                else
                {
                    DealDrawResult(rsp);
                }
                
            }, () =>
            {
                DrawIng = false;
            }).Forget();
        }

        private void AniamtionEnd(TrackEntry trackentry)
        {
            DealDrawResult(drawBackRsp);
        }


        private void DealDrawResult( RspLuckyTrip rsp)
        {
            if(rsp==null) return;
            DrawIng = false;
            if (rsp.Pull)
            {
                RefreshTipsList();
            }
            RefreshLimitCnt();
            RefreshDrawCnt();
            UIHelper.OpenCommonRewardView(rsp.Reward.ToList(),null,rsp.Prize!=null && rsp.Prize.Count>0?TitleShowType.SuperReward:TitleShowType.Base,rsp.Prize.ToList());
        }

        #endregion
        
    }
}