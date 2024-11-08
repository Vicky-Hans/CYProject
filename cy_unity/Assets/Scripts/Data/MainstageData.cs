using System;
using System.Collections.Generic;
using DH.Config;
using DH.Proto;
using DH.UIFramework;

namespace DH.Data
{
    [ProtoWrap(typeof(MainStage))]
    public partial class MainStageData : BaseData
    {
        [AutoNotify] private List<CopyCfg> chapterCfgs = new();
        public override void Init()
        {
            base.Init();
            UpdateChapterList();
        }

        protected override void ClearData()
        {
            chapterCfgs.Clear();
            Chapters.Clear();
            base.ClearData();
        }

        /// <summary>
        /// 获取章节信息
        /// </summary>
        /// <param name="chapterId"></param>
        /// <returns></returns>
        public MainChapter GetChapterInfo(int chapterId)
        {
            if(Chapters.TryGetValue(chapterId, out var chapter))
            {
                return chapter;
            }

            return null;
        }

        /// <summary>
        /// 是否可以领取
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public int CheckChapterBoxIsCanClaim(int chapterId, int index)
        {
            int ret = 0;
            if (!DataCenter.mainStageData.Chapters.TryGetValue(chapterId, out MainChapter curInfo))
            {
                ret =  -1;
            }
            else
            {
                ret = curInfo.Star >= index ? 0: -1;
            }
            return ret; 
         }
         
        //是否通关
        public bool IsPassChapter(int chapterId)
        {
            return chapterId == 0 || chapters[chapterId]?.Star>=3;
        }
        /// <summary>
        /// 获取最大的通关章节Id
        /// </summary>
        /// <returns></returns>
        public int GetMaxPassChapter()
        {
            var maxPassChapter = CurrChapter;
            if (!IsPassChapter(maxPassChapter)) maxPassChapter -= 1;
            return maxPassChapter;
        }
        /// <summary>
        /// 获取扫荡的类型 1-免费扫荡，2-广告扫荡， -1 没有次数了
        /// </summary>
        /// <returns></returns>
        public void OnPassCurChapter(int chapterId, FightStat data, int star)
        {
            UpdateChapterInfo(chapterId, data, star);
            if (star > 0)
            {
                var cfg = ConfigCenter.CopyCfgColl.GetDataById(chapterId);
                if (cfg.NextPass > 0)
                {
                    AddNewChapterInfo(cfg.NextPass);
                }
            }
        }
        public void AddNewChapterInfo(int chapterId)
        {
            if(Chapters.TryGetValue(chapterId, out _)) return;
            
            MainChapter tempInfo = new MainChapter();
            tempInfo.Star = 0;
            tempInfo.BoxClaimStatus = 0;
            tempInfo.Wave = 0;
            Chapters.Add(chapterId,tempInfo);
        }
        private void UpdateChapterInfo(int chapterId, FightStat info, int star)
        {
            if (Chapters.TryGetValue(chapterId, out MainChapter data))
            {
                data.Star = data.Star <star ? star : data.Star;
                if ((star > 0 && data.Star > 0) || (star ==0 && data.Star == 0))
                {
                    data.Wave = data.Wave < info.Wave ? info.Wave : data.Wave;
                }
            }
        }
        /// <summary>
        /// 波次
        /// </summary>
        /// <returns></returns>
        public int LevelWave()
        {
            return Chapters[CurrChapter].Wave;
        }
        
        public void UpdateChapterList()
        {
            chapterCfgs.Clear();
            var cfgs = ConfigCenter.CopyCfgColl.DataItems;
            foreach (var cfg in cfgs)
            {
                if (cfg.Id <= CurrChapter)
                {
                    chapterCfgs.Add(cfg);   
                }
            }
            chapterCfgs.Sort((a, b) => a.Id - b.Id);
        }
        
        
        public int GetChapterIdByBoxId(int boxId)
        {
            return boxId / 10;
        }

        public int GetBoxIndexByBoxId(int boxId)
        {
            return boxId % 10;
        }
        
        public int GetBoxIdByChapterIdAndIndex(int chapterId, int boxIndex)
        {
            return chapterId * 10 + boxIndex;
        }

        /// <summary>
        /// 获取没领取的章节宝箱 id  这里的 id是 章节id * 10 + 宝箱id  -1 的话 表示都领取完了 0 表示全部领取了
        /// </summary>
        /// <returns></returns>
        public int GetUnClaimedChapterBoxId()
        {
            var ret = 0; 
            // 走当前往回找
            for (int i = 1; i <= CurrChapter; i++)
            {
                var retValue = GetCurChapterUnClaimedBoxId(i);
                if (retValue != -1)
                {
                    ret = retValue;
                    break;
                }
            }
            return ret;
        }
        
        public int GetCurChapterUnClaimedBoxId(int chapterId)
        {
            if (!Chapters.TryGetValue(chapterId, out MainChapter data))
            {
                return -1;
            }
            // 状态
            var boxState = data.BoxClaimStatus;
            for (int i = 1; i < 4; i++)
            {
                // 是否领取了 
                var state = Lodash.ParsePosValue(boxState, i);
                
                if(state != 0) continue;
                // 判断是否可以领取
                return GetBoxIdByChapterIdAndIndex(chapterId, i);
            }
            return -1;
        }
    }
    
    [ProtoWrap(typeof(Hangup))]
    public partial class HangupData : BaseData
    {
        /// <summary>
        /// 获得巡逻时长
        /// </summary>
        /// <returns></returns>
        public long GetTimesPatrol()
        {
           var monthCardEffect = DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.PatrolTimes);
           var settleTime = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.PatrolIncome_01).Content[0]*3600;
           var monthCardCfg = ConfigCenter.MonthlyVipEffectCfgColl.GetDataById((int)MonthCardEffectType.PatrolTimes);
           var MaxTimes = settleTime * (1+(monthCardEffect?(monthCardCfg.Value[0]/100):(0)));
           var time = ServerTime.Instance.GetNowTime() - ClaimedTime;
           return Math.Min(MaxTimes, time);
        }

        /// <summary>
        /// 收益加成
        /// </summary>
        /// <param name="Isshow">是否是预览 特权不生效也展示</param>
        /// <returns></returns>
        public int GetPatrolEarningsAttr(bool isShow = false)
        {
            var isMonthCard = DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.PatrolGains15);
            return isMonthCard || isShow ? 100+ConfigCenter.MonthlyVipEffectCfgColl.GetDataById((int)MonthCardEffectType.PatrolGains15).Value[0] : 100;
        }

        public int MonthAddPatrolNums()
        {
            var monthCard = DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.PatrolNumsx3);
            var addNums = ConfigCenter.MonthlyVipEffectCfgColl.GetDataById((int)MonthCardEffectType.PatrolNumsx3).Value[0];
            return monthCard ? addNums: 0;
        }

        public bool IsRed()
        {
            return  GetTimesPatrol() >= ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.PatrolIncome_04).Content[0];
        }
    }
}