using System.Collections.Generic;
using System.Linq;
using DH.Config;
using DH.Proto;

namespace DH.Data
{
    [ProtoWrap(typeof(DailyPack))]
    public partial class DailyPackData : BaseData
    {

        /// <summary>
        /// 每日免费是否领取
        /// </summary>
        /// <returns></returns>
        public bool FreeDayIsGet()
        {
            var items = ConfigCenter.DailySpecialPackageCfgColl.DataItems;
            return items.Any(item => item.Type == 0 && PackRecord.ContainsKey(item.Id));
        }

        public DailySpecialPackageCfg AllBuyCfg()
        {
            return ConfigCenter.DailySpecialPackageCfgColl.DataItems.FirstOrDefault(item => item.Type == 2);
        }
        public DailySpecialPackageCfg FreeCfg()
        {
            return ConfigCenter.DailySpecialPackageCfgColl.DataItems.FirstOrDefault(item => item.Type == 0);
        }
        public bool IsCanGetFree()
        {
            return !FreeDayIsGet();
        }

        public bool IsCanBuy(int id)
        {
           var cfg =  ConfigCenter.DailySpecialPackageCfgColl.GetDataById(id);
            if (cfg.Type == 1 && PackRecord.ContainsKey(AllBuyCfg().Id))
            {
                return false;
            }
            if (cfg.Type == 2 && ConfigCenter.DailySpecialPackageCfgColl.DataItems.Any(o=>o.Type==1 && PackRecord.ContainsKey(o.Id) ))
            {
                return false;
            }
            return !PackRecord.ContainsKey(id);
        }

        public bool IsBuy;
        public void Buy(int id)
        {
            if (PackRecord.ContainsKey(id))
            {
                PackRecord[id]++;
                RaisePropertyChanged(nameof(IsBuy));
                return;
            }

            PackRecord.Add(id, 0);
            RaisePropertyChanged(nameof(IsBuy));
        }
        /// <summary>
        /// 是否购买了礼包 0免费 1单个 2全部
        /// </summary>
        /// <param name="type"></param>
        public bool IsBuyGift(int type)
        {
           return  ConfigCenter.DailySpecialPackageCfgColl.DataItems.Any(o=>o.Type==type && PackRecord.ContainsKey(o.Id));
        }

        public bool IsButAllType(int type)
        {
            var items = ConfigCenter.DailySpecialPackageCfgColl.DataItems;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Type == type && !PackRecord.ContainsKey(items[i].Id))
                {
                    return false;
                }
            }

            return true;
        }

        #region 自选相关
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

    }
}