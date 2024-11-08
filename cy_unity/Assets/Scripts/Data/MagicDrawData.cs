using System.Collections.Generic;
using DH.Config;
using DH.Proto;
using DH.UIFramework;

namespace DH.Data
{
    [ProtoWrap(typeof(MagicDrawSync))]
    public partial class MagicDrawData : BaseData
    {
        public override void Init()
        {
            base.Init();
        }

        protected override void ClearData()
        {
            ClaimRecord.Clear();
            DrawRecord.Clear();
            base.ClearData();
        }


        /// <summary>
        /// 检查是否已领取
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CheckIsClaimed(int id)
        {
            return claimRecord.Contains(id);
        }
        /// <summary>
        /// 检查是否可以领取
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CheckIsCanClaim(int id)
        {
            var cfg = ConfigCenter.StageRewardCfgColl.GetDataById(id);
            if (cfg == null) return false;
            return Progress >= cfg.Level;

        }
        /// <summary>
        /// 检查是否已领取
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CheckFundIsClaimed(EPassportRewardType type, int id)
        {
            if (type == EPassportRewardType.PassportTypeFree)
            {
                return fundClaimed.Contains(id);
            }

            return fundPlusClaimed.Contains(id);
        }

        public List<int> GetCanClaimIdList()
        {
            List<int> ret = new();
            //进度奖励
            var cfgs = ConfigCenter.StageRewardCfgColl.DataItems;
            foreach (var item in cfgs)
            {
                if (item.Type != 2) continue;
                if(!CheckIsCanClaim(item.Id)) continue;
                if(CheckIsClaimed(item.Id)) continue;
                ret.Add(item.Id);
            }
            return ret;
        }

        public List<int> GetCanFundClaimIdList()
        {
            List<int> ret = new();
            var cfgs = ConfigCenter.ActivityFundCfgColl.GetDataByType(1);
            foreach (var item in cfgs)
            {
                if (item.Factor <= Days && !CheckFundIsClaimed(EPassportRewardType.PassportTypeFree,item.Id))
                {
                    ret.Add(item.Id);
                } else if (FundPlus && item.Factor <= Days && !CheckFundIsClaimed(EPassportRewardType.PassportTypeVip,item.Id))
                {
                    ret.Add(item.Id);
                }
            }
            return ret;
        }

        /// <summary>
        /// 更新领取记录
        /// </summary>
        /// <param name="list"></param>
        public void UpdateRecordList(List<int> list)
        {
            foreach (var id in list)
            {
                UpdateRecordList(id);
            }
        }
        /// <summary>
        /// 更新领取记录
        /// </summary>
        /// <param name="id"></param>
        public void UpdateRecordList(int id)
        {
            ClaimRecord.Add(id);
        }

        public void UpdateDrawRecordList(List<int> list, bool isLimit)
        {
            foreach (var id in list)
            {
                if (!drawRecord.TryGetValue(id, out var value))
                {
                    drawRecord.Add(id, 0);
                }

                value += 1;
                drawRecord[id] = value;
            }

            if (isLimit)
            {
                UpdateLimitDrawRecord();
            }
        }

        public void UpdateMagicFundClaimedList(List<int> freeList, List<int> plusList)
        {
            foreach (var id in freeList)
            {
                fundClaimed.Add(id);
            }
            foreach (var id in plusList)
            {
                fundPlusClaimed.Add(id);
            }
        }

        /// <summary>
        /// 是否触发红点
        /// </summary>
        /// <returns></returns>
        public bool CheckIsShowRedDot()
        {
            var ret = CheckIsValid();
            if (!ret) return false;
            // 进度检查
            var list = GetCanClaimIdList();
            if (list.Count > 0) return true;
            // 检查基金
            var fundList = GetCanFundClaimIdList();
            if (fundList.Count > 0) return true;
            if (IsClickAdBtn()) return true;
            return false;
        }

        /// <summary>
        /// 检查活动是否在有效期内
        /// </summary>
        /// <returns></returns>
        public bool CheckIsValid()
        {
            return ServerTime.Instance.IsOpenTime(StartStamp, EndStamp);
        }
        
        public bool IsSkip
        {
            get
            {
                var key = GetIsSkipKey();
                return DHUnityUtil.PlayerPrefs.GetInt(key, 0) != 0;
            }
            set
            {
                var key = GetIsSkipKey();
                DHUnityUtil.PlayerPrefs.SetInt(key, value? 1 : 0);
            }
        }

        private string GetIsSkipKey()
        {
            return  $"{DataCenter.charcaterData.Digest.RoleId}_{GameConst.MagicDrawKey}";
        }

        /// <summary>
        /// 获取抽中的记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetDrawRecord(int id)
        {
            return DrawRecord.ContainsKey(id) ? DrawRecord[id] : 0;
        }

        public bool IsClickAdBtn()
        {
            var countCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.MagicPray_02);
            if (countCfg == null || countCfg.Content == null || countCfg.Content.Count <= 0)
                return false;
            if (!CheckRewardIsEnough(1)) return false;
            return countCfg.Content[0] > AdCount;
        }

        /// <summary>
        /// 检查奖励是否足够
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool CheckRewardIsEnough(int count)
        {
            var leftRewardCount = GetLeftRewardCount();
            return leftRewardCount >= count;
        }
        
        public int GetLeftRewardCount()
        {
            var cfgs = ConfigCenter.PrayJackpotCfgColl.DataItems;
            var leftRewardCount = 0;
            foreach (var cfg in cfgs)
            {
                leftRewardCount += cfg.Frequency - GetDrawRecord(cfg.Id);
            }
            return leftRewardCount;
        }

        /// <summary>
        ///  抽中限定
        /// </summary>
        private void UpdateLimitDrawRecord()
        {
            var cfgs = ConfigCenter.PrayJackpotCfgColl.DataItems;
            var leftRewardCount = 0;
            foreach (var cfg in cfgs)
            {
                if (!DrawRecord.TryGetValue(cfg.Id, out int count))
                {
                    DrawRecord.Add(cfg.Id, 0);
                }

                DrawRecord[cfg.Id] = cfg.Frequency;
            }
        }
        
        public int GetSelectIndex(int cfgId)
        {
            if(!OptionalRecord.ContainsKey(cfgId)) return -1;
            return OptionalRecord[cfgId];
        }
        public void SetSelectIndex(int cfgId, int index)
        {
            OptionalRecord[cfgId] = index;
        }
    }
}