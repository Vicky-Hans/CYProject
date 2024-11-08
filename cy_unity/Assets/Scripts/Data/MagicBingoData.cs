using System.Collections.Generic;
using System.Linq;
using DH.Config;
using DH.Proto;
using Google.Protobuf.Collections;

namespace DH.Data
{
    [ProtoWrap(typeof(BingoSync))]
    public partial class MagicBingoData : BaseData
    {

        public bool IsTimeOver()
        {
            return ServerTime.Instance.GetNowTime() > EndStamp;
        }
        /// <summary>
        /// 兑换时间
        /// </summary>
        public bool IsExchangeTime => EndStamp <= ServerTime.Instance.GetNowTime() 
                                      && EndExchangeStamp > ServerTime.Instance.GetNowTime();
        
        #region 任务相关
 
        public List<ActivityTaskCfg> GetAllRewardList(int type,int type2)
        {
            var items = ConfigCenter.ActivityTaskCfgColl.DataItems;
            var taskList = new List<ActivityTaskCfg>(items.Where(o => o.Type== (int)ETaskType.MagicBingo &&
                o.TypeCondi== type2));
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
                    if (!IsFinishTask(item.Id,type2))
                    {
                        typeList.Add(item);
                    }
                }
                else
                {
                    if (item.NextId != 0)
                    {
                        if (IsFinishTask(item.FirstId,type2) && !IsFinishTask(item.NextId,type2) && !IsFinishTask(item.Id,type2))
                        {
                            typeList.Add(item);
                        }
                    }
                    else
                    {
                        if (IsFinishTask(item.FirstId,type2))
                        {
                            typeList.Add(item);
                        }

                    }

                }
            }
            return typeList;
        }
        public bool IsFinishTask(int taskId,int type)
        {
            if (type == 1)
            {
                return taskDailyClaimed.Contains(taskId);
            }
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
        
        public long GetTaskProgress(int taskId,int type)
        {
            var id = 0;
            var cfg = ConfigCenter.ActivityTaskCfgColl.GetDataById(taskId);
            if (cfg != null)
            {
                id = cfg.EventCondi * 1000 + cfg.EventType;
            }

            if (type == 1)
            {
                if (TaskDailyProgress.ContainsKey(id))
                {
                    return TaskDailyProgress[id];
                }
                return 0;
            }
            if (TaskProgress.ContainsKey(id))
            {
                return TaskProgress[id];
            }
            return 0;
        }
        public void SetTaskState(int taskId,int type)
        {
            if (type == 1)
            {
                TaskDailyClaimed.Add(taskId);
                RaisePropertyChanged(nameof(TaskDailyClaimed));
            }
            else
            {
                TaskClaimed.Add(taskId);
                RaisePropertyChanged(nameof(TaskClaimed));
            }


        }

        public LuckEggTaskItemState GetTaskState(int taskId,int type)
        {
            var cfg = ConfigCenter.ActivityTaskCfgColl.GetDataById(taskId);
            var finish = IsFinishTask(cfg.Id,type);
            var all = cfg.EventLoad;
            var value =  GetTaskProgress(cfg.Id,type);
            var IsAdv = cfg.EventType == 8;//看广告
            if (IsAdv)
            {
                var finishAdv = IsFinishTask(cfg.Id,type);
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

        #endregion
        
        public int GetGiftNum(int id)
        {
            return PackageRecord.ContainsKey(id) ? PackageRecord[id] : 0;
        }
        public void BuyGift(int id)
        {
            if (!PackageRecord.ContainsKey(id))
            {
                PackageRecord.Add(id,1);
                RaisePropertyChanged(nameof(PackageRecord));
            }
            else
            {
                PackageRecord[id]++;
                RaisePropertyChanged(nameof(PackageRecord));
            }
        }
        
        public bool IsGetProgressAward(int id)
        {
            return Grid.StageClaimed.Contains(id);
        }
        
        public int GetLastLevel(int id)
        {
            var cfg = ConfigCenter.StageRewardCfgColl.GetDataById(id - 1);
            if (cfg == null)
            {
                return 0;
            }

            if (cfg.Type != (int)ActivityStageType.MagicBingo)
                return 0;
            return cfg.Level;

        }
        public void GetProgressAward()
        {
            var configItems = ConfigCenter.StageRewardCfgColl.GetDataByType((int)ActivityStageType.MagicBingo);
            for (int i = 0; i < configItems.Count; i++)
            {
                if (!Grid.StageClaimed.Contains(configItems[i].Level) && Grid.BingoCount >= configItems[i].Level)
                {
                    Grid.StageClaimed.Add(configItems[i].Id);
                }
            }
            RaisePropertyChanged(nameof(Grid.StageClaimed));
        }
        #region 抽奖相关

        public Resource GetGradAward(int pos)
        {
            if ( Grid.OpenRecord.ContainsKey(pos))
            {
                return Grid.OpenRecord[pos];
            }

            return null;
        }

        /// <summary>
        /// 横数斜是否连线
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool  IsBingoAward(int pos)
        {
            int hundreds = pos / 100 % 10; // 获取百位数字 横
            int ones = pos % 10; // 获取个位数字 列
            
            int rowTemp = 0;
            int columnTemp = 0;
            int HuTemp = 0;
            int HuTemp1 = 0;
            foreach (var item in Grid.OpenRecord)
            {
                int hundreds2 = item.Key / 100 % 10;
                int ones2 = item.Key % 10;
                if (hundreds == hundreds2)
                {
                    rowTemp++;
                    if ( rowTemp >= 5)
                        return true;
                }
                if (ones == ones2)
                {
                    columnTemp++;
                    if ( columnTemp >= 5)
                        return true;
                }

                if (ones2 == ones && hundreds2 == hundreds)
                    continue;
                double slope = (double)(hundreds2 - hundreds) / (ones2 - ones);
                if (slope == -1)
                {
                    HuTemp++;
                    if ( HuTemp >= 4)
                        return true;
                }

                if (slope == 1)
                {
                    HuTemp1++;
                    if ( HuTemp1 >= 4)
                        return true;
                }
            }

            return false;
        }
        
        public void BingoCount()
        {
            Grid.Count ++;
            RaisePropertyChanged(nameof(Grid.Count));
        }
        
        public void Bingo(MapField<int,Resource> gard)
        {
            foreach (var item in gard)
            {
                Grid.OpenRecord.TryAdd(item.Key,item.Value);
            }
            RaisePropertyChanged(nameof(Grid.OpenRecord));
        }
        public void BingoNums(int Nums)
        {
            if (Grid.BingoCount != Nums)
            {
                Grid.BingoCount = Nums;
                RaisePropertyChanged(nameof(Grid.BingoCount));
            }
        }

        public bool IsCanGetAward(int id)
        {
            if (!packageRecord.ContainsKey(id))
            {
                return true;
            }
            var cfg = ConfigCenter.PackageCfgColl.GetDataById(id);
            return packageRecord[id] < cfg.BuyLimit;
        }
        
        #endregion

        /// <summary>
        /// 重置
        /// </summary>
        public void RestData()
        {
            Grid.Count = 0;
            Grid.BingoCount = 0;
            Grid.StageClaimed.Clear();
            Grid.OpenRecord.Clear();
        }

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

        #region 红点相关

        /// <summary>
        /// 宾果奖励红点
        /// </summary>
        /// <returns></returns>
        public bool BinGoCountAwardRed()
        {
            if (IsTimeOver()) return false;
            var configItems = ConfigCenter.StageRewardCfgColl.GetDataByType((int)ActivityStageType.MagicBingo);
            for (int i = 0; i < configItems.Count; i++)
            {
                if (!Grid.StageClaimed.Contains(configItems[i].Id) && Grid.BingoCount >= configItems[i].Level)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 任务红点
        /// </summary>
        /// <returns></returns>
        public bool BinGoTaskRed()
        {
            if (IsTimeOver())return false;
            var temp = GetAllRewardList((int)ETaskType.MagicBingo,1);
            var temp2 = GetAllRewardList((int)ETaskType.MagicBingo,2);
            for (int i = 0; i < temp.Count; i++)
            {
                var cfg = temp[i];
                var state = GetTaskState(cfg.Id,cfg.EventCondi);
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
            for (int i = 0; i < temp2.Count; i++)
            {
                var cfg = temp2[i];
                var state = GetTaskState(cfg.Id,cfg.EventCondi);
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