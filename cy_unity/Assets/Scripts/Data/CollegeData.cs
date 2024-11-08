using System.Collections.Generic;
using DH.Config;
using DH.Proto;

namespace DH.Data
{
    [ProtoWrap(typeof(SchoolSync))]
    public partial class CollegeData : BaseData
    {
        public override void Init()
        {
            base.Init();
        }
        
        protected override void ClearData()
        {
            base.ClearData();
        }
        
        public void SyncData(SchoolSync message)
        {
            var clearCollection = true;
            Score = message.Score;
            if(clearCollection)
            {
                taskProgress.Clear();
            }

            foreach(var genCodeItem in message.TaskProgress)
            {
                taskProgress.Add(genCodeItem.Key,genCodeItem.Value);
            }

            if(clearCollection)
            {
                taskClaimed.Clear();
            }

            foreach(var genCodeItem in message.TaskClaimed)
            {
                taskClaimed.Add(genCodeItem);
            }

            if(clearCollection)
            {
                scoreClaimed.Clear();
            }

            foreach(var genCodeItem in message.ScoreClaimed)
            {
                scoreClaimed.Add(genCodeItem);
            }

            if(clearCollection)
            {
                packageRecord.Clear();
            }

            foreach(var genCodeItem in message.PackageRecord)
            {
                packageRecord.Add(genCodeItem.Key,genCodeItem.Value);
            }

            FundPlus = message.FundPlus;
            if(clearCollection)
            {
                fundClaimed.Clear();
            }

            foreach(var genCodeItem in message.FundClaimed)
            {
                fundClaimed.Add(genCodeItem);
            }

            if(clearCollection)
            {
                fundPlusClaimed.Clear();
            }

            foreach(var genCodeItem in message.FundPlusClaimed)
            {
                fundPlusClaimed.Add(genCodeItem);
            }
        }

        public int GetScore()
        {
            return Score;
        }

        public bool CheckProgressFinish(int id)
        {
            return ScoreClaimed.Contains(id);
        }
        
        public bool IsSelectOptionalReward(int id,int type =1)
        {
            return GetOptionalSelectIndex(id,type) != -1;
        }

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
        }

        public void SetScoreClaimed(int id)
        {
            ScoreClaimed.Add(id);
        }

        public bool IsFinishTask(int taskId)
        {
            return TaskClaimed.Contains(taskId);
        }
        
        public void SetTaskState(int taskId)
        {
            TaskClaimed.Add(taskId);
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

        public bool GetPlusIsBuy()
        {
            return FundPlus;
        }

        public bool IsClaimed(int id)
        {
            return FundClaimed.Contains(id);
        }
        
        public bool IsPlusClaimed(int id)
        {
            return FundPlusClaimed.Contains(id);
        }

        public void AddScore(int score)
        {
            if (score > 0) Score += score;
        }
        
        
        //添加免费领取ID
        public void AddFreeIds(List<int> ids)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                if (!FundClaimed.Contains(ids[i])) FundClaimed.Add(ids[i]);
            }
        }        //添加付费领取ID
        public void AddPlusOneIds(List<int> ids)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                if (!FundPlusClaimed.Contains(ids[i])) FundPlusClaimed.Add(ids[i]);
            }
        }

        public bool IsFinish(int value)
        {
            return ServerTime.Instance.GetPassDay(StartStamp)>=value;
        }

        public void AddBuyState()
        {
            FundPlus = true;
        }
        
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
    }
}
