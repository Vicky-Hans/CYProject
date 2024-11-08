using System.Collections.Generic;
using DH.Config;
using DH.Proto;
using DH.UIFramework;

namespace DH.Data
{
    [ProtoWrap(typeof(LuckyDrawSync))]
    public partial class LuckyDrawData : BaseData
    {
        public override void Init()
        {
            base.Init();
        }

        protected override void ClearData()
        {
            ClaimRecord.Clear();
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

        public List<int> GetCanClaimIdList()
        {
            List<int> ret = new();
            //进度奖励
            var cfgs = ConfigCenter.StageRewardCfgColl.DataItems;
            foreach (var item in cfgs)
            {
                if (item.Type != 1) continue;
                if(!CheckIsCanClaim(item.Id)) continue;
                if(CheckIsClaimed(item.Id)) continue;
                ret.Add(item.Id);
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
                ClaimRecord.Add(id);
            }
        }

        /// <summary>
        /// 是否触发红点
        /// </summary>
        /// <returns></returns>
        public bool CheckIsShowRedDot()
        {
            if(IsClickAdBtn()) return true;
            var list = GetCanClaimIdList();
            return list.Count > 0;
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
            return  $"{DataCenter.charcaterData.Digest.RoleId}_{GameConst.LuckDrawKey}";
        }

        public bool IsClickAdBtn()
        {
            if (!DataCenter.luckyDrawData.CheckIsCanOp(ELuckDrawOpType.LuckDrawOpAd))
            {
                return false;
            }
            var countCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Turntable_07);
            if (countCfg == null || countCfg.Content == null || countCfg.Content.Count <= 0)
                return false;
            return countCfg.Content[0] > AdCount;
        }

        /// <summary>
        /// 获取当天剩余抽奖次数
        /// </summary>
        /// <returns></returns>
        public int GetCurDayEndCount()
        {
            var countCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Turntable_03);
            if (countCfg is { Content: not null } && countCfg.Content.Count != 0)
            {
                var totalCount = countCfg.Content[0];
                return totalCount - DrawDayTimes;
            }

            return 0;
        }

        /// <summary>
        ///  检查是否可以操作
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool CheckIsCanOp(ELuckDrawOpType type)
        {
            switch (type)
            {
                case ELuckDrawOpType.LuckDrawOpAd:
                case ELuckDrawOpType.LuckDrawOpOneTimes:
                    return GetCurDayEndCount() >= 1;
                case ELuckDrawOpType.LuckDrawOpFiveTimes:
                    return GetCurDayEndCount() >= 5;
            }

            return false;
        }

        public int GetSelectIndex(int cfgId)
        {
            return -1;
        }
        public void SetSelectIndex(int cfgId, int index)
        {
            
        }
    }
}