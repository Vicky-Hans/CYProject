using System.Collections.Generic;
using DH.Config;
using DH.Proto;

namespace DH.Data
{
    [ProtoWrap(typeof(AllPassport))]
    public partial class AllPassportData : BaseData
    {
        protected override void ClearData()
        {
            Data.Clear();
            base.ClearData();
        }
        /// <summary>
        /// 检查是否已领取
        /// </summary>
        /// <param name="type"></param>
        /// <param name="rewardType"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool CheckIsClaimed(EPassPortType type, EPassportRewardType rewardType, int level)
        {
            if (GetPassportData(type) == null)
            {
                return false;
            }
            var passportData = GetPassportData(type);
            
            if (rewardType == EPassportRewardType.PassportTypeFree)
            {
                return level <= passportData.FreeClaimLv;
            }
            else
            {
                return level <= passportData.PlusClaimLv;
            }
        }
        
        /// <summary>
        /// 获取指定的通行证数据
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Passport GetPassportData(EPassPortType type)
        {
            if (!Data.TryGetValue((int)type, out Passport passportData))
            {
                return null;
            }
            return passportData;
        }
        
        /// <summary>
        /// 检查是否可领取
        /// </summary>
        /// <param name="type"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool CheckIsCanClaim(EPassPortType type, int level)
        {
            if(GetPassportData(type) == null)
            {
                return false;
            }

            var passportData = GetPassportData(type);
            return level <= passportData.Lv;
        }

        /// <summary>
        /// 是否触发红点
        
        /// </summary>
        /// <returns></returns>
        public bool CheckIsShowRedDot()
        {
            var ret = false;
            foreach (var item in Data)
            {
                ret =  CheckIsShowRedDot((EPassPortType)item.Key);
                if(ret) break;
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool CheckIsShowRedDot(EPassPortType type)
        {
            if(GetPassportData(type) == null)
            {
                return false;
            }
            var passportData = GetPassportData(type);
            return passportData.Lv > passportData.FreeClaimLv || (passportData.Plus && passportData.Lv > passportData.PlusClaimLv);
        }

        /// <summary>
        /// 获取当前展示的轮次配置
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<FundRewardsCfg> GetShowConfigs(EPassPortType type)
        {
            List<FundRewardsCfg> retList = new();
            if(GetPassportData(type) == null)
            {
                return retList;
            }
            var passportData = GetPassportData(type);
            var cfgs = ConfigCenter.FundRewardsCfgColl.DataItems;
            foreach (var cfg in cfgs)
            {
                if(cfg.Type != (int)type) continue;
                if (cfg.Rounds == passportData.Version)
                {
                    retList.Add(cfg);
                }
            }
            retList.Sort((a, b) => a.Factor.CompareTo(b.Factor));
            return retList;
        }

        /// <summary>
        /// 检查是否还在有效期内
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool CheckIsVail(EPassPortType type)
        {
            if(GetPassportData(type) == null)
            {
                return false;
            }

            var passportData = GetPassportData(type);
            return ServerTime.Instance.RemainTime(passportData.EndTime) > 0;
        }
        public bool CheckIsVail()
        {
            bool ret = false;
            foreach (var item in data)
            {
                ret = CheckIsVail((EPassPortType)item.Key);
                if (ret) break;
            }
            return ret;
        }

        /// <summary>
        /// 领取了，跟新数据
        /// </summary>
        /// <param name="type"></param>
        public void UpdateClaimedInfo(EPassPortType type)
        {
            if(GetPassportData(type) == null)
            {
                return;
            }

            var passportData = GetPassportData(type);
            passportData.FreeClaimLv = passportData.Lv;
            if (passportData.Plus)
            {
                passportData.PlusClaimLv = passportData.Lv;
            }
        }

        public EPassPortType GetDefaultPassportType()
        {
            if (CheckIsVail(EPassPortType.PassportTypeDiscount))
            {
                return EPassPortType.PassportTypeDiscount;
            } 
            if (CheckIsVail(EPassPortType.PassportTypeStone))
            {
                return EPassPortType.PassportTypeStone;
            }
            return EPassPortType.PassportTypeDiscount;
        }
    }
}