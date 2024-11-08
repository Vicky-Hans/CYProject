using DH.Config;
using DH.Proto;


namespace DH.Data
{
    [ProtoWrap(typeof(AutumnSpecialSync))]
    public partial class AutumnSpecialData : BaseData
    {
        public int GetBuyNum(int id)
        {
            if (packRecord.TryGetValue(id, out var num))
            {
                return num;
            }

            return 0;
        }

        public bool IsCanGetAward(int id)
        {
            if (!packRecord.ContainsKey(id))
            {
                return true;
            }
            var cfg = ConfigCenter.PackageCfgColl.GetDataById(id);
            return packRecord[id] < cfg.BuyLimit;
        }

        public void GetAward(int id)
        {
            if (!PackRecord.ContainsKey(id))
            {
                PackRecord.Add(id,1);
            }
            else
            {
                PackRecord[id]++;
            }
            RaisePropertyChanged(nameof(PackRecord));
        }

        public bool IsBuyAllOver()
        {
            var lists = ConfigCenter.PackageCfgColl.GetDataByRule((int)EPackageType.AutumnSpecial);
            for (int i = 0; i < lists.Count; i++)
            {
                if (IsCanGetAward(lists[i].Id))
                {
                    return false;
                }
            }
            return true;
        }

        #region 自选缓存

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
