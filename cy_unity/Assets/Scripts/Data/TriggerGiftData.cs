using System.Linq;
using DH.Config;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;

namespace DH.Data
{
    [ProtoWrap(typeof(TriggerGiftOne))]
    public partial class TriggerGiftOneData : BaseData
    {

    }

    [ProtoWrap(typeof(TriggerGift))]
    public partial class TriggerGiftData : BaseData
    {
        [AutoNotify] private bool isPopup = false;

        public long GetTriggerGiftEndTime(int type)
        {
            return Data.ContainsKey(type) ? Data[type].EndStamp : 0;
        }

        public bool IsBuyTriggerGift(int type,int id)
        {
            if (Data.ContainsKey(type))
            {
                var cfg = ConfigCenter.TriggerGiftCfgColl.GetDataById(id);
                return Data[type].Record[id] >= cfg.BuyLimit;
            }
            return false;
        }

        public int BuyTriggerGiftNums(int type,int id)
        {
            if (Data.ContainsKey(type))
            {
                return Data[type].Record[id];
            }
            return 0;
        }

        public bool IsOpenTriggerGift(int type)
        {
            if (Data.ContainsKey(type))
            {
                return ServerTime.Instance.IsOpenTime(Data[type].EndStamp);
            }
            return false;
        }

        public void BuyTriggerGift(int id)
        {
            var triggerCfg = ConfigCenter.TriggerGiftCfgColl.GetDataById(id);
            if(triggerCfg==null) return;
            if (Data.ContainsKey(triggerCfg.Type))
            {
                if (Data[triggerCfg.Type].Record.ContainsKey(id))
                {
                    Data[triggerCfg.Type].Record[id]++;
                }
                else
                {
                    Data[triggerCfg.Type].Record.Add(id,1);
                }
            }
            RaisePropertyChanged(nameof(Data));
        }

        public bool CheckPreData(int type)
        {
            return PreData.Contains(type);
        }

        public void RefreshData()
        {
            RaisePropertyChanged(nameof(Data));
        }
        #region 商店自选缓存
        
        public void SetSelectPacket(int packetId, int index)
        {
            OptionalRecord[packetId] = index;
            RaisePropertyChanged(nameof(OptionalRecord));
        }
        
        public int GetSelectPacket(int packetId)
        {
            if (OptionalRecord.ContainsKey(packetId))
            {
                return OptionalRecord[packetId];
            }
            return -1;
        }

        public bool CheckSelectPacket(int packetId)
        {
            return GetSelectPacket(packetId) != -1;
        }
        
        #endregion

        #region 助力礼包

        public bool CheckIsShowBoosterPackRedDot()
        {
            return false;
        }

        private string GetBoosterPackInfoKey()
        {
            return $"{DataCenter.charcaterData.Digest.RoleId}_BoosterPackInfo";
        }

        public bool CheckIsPopupBoosterPack(int id)
        {
            var key = GetBoosterPackInfoKey();
            return DHUnityUtil.PlayerPrefs.GetInt($"{key}_{id}", 0) != 0;
        }
        
        public EBoosterPackState GetCurBoosterPackState( TriggerGiftCfg cfg)
        {
            var curTriggerData = Data.ContainsKey(cfg.Type) ? Data[cfg.Type] : null;
            if(curTriggerData == null)
            {
                return EBoosterPackState.Lock;
            }
            if (cfg.FrontId == 0)
            {
                if (curTriggerData.Record.ContainsKey(cfg.Id) && curTriggerData.Record[cfg.Id] >= cfg.BuyLimit)
                {
                    // 买完了
                    return EBoosterPackState.SoldOut;
                }
                // 可以买
                return EBoosterPackState.CanOp;
            }
            // 前置条件 未达成
            if (!curTriggerData.Record.ContainsKey(cfg.FrontId))
            {
                // 锁住
                return EBoosterPackState.Lock;
            }
		    
            var frontCfg = ConfigCenter.TriggerGiftCfgColl.GetDataById(cfg.FrontId);
            if (frontCfg!=null && curTriggerData.Record[frontCfg.Id] < frontCfg.BuyLimit)
            {
                return EBoosterPackState.Lock;
            }

            if (curTriggerData.Record[cfg.Id] < cfg.BuyLimit)
            {
                return EBoosterPackState.CanOp;
            }
            return EBoosterPackState.SoldOut;
        } 

        public bool CheckIsCanOp(int type)
        {
            var items = ConfigCenter.TriggerGiftCfgColl.GetDataByType(type);
            for (int i = 0; i < items.Count; i++)
            {
                var cfg = items[i];
                var state = GetCurBoosterPackState(cfg);
                if (state != EBoosterPackState.SoldOut)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 设置弹窗是否弹出
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isPopup"></param>
        public void SetBoosterPackInfo(int id, bool isPopup)
        {
            var key = GetBoosterPackInfoKey();
            DHUnityUtil.PlayerPrefs.SetInt($"{key}_{id}", isPopup ? 1 : 0);
        }

        /// <summary>
        /// 获取已经购买的次数
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cfgId"></param>
        /// <returns></returns>
        public int GetAlreadyBuyCount(int type, int cfgId)
        {
            if (!Data.ContainsKey(type)) return 0;
            return Data[type].Record.ContainsKey(cfgId) ? Data[type].Record[cfgId] : 0;
        }
        

        #endregion


        #region 累充天数

        public bool IsGetProgressAward(int id)
        {
            return claimRecord.Contains(id);
        }

        public int GetLastLevel(int id)
        {
            var cfg = ConfigCenter.StageRewardCfgColl.GetDataById(id - 1);
             if (cfg == null)
             {
                 return 0;
             }
             if (cfg.Type != (int)ActivityStageType.TriggerGiftProgress)
                 return 0;
             return cfg.Level;

        }

        public void GetProgressAward(int id)
        {
            if (!ClaimRecord.Contains(id))
            {
                ClaimRecord.Add(id);
                RaisePropertyChanged(nameof(ClaimRecord));
            }
        }

        public int GetProgressGiftNowIndex()
        {
            var configItems = ConfigCenter.StageRewardCfgColl.GetDataByType((int)ActivityStageType.TriggerGiftProgress);
            configItems = configItems.OrderBy(o => o.Level).ToList();
            
            for (int i = 0; i < configItems.Count; i++)
            {
                if (BuyDay >= configItems[i].Level && !ClaimRecord.Contains( configItems[i].Id))
                {
                    return i;
                }

                if ( BuyDay < configItems[i].Level && !ClaimRecord.Contains( configItems[i].Id))
                {
                    return i;
                }
            }

            return 0;
        }

        public bool IsProgressRed()
        {
            var configItems = ConfigCenter.StageRewardCfgColl.GetDataByType((int)ActivityStageType.TriggerGiftProgress);
            configItems = configItems.OrderBy(o => o.Level).ToList();
            for (int i = 0; i < configItems.Count; i++)
            {
                if (BuyDay >= configItems[i].Level && !ClaimRecord.Contains( configItems[i].Id))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
