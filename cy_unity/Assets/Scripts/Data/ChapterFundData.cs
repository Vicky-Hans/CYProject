using System;
using System.Linq;
using DH.Config;
using DH.Proto;
using DHFramework;

namespace DH.Data
{
    [ProtoWrap(typeof(ChapterFund))]
    public partial class ChapterFundData : BaseData
    {
        /// <summary>
        /// 是否购买
        /// </summary>
        public bool IsBuy => Plus;

        /// <summary>
        /// 是否可以领取奖励
        /// </summary>
        /// <param name="fundRewardId">表ID</param>
        /// <param name="chapterId">章节Id</param>
        /// <param name="isPlus">是否是Plus</param>
        /// <returns></returns>
        public bool IsCanGetAward(int fundRewardId,int chapterId,bool isPlus = false)
        {
            var passChapter = chapterId <= DataCenter.mainStageData.GetMaxPassChapter();
            if (!isPlus) return !FreeClaim.Contains(fundRewardId) && passChapter;
            return !PlusClaim.Contains(fundRewardId) && passChapter && IsBuy;
        }
        
        /// <summary>
        /// 是否领取了奖励
        /// </summary>
        /// <param name="fundRewardId"></param>
        /// <param name="isPlus"></param>
        /// <returns></returns>
        public bool IsGetAward(int fundRewardId,bool isPlus = false)
        {
            if (!isPlus) return FreeClaim.Contains(fundRewardId);
            return PlusClaim.Contains(fundRewardId);
        }

        public bool GetAwards;
        /// <summary>
        /// 领取了奖励
        /// </summary>
        public void GetAward()
        {
            var items = ConfigCenter.FundRewardsCfgColl.DataItems.Where(o=>o.Type == (int)EPassPortType.PassportTypeChapter).ToList();
            for (int i = 0; i < items.Count; i++)
            {
                var isPassChapter = items[i].Factor <= DataCenter.mainStageData.GetMaxPassChapter();
                if (IsBuy && isPassChapter && !PlusClaim.Contains(items[i].Id))
                {
                    PlusClaim.Add(items[i].Id);
                }
                if (isPassChapter && !FreeClaim.Contains(items[i].Id))
                {
                    FreeClaim.Add(items[i].Id);
                }
            }
            RaisePropertyChanged(nameof(GetAwards));
        }

        /// <summary>
        /// 红点
        /// </summary>
        /// <returns></returns>
        public bool IsRed()
        {
            var items = ConfigCenter.FundRewardsCfgColl.DataItems.Where(o=>o.Type == (int)EPassPortType.PassportTypeChapter).ToList();
            for (int i = 0; i < items.Count; i++)
            {
                if (IsCanGetAward(items[i].Id,items[i].Factor,true) || IsCanGetAward(items[i].Id,items[i].Factor))
                {
                    return true;
                }
            }
            return false;
        }
        
        public int NowIndex()
        {
            var items = ConfigCenter.FundRewardsCfgColl.DataItems.Where(o=>o.Type == (int)EPassPortType.PassportTypeChapter).ToList();
            if (IsRed())
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (IsCanGetAward(items[i].Id,items[i].Factor) || 
                        IsCanGetAward(items[i].Id,items[i].Factor,true))
                    {
                        return i;
                    }
                }
            }
            else
            {
                var nowChapter = DataCenter.mainStageData.GetMaxPassChapter();
                int Index = 0;
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Factor <= nowChapter)
                    {
                        Index++;
                    }
                    else
                    {
                        break;
                    }
                    
                }
                return Math.Min(items.Count-1,Math.Max(0,Index-1));
            }

            return 0;
        }

        public bool IsGetAwardOver()
        {
            if (!IsBuy) return false;
            var items = ConfigCenter.FundRewardsCfgColl.DataItems
                .Where(o => o.Type == (int)EPassPortType.PassportTypeChapter).ToList();
            items = items.OrderBy(o => o.Factor).ToList();
            if (FreeClaim.Contains(items[^1].Id) && plusClaim.Contains(items[^1].Id))
            {
                return true;
            }
            return false;
        }

    }
}
