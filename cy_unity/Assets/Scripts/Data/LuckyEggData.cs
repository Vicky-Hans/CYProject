using System.Collections.Generic;
using System.Linq;
using DH.Config;
using DH.Proto;


namespace DH.Data
{
    public enum LuckEggTaskItemState
    {
        NoFinish,
        NoWay,
        NotGetAward,
        Finish,
    }
    [ProtoWrap(typeof(LuckyEggSync))]
    public partial class LuckyEggData : BaseData
    {
        
        
        /// <summary>
        /// 活动时间是否结束（只能打开兑换界面） -不包含兑换时间结束
        /// </summary>
        /// <returns></returns>
        public bool IsTimeOver()
        {
            return ServerTime.Instance.GetNowTime() > EndStamp;
        }
        
        /// <summary>
        /// 兑换时间
        /// </summary>
        public bool IsExchangeTime => EndStamp <= ServerTime.Instance.GetNowTime() 
                                      && EndExchangeStamp > ServerTime.Instance.GetNowTime();
        

        #region 扭蛋抽奖
        public bool CheckProgressFinish(int id)
        {
            return StageClaimed.Contains(id);
        }
        
        public void SetScoreClaimed(int id)
        {
            StageClaimed.Add(id);
        }

        #endregion
        #region 自选缓存

        public bool IsSelectOptionalReward(int id,int type =1)
        {
            return GetOptionalSelectIndex(id,type) != -1;
        }
        /// <summary>
        /// k-对应id*10+type[1-进度奖励自选，2-兑换自选] ，v-自选index从0开始
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetOptionalSelectIndex(int id,int type =1)
        {
            if (OptionalRecord.ContainsKey(id*10+type))
            {
                return OptionalRecord[id*10+type];
            }

            return -1;
        }
        
        public void SetOptionalSelectIndex(int id,int selectIndex,int type =1)
        {
            OptionalRecord[id*10+type] = selectIndex;
            RaisePropertyChanged(nameof(OptionalRecord));
        }
        
        #endregion

        #region 兑换相关
        /// <summary>
        /// 兑换
        /// </summary>
        /// <param name="id"></param>
        public void Exchange(int id)
        {
            if (!ExchangeRecord.ContainsKey(id))           
                ExchangeRecord.Add(id,1);
            else
                ExchangeRecord[id]++;
            RaisePropertyChanged(nameof(ExchangeRecord));
        }
        
        public bool IsCanExchange(int id)
        {
            var cfg = ConfigCenter.ExchangeShopCfgColl.GetDataById(id);
            if (!ExchangeRecord.ContainsKey(id))           
                return true;
            
            return ExchangeRecord[id] < cfg.BuyLimit ;
        }
        
        public int GetExchangeNums(int id)
        {
            if (!ExchangeRecord.ContainsKey(id))           
                return 0;
            else
                return ExchangeRecord[id];
        }

        #endregion

        #region 基金相关

        public bool GetPlusIsBuy()
        {
            return FundPlus;
        }
        public void AddBuyState()
        {
             FundPlus = true;
        }
        
        public bool IsFinish(int value)
        {
            return ServerTime.Instance.GetPassDay(StartStamp)>=value;
        }
        public bool IsClaimed(int id)
        {
            return FundClaimed.Contains(id);
        }
        public bool IsPlusClaimed(int id)
        {
            return FundPlusClaimed.Contains(id);
        }
        
        //添加免费领取ID
        public void AddFreeIds(List<int> ids)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                if (!FundClaimed.Contains(ids[i])) FundClaimed.Add(ids[i]);
            }
            RaisePropertyChanged(nameof(FundClaimed));
        }
        
        //添加付费领取ID
        public void AddPlusOneIds(List<int> ids)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                if (!FundPlusClaimed.Contains(ids[i])) FundPlusClaimed.Add(ids[i]);
            }
            RaisePropertyChanged(nameof(FundPlusClaimed));
        }

        private bool isCanGetAward(ActivityFundCfg Cfg)
        {
            if (IsTimeOver())return false;
            var FreeRed = false;
            var PlusRed = false;
            var finish =IsFinish(Cfg.Factor);
            if (!IsClaimed(Cfg.Id) && finish)
            {
                FreeRed = true;
            }


            var isPlus = IsPlusClaimed(Cfg.Id);
            var isBuy = GetPlusIsBuy();
            if (isBuy && !isPlus && finish)
            {
                PlusRed = true;
            }

            return FreeRed || PlusRed;
        }

        public bool IsFundRed()
        {
            var levelItems = ConfigCenter.ActivityFundCfgColl.GetDataByType((int)ActivityFund.LuckEgg);
            foreach (var cfg in levelItems)
            {
                if (isCanGetAward(cfg))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region 任务相关
 
        public List<ActivityTaskCfg> GetAllRewardList(int type)
        {
            var items = ConfigCenter.ActivityTaskCfgColl.DataItems;
            var taskList = new List<ActivityTaskCfg>(items.Where(o => o.Type== (int)ETaskType.LuckEgg));
            List<ActivityTaskCfg> typeList = new();
            foreach (var item in taskList)
            {

                if (item.NextId == 0 && item.FirstId == 0 )
                {
                    typeList.Add(item);
                    continue;
                }
                
                if (item.FirstId == 0 )
                {
                    if (!IsFinishTask(item.Id))
                    {
                        typeList.Add(item);
                    }
                }
                else
                {
                    if (item.NextId != 0)
                    {
                        if (IsFinishTask(item.FirstId) && !IsFinishTask(item.NextId) && !IsFinishTask(item.Id))
                        {
                            typeList.Add(item);
                        }
                    }
                    else
                    {
                        if (IsFinishTask(item.FirstId))
                        {
                            typeList.Add(item);
                        }

                    }

                }
            }
            return typeList;
        }
        public bool IsFinishTask(int taskId)
        {
            return TaskClaimed.Contains(taskId);
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
        
        public long GetTaskProgress(int taskId)
        {
            var id = 0;
            var cfg = ConfigCenter.ActivityTaskCfgColl.GetDataById(taskId);
            if (cfg != null)
            {
                id = cfg.EventCondi * 1000 + cfg.EventType;
            }
            if (TaskProgress.ContainsKey(id))
            {
                return TaskProgress[id];
            }
            return 0;
        }
        public void SetTaskState(int taskId)
        {
            TaskClaimed.Add(taskId);
            RaisePropertyChanged(nameof(TaskClaimed));
        }

        public LuckEggTaskItemState GetTaskState(int taskId)
        {
            var cfg = ConfigCenter.ActivityTaskCfgColl.GetDataById(taskId);
            var finish = IsFinishTask(cfg.Id);
            var all = cfg.EventLoad;
            var value =  GetTaskProgress(cfg.Id);
            var IsAdv = cfg.EventType == 8;//看广告
            if (IsAdv)
            {
                var finishAdv = IsFinishTask(cfg.Id);
                if (finishAdv)
                {
                    return LuckEggTaskItemState.Finish;
                }
                
                return LuckEggTaskItemState.NoFinish;
            }
            if (all > value)
            {
                if (cfg.TaskList != 0)
                {
                    return LuckEggTaskItemState.NoFinish;
                }

                return LuckEggTaskItemState.NoWay;

            }
            if (finish)
            {
                return LuckEggTaskItemState.Finish;   
            }

            return LuckEggTaskItemState.NotGetAward;
        }
        
        public bool IsTaskRed()
        {
            if (IsTimeOver())return false;
            var temp = GetAllRewardList((int)ETaskType.LuckEgg);
            for (int i = 0; i < temp.Count; i++)
            {
                var cfg = temp[i];
                var state = GetTaskState(cfg.Id);
                var IsAdv = cfg.EventType == 8;//看广告
                if (state == LuckEggTaskItemState.NoFinish && IsAdv)
                {
                    return true;
                }

                if (state == LuckEggTaskItemState.NotGetAward && !IsAdv)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }

}
