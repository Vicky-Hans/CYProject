using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using Dh.Game.ViewModels;
using DH.Game.ViewModels;
using DH.Proto;
using DH.UIFramework;
using DHFramework;

namespace DH.Game.UI
{
    public enum CollegeActivity
    {
        None = 0,
        CollegeTask=1,
        CollegeRank=2,
        CollegeShop=3,
        CollegeFund=4,
    }

    public partial class CollegeActivityManager : DH.UIFramework.ObservableSingleton<CollegeActivityManager>
    {
        [AutoNotify] public bool drawIng;
        private CollegeActivity curTab = CollegeActivity.CollegeTask;

        public string CollegeSign = $"CollegeFund_{DataCenter.charcaterData.Digest.RoleId}";
 
        public CollegeActivity CurTab
        {
            get => curTab;
            set => Set(ref curTab, value);
        }

        public string GetTaskDesc(ActivityTaskCfg cfg)
        {
            var cfgL = ConfigCenter.ActivityTaskLanguageCfgColl.GetDataById(cfg.Id);
            if (cfgL != null)
            {
                return string.Format(cfgL.Name, cfg.EventLoad);
            }

            return string.Empty;
        }

        public List<StageRewardCfg> GetAllRewardList()
        {
            return UIHelper.GetAllStageRewardList(ActivityStageType.College);
        }
        
        public List<ActivityTaskCfg> GetAllRewardList(int type)
        {
             var taskList = ConfigCenter.ActivityTaskCfgColl.GetDataByType((int)ETaskType.CollegeTask);
             List<ActivityTaskCfg> typeList = new();
             if (taskList != null)
             {
                 foreach (var item in taskList)
                 {
                     if (item.TypeCondi == type)
                     {
                         if (item.NextId == 0 && (item.FirstId == 0 || DataCenter.collegeData.IsFinishTask(item.FirstId)))
                         {
                             typeList.Add(item);
                             continue;
                         }

                         if (item.FirstId == 0 && !DataCenter.collegeData.IsFinishTask(item.Id))
                         {
                             typeList.Add(item);
                         }
                         else
                         {
                             if (DataCenter.collegeData.IsFinishTask(item.FirstId) && !DataCenter.collegeData.IsFinishTask(item.Id))
                             {
                                 typeList.Add(item);
                             }
                         }
                     }

                 }
             }
             return typeList;
        }
        
        public int GetLastRewardValue(int id)
        {
            var value = 0;
            var cfg = ConfigCenter.StageRewardCfgColl.GetDataById(id-1);
            if (cfg != null)
            {
                value += cfg.Level;
            }
            return value;
        }
        
        public int GetScrollIdx()
        {
            var curList = new List<int>();
            var list = ActivityUIManager.Instance.GetActivityFund(ActivityFund.College);
            foreach (var item in list)
            {
                if (DataCenter.collegeData.IsFinish(item.Factor) && (!DataCenter.collegeData.IsClaimed(item.Id) || (DataCenter.collegeData.GetPlusIsBuy() && !DataCenter.collegeData.IsPlusClaimed(item.Id))))
                {
                    curList.Add(item.Id);
                }
            }
            for (int i = 0; i < curList.Count; i++)
            {
                if (!DataCenter.collegeData.IsClaimed(curList[i]) && !DataCenter.collegeData.IsPlusClaimed(curList[i])) return i;
            }
            return 0;
        }

        public bool CheckEndTime()
        {
            return !ServerTime.Instance.IsOpenTime(DataCenter.collegeData.StartStamp,DataCenter.collegeData.EndStamp);
            
        }

        public RankMember GetMySelfRankMember(int score,long rank)
        {
            return new RankMember
            {
                RoleId = DataCenter.charcaterData.Digest.RoleId,
                Name = DataCenter.charcaterData.Digest.Name,
                Logo = DataCenter.charcaterData.Digest.HeadId,
                HeadFrame = DataCenter.charcaterData.Digest.HeadFrame,
                Score = score,
                Rank = (int)rank,
                Stage = DataCenter.mainStageData.IsPassChapter(DataCenter.mainStageData.CurrChapter)?DataCenter.mainStageData.CurrChapter:DataCenter.mainStageData.CurrChapter-1,
                VipStatus = DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.GoldenNickname) ? 3 : 0,
            };
        }

        public ActivityRankingCfg GetMySelfRankingCfg(long rank)
        {
            var rankList = ConfigCenter.ActivityRankingCfgColl.DataItems.ToList();
            foreach (var item in rankList)
            {
                if (rank >= item.Ranking[0] && rank <= item.Ranking[1])
                {
                    return item;
                }
            }

            return null;
        }

        #region 红点相关

        public bool CheckLuckTravelRedDot()
        {
            //红点功能添加
            return false;
        }
        public bool CheckCollegeRedDot()
        {
            //红点功能添加
            return CheckCollegeTaskRed() || CheckCollegeFundRed() || CheckCollegeShopRed();
        }

        public bool CheckCollegeFundRed()
        {
            var levelItems = ActivityUIManager.Instance.GetActivityFund(ActivityFund.College);
            foreach (var item in levelItems)
            {
                if (DataCenter.collegeData.IsFinish(item.Factor) && !DataCenter.collegeData.IsClaimed(item.Id))
                {
                    return true;
                }
            
                if (DataCenter.collegeData.GetPlusIsBuy() && DataCenter.collegeData.IsFinish(item.Factor) && !DataCenter.collegeData.IsPlusClaimed(item.Id))
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckCollegeTaskRed()
        {
            bool taskRed = CheckCollegeTypeRed(CollegeTaskTitle.Clothes) || CheckCollegeTypeRed(CollegeTaskTitle.Draw) || CheckCollegeTypeRed(CollegeTaskTitle.Else);
            if (taskRed) return true;
            var scoreList = GetAllRewardList();
            foreach (var item in scoreList)
            {
                if (item.Level <= DataCenter.collegeData.Score && !DataCenter.collegeData.CheckProgressFinish(item.Id))
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckCollegeTypeRed(CollegeTaskTitle collegeTaskTitle)
        {
            var list = GetAllRewardList((int)collegeTaskTitle);
            foreach (var item in list)
            {
                var finish = DataCenter.collegeData.IsFinishTask(item.Id);
                var all = item.EventLoad;
                var value = DataCenter.collegeData.GetTaskProgress(item.Id);
                if (!finish && value>=all)
                {
                    return true;
                }
            }
            return false;
        }

        
        public bool GetCollegeTypeTask(CollegeTaskTitle collegeTaskTitle)
        {
            var list = GetAllRewardList((int)collegeTaskTitle);
            foreach (var item in list)
            {
                var finish = DataCenter.collegeData.IsFinishTask(item.Id);
                var all = item.EventLoad;
                var value = DataCenter.collegeData.GetTaskProgress(item.Id);
                if (!finish && value>=all)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsNeedSelectReward(int id)
        {
            var proList = GetAllRewardList();
            foreach (var item in proList)
            {
                if (item.Id < id && item.OptionalReward is { Count: > 0 } && !DataCenter.collegeData.IsSelectOptionalReward(item.Id))
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckCollegeShopRed()
        {
            var list = ConfigCenter.PackageCfgColl.GetDataByRule((int)EPackageType.CollegeShop);
            if (list == null) return false;
            for (int i = 0; i < list.Count; i++)
            {
                var cfg = list[i];
                if ((cfg.RechargeId is  0 or 1) && DataCenter.collegeData.GetGiftNum(cfg.Id) < cfg.BuyLimit)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region 网络相关

        /// <summary>
        /// 领取记录
        /// </summary>
        /// <param name="id">记录id</param>
        /// <param name="index">自选奖励</param>
        public async UniTaskVoid SendCollegeScoreClaim(int id,int index = -1)
        {
            if(CheckEndTime()) return;
            var req = new ReqSchoolScoreClaim()
            {
                Id = id,
                OptionalIndex = index
            };
            var result =await GameNetworkManager.Instance.SendAsync<RspSchoolScoreClaim>(req);

            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                Lodash.DealRewards(result.rsp.Rewards.ToList());
                DataCenter.collegeData.SetScoreClaimed(id);
                UIHelper.OpenCommonRewardView(result.rsp.Rewards.ToList());
            }
        }
        
        /// <summary>
        /// 领取记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="index"></param>
        public async UniTaskVoid SendCollegeTaskClaim(int id,Action succeedAction=null)
        {
            if(CheckEndTime()) return;
            var req = new ReqSchoolTaskClaim()
            {
                Id = id,
            };
            var result =await GameNetworkManager.Instance.SendAsync<RspSchoolTaskClaim>(req);

            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                succeedAction?.Invoke();
                Lodash.DealRewards(result.rsp.Rewards.ToList());
                DataCenter.collegeData.SetTaskState(id);
                UIHelper.OpenCommonRewardView(result.rsp.Rewards.ToList());
            }
        }
        
        public async UniTaskVoid SendCollegeRank(Action<List<RankMember>,int,long> succeedAction=null,Action failAction=null)
        {
            var data = new ReqRankOp();
            data.Op = 3;
            data.Param = 1;
            var result = await GameNetworkManager.Instance.SendAsync<RspRankOp>(data);
            NetHelper.CheckNetErrorMessage(result.rsp, true, () =>
            {
                succeedAction?.Invoke(result.rsp.Top.ToList(),result.rsp.Score,result.rsp.Rank);
            },failAction);
        }
        
        public async UniTaskVoid SendPlayerInfo(RankMember info)
        {
            if(info==null) return;
            ReqRankDigest data = new ReqRankDigest();
            data.RoleId = info.RoleId;
            var result = await GameNetworkManager.Instance.SendAsync<RspRankDigest>(data);
            NetHelper.CheckNetErrorMessage(result.rsp, true, () =>
            {
                DHLog.Debug($" 请求 玩家信息 {info.RoleId}");
                RankPlayerInfoViewModel tempVm = new (info, result.rsp.Data);
                UIManager.Instance.OpenDialog<RankPlayerInfoView>(tempVm).Forget();
            });
         
       
			
        }
        
        public async UniTaskVoid SendSchoolFundClaim(Action succeedAction)
        {
            var result = await GameNetworkManager.Instance.SendAsync<RspSchoolFundClaim>(new ReqSchoolFundClaim());
            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                Lodash.DealRewards(result.rsp.Rewards.ToList());
                DataCenter.collegeData.AddFreeIds(result.rsp.FreeIds.ToList());
                DataCenter.collegeData.AddPlusOneIds(result.rsp.PlusIds.ToList());
                UIHelper.OpenCommonRewardView(result.rsp.Rewards.ToList());
                succeedAction?.Invoke();
            }
        }
        
      

        #endregion

    
    }
}