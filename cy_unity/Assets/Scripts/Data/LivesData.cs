using System;
using DH.Config;
using DH.Proto;
using DH.UIFramework;
using UnityEngine;

namespace DH.Data
{
    [ProtoWrap(typeof(Lives))]
    public partial class LivesData : BaseData
    {
        private readonly string LivesTag = "LivesTag"; 
        public int CfgCd;
        public long CountPullTime;
        private int cfgMax;
        private int monthlyCardNum;
        private long startTimeOffset;
        [AutoNotify] private int maxLives;
        public Action RequestLivesCallback;

        public override void Init()
        {
            base.Init();
            GetCfgs();
            MaxLives = GetMaxLives();
            if(!IsFull())
            {
                startTimeOffset = (ServerTime.Instance.GetNowTime() - RefreshStamp) % CfgCd;//ServerTime.Instance.SvrTime - RefreshStamp;
                StopShedule();
                StartSchedule(startTimeOffset);
            }
        }

        public bool IsFull()
        {
            return Curr >= GetMaxLives();
        }

        public string NextLiveTime()
        {
            if(IsFull())
            {
                return "";
            }
            var nextTime = CfgCd - (ServerTime.Instance.GetNowTime() - RefreshStamp)%CfgCd;
            return ServerTime.Instance.Seconds2Mmss(nextTime);
        }

        private void StartSchedule(long offset)
        {
            //Debug.LogError((CfgCd - offset).ToString());
            GlobalSchedule.Instance.AddScheduler(AddLives, CfgCd, CfgCd - offset, -1, LivesTag);
        }
        
        private void StopShedule()
        {
            GlobalSchedule.Instance.RemoveSchedulerByTag(LivesTag);
        }

        private void GetCfgs()
        {
            CfgCd = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.hp_recover_time).Content[0]*60;//分钟
            cfgMax = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.hp_save_max).Content[0];
        }

        protected override void ClearData()
        {
            CfgCd = 0;
            StopShedule();
            base.ClearData();
        }

        private int MonthAddLivesEffect => DataCenter.monthCardData.GetAddLivesEffect();
        public int GetMaxLives()
        {
            var cfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.hp_save_max);
            var max = cfg.Content[0] + MonthAddLivesEffect;
            return max;
        }

        public void UpdateMaxLives()
        {
            var tmpMaxLives = GetMaxLives();
            if(!IsFull())
            {
                var offsetTime = (ServerTime.Instance.GetNowTime() - RefreshStamp) % CfgCd;
                StopShedule();
                StartSchedule(offsetTime);
            }
            else
            {
                StopShedule();
            }
            MaxLives = tmpMaxLives;
        }

        /// <summary>
        /// 用于自动回复
        /// </summary>
        /// <param name="count"></param>
        private void AddLives(int count=1)
        {
            if(IsFull())
            {
                StopShedule();
                return;
            }
            var max = MaxLives;
            Curr = Mathf.Min(max,Curr+count);
            RefreshStamp = ServerTime.Instance.GetNowTime();
        }

        public void DealLivesChange(int count, bool isAdd)
        {
            if (isAdd)
            {
                Curr += count;
                if(IsFull())
                {
                    StopShedule();
                    return;
                }
            }
            else
            {
                ConsumeLives(count);
            }
        }

        private void AddLives()
        {
            RequestLivesCallback?.Invoke();
        }
        public bool ConsumeLives(int count=1)
        {
            if(Curr < count)
                return false;
            if (IsFull())
            {
                RefreshStamp = ServerTime.Instance.GetNowTime();
            }
            Curr -= count;
            if (!IsFull())
            {
                StartSchedule(0);
            }
            return true;
        }

        public void OnBuyMonthCardPlus()
        {
        }

        public void OnSaveCountBuy()
        {
            DiamondStoreTimes -= 1;
        }

        public bool CheckItemIsEnough(int id, long count)
        {
            if (id != (int)GameConst.ItemIdCode.EnergyDrink)
            {
                return false;
            }

            return Curr >= count;
        }
        
        
        
    }
}