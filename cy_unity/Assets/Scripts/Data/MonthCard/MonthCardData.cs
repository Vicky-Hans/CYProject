using System;
using System.Collections.Generic;
using DH.Config;
using DH.Proto;

namespace DH.Data
{
    public enum MonthType
    {
        MonthCard = 1,//超值月卡
        PrivilegeMonthCard = 2,//特权月卡
        PermanentCard = 3,//终身卡
    }
    [ProtoWrap(typeof(MonthCard))]
    public partial class MonthCardData : BaseData
    {
        private readonly int getAddLivesEffectId = 1005;
        private readonly string MonthVipPlus = "MonthVipPlus"; 
        private MonthlyVipMainCfg normalCardCfg;
        public MonthlyVipMainCfg NormalCardCfg
        {
            get
            {
                if (normalCardCfg == null)
                    normalCardCfg = ConfigCenter.MonthlyVipMainCfgColl.GetDataById((int)MonthType.MonthCard);
                return normalCardCfg;
            }
        }

        private MonthlyVipMainCfg plusCardCfg;
        public MonthlyVipMainCfg PlusCardCfg
        {
            get
            {
                if (plusCardCfg == null)
                    plusCardCfg = ConfigCenter.MonthlyVipMainCfgColl.GetDataById((int)MonthType.PrivilegeMonthCard);
                return plusCardCfg;
            }
        }
        private MonthlyVipMainCfg  permanentCardCfg;
        public MonthlyVipMainCfg PermanentCardCfg
        {
            get
            {
                if (permanentCardCfg == null)
                    permanentCardCfg = ConfigCenter.MonthlyVipMainCfgColl.GetDataById((int)MonthType.PermanentCard);
                return permanentCardCfg;
            }
        }
        public override void Init()
        {
            base.Init();
            UpdataMonthVipPlusCd();
        }
        private void StartSchedule()
        {
            GlobalSchedule.Instance.RemoveSchedulerByTag(MonthVipPlus);
            var cd = GetLastTime(MonthType.PrivilegeMonthCard);
            if (cd>0)
            {
                GlobalSchedule.Instance.AddScheduler(UpdateMaxLives, cd, cd,1, MonthVipPlus);
            }
        }
        void UpdateMaxLives()
        {
            DataCenter.livesData.UpdateMaxLives();
            RaisePropertyChanged(nameof(IsMonthVipPlus));
        }

        public void UpdataMonthVipPlusCd()
        {
            if(IsMonthVipPlus)
            {
                StartSchedule();
            }
            RaisePropertyChanged(nameof(IsMonthVipPlus));
        }

        /// <summary>
        /// 月卡剩余时间
        /// </summary>
        /// <returns>类型 1超值月卡 2特权月卡 </returns>
        public long GetLastTime(MonthType type)
        {
            if (type == MonthType.MonthCard)
            {
               var time = NormalCard.LastStamp - ServerTime.Instance.GetNowTime();
               return Math.Max(0, time);
            }

            if (type == MonthType.PrivilegeMonthCard)
            {
                var time = PlusCard.LastStamp - ServerTime.Instance.GetNowTime();
                return Math.Max(0, time);
            }
            
            return 0;
        }
        
        
        public bool IsMonthVip => GetLastTime(MonthType.MonthCard)>0;
        
        /// <summary>
        /// 判断某个特权开启用MonthCardFuncIsOpen方法 不直接使用 IsMonthVipPlus
        /// </summary>
        public bool IsMonthVipPlus => GetLastTime(MonthType.PrivilegeMonthCard)>0;
        /// <summary>
        /// 终身卡
        /// </summary>
        public bool IsPermanent => LifetimeCard;
        
        public void GetTodayAward(MonthType type)
        {
            if (type == MonthType.MonthCard)
            {
                NormalCard.ClaimDay = TodayNums();
                NormalCard.AllNums++;
            }
            else
            {
                PlusCard.ClaimDay = TodayNums();
                PlusCard.AllNums++;
            }
        }
        
        public void OnBuyNormal()
        {
            if (GetLastTime(MonthType.MonthCard) <= 0)
            {
                NormalCard.LastStamp = ServerTime.Instance.GetNowTime() + NormalCardCfg.Time * 86400;//day
                AddSweepCount(MonthType.MonthCard);
                AddMaxPatrolTimes(MonthType.MonthCard);
            }
            else
            {
                NormalCard.LastStamp += NormalCardCfg.Time * 86400;//day
            }
            NormalCard.MaxNum += NormalCardCfg.Time;
            RaisePropertyChanged(nameof(IsMonthVip));
        }

        public void OnBuyPlus()
        {
            if (GetLastTime(MonthType.PrivilegeMonthCard) <= 0)
            {       
                
                PlusCard.LastStamp = ServerTime.Instance.GetNowTime() + PlusCardCfg.Time * 86400;//day
                DataCenter.livesData.DealLivesChange(GetAddLivesEffect(),true);
                AddSweepCount(MonthType.PrivilegeMonthCard);
                AddMaxPatrolTimes(MonthType.PrivilegeMonthCard);
            }
            else
            {
                PlusCard.LastStamp += PlusCardCfg.Time * 86400;//day
            }
            
            PlusCard.MaxNum += PlusCardCfg.Time;
            DataCenter.livesData.UpdateMaxLives();
            UpdataMonthVipPlusCd();
            RaisePropertyChanged(nameof(IsMonthVipPlus));
        }

        public int TodayNums()
        {
            var unixEpoch = Lodash.GetOriginDateTime();
            var nowTime = unixEpoch.AddSeconds(ServerTime.Instance.GetNowTime());
            var difference = nowTime - unixEpoch;
            return (int)difference.TotalDays;
        }


        public bool IsCanGetAward(MonthType type)
        {
            if (type == MonthType.MonthCard)
            {
                if (!IsMonthVip) return false;
                return NormalCard.AllNums < NormalCard.MaxNum;
            }
            if (type == MonthType.PrivilegeMonthCard)
            {
                if (!IsMonthVipPlus) return false;
                return PlusCard.AllNums < PlusCard.MaxNum;
            }
            return  false;
        }


        /// <summary>
        /// 今天是否已经领取奖励
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool ToDayIsGetAward(MonthType type)
        {
            if (type == MonthType.MonthCard)
            {
                return TodayNums()== NormalCard.ClaimDay;
            }
            if (type == MonthType.PrivilegeMonthCard)
            {
                return TodayNums()== PlusCard.ClaimDay;
            }
            return  false;
        }

        /// <summary>
        /// 是否有未领取奖励
        /// </summary>
        /// <returns></returns>
        public bool IsCanGetAward()
        {
            bool temp = false;
            if (IsMonthVip)
            {
                temp = IsCanGetAward(MonthType.MonthCard) && !ToDayIsGetAward(MonthType.MonthCard);
            }
            
            if (temp) return true;
            
            if (IsMonthVipPlus)
            {
                temp = IsCanGetAward(MonthType.PrivilegeMonthCard) &&  !ToDayIsGetAward(MonthType.PrivilegeMonthCard);
            }
            return temp;
   
        }

        /// <summary>
        /// 获得特权卡增加的额外最大体力值
        /// </summary>
        /// <returns></returns>
        public int GetAddLivesEffect()
        {
            if (MonthCardFuncIsOpen(MonthCardEffectType.Stamina20))
            {
                return ConfigCenter.MonthlyVipEffectCfgColl.GetDataById(getAddLivesEffectId)
                    .Value[0];
            }

            return 0;
        }

        public string OutPutEffectDes(string des,List<int> list)
        {
            if (list == null) return des;
            var desTemplate = des;
            for (int j = 0; j < list.Count; j++)
            {
                desTemplate = desTemplate.Replace("{" + j + "}",list[j].ToString());
            }

            return desTemplate;
        }

        #region 功能判断
        
        /// <summary>
        /// 是否开启月卡特权
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool  MonthCardFuncIsOpen(MonthCardEffectType type)
        {
            bool isOpen = false;
            for (int i = 0; i < NormalCardCfg.EffectId.Count; i++)
            {
                if (NormalCardCfg.EffectId[i] == (int)type && IsMonthVip)
                {
                    isOpen = true;
                    break;
                } 
            }
            for (int i = 0; i < PlusCardCfg.EffectId.Count; i++)
            {
                if (PlusCardCfg.EffectId[i] == (int)type && IsMonthVipPlus)
                {
                    isOpen = true;
                    break;
                } 
            }
            for (int i = 0; i < PermanentCardCfg.EffectId.Count; i++)
            {
                if (PermanentCardCfg.EffectId[i] == (int)type && IsPermanent)
                {
                    isOpen = true;
                    break;
                } 
            }
            return isOpen;
        }
        
        /// <summary>
        /// 添加扫荡次数
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public void AddSweepCount(MonthType monthtype,MonthCardEffectType type = MonthCardEffectType.PatrolNumsx3)
        {
            Action<bool> temp = (isAdd) =>
            {
                if (isAdd)
                {
                    var nums = ConfigCenter.MonthlyVipEffectCfgColl.GetDataById((int)type).Value[0];
                    DataCenter.mainStageData.Hangup.FreeCount += nums;
                }
            };
            if (monthtype == MonthType.MonthCard)
                for (int i = 0; i < NormalCardCfg.EffectId.Count; i++)
                {
                    if (NormalCardCfg.EffectId[i] == (int)type && IsMonthVip)
                    {
                        temp(true);
                        break;
                    } 
                }
            
            if (monthtype == MonthType.PrivilegeMonthCard)
                for (int i = 0; i < PlusCardCfg.EffectId.Count; i++)
                {
                    if (PlusCardCfg.EffectId[i] == (int)type && IsMonthVipPlus)
                    {
                        temp(true);
                        break;
                    } 
                }
            
            if (monthtype == MonthType.PermanentCard)
                for (int i = 0; i < PermanentCardCfg.EffectId.Count; i++)
                {
                    if (PermanentCardCfg.EffectId[i] == (int)type && IsPermanent)
                    {
                        temp(true);
                        break;
                    } 
                }
        }
        
        public void AddMaxPatrolTimes(MonthType monthtype,MonthCardEffectType type = MonthCardEffectType.PatrolTimes)
        {
            Action<bool> temp = (isAdd) =>
            {
                if (isAdd)
                {
                    var times = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.PatrolIncome_01).Content[0]*3600;
                    var NowTimes= DataCenter.mainStageData.Hangup.GetTimesPatrol();
                    if (NowTimes >= times)
                    {
                        DataCenter.mainStageData.Hangup.ClaimedTime = ServerTime.Instance.GetNowTime() - times;
                    }
                }
            };
            if (monthtype == MonthType.MonthCard)
                for (int i = 0; i < NormalCardCfg.EffectId.Count; i++)
                {
                    if (NormalCardCfg.EffectId[i] == (int)type && IsMonthVip)
                    {
                        temp(true);
                        break;
                    } 
                }
            
            if (monthtype == MonthType.PrivilegeMonthCard)
                for (int i = 0; i < PlusCardCfg.EffectId.Count; i++)
                {
                    if (PlusCardCfg.EffectId[i] == (int)type && IsMonthVipPlus)
                    {
                        temp(true);
                        break;
                    } 
                }
            
            if (monthtype == MonthType.PermanentCard)
                for (int i = 0; i < PermanentCardCfg.EffectId.Count; i++)
                {
                    if (PermanentCardCfg.EffectId[i] == (int)type && IsPermanent)
                    {
                        temp(true);
                        break;
                    } 
                }
        }

        #endregion
    }
}