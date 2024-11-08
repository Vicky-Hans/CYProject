using System.Collections.Generic;
using DH.Config;
using DH.Proto;
using DH.UIFramework;

namespace DH.Data
{
    [ProtoWrap(typeof(LuckyTripSync))]
    public partial class LuckyTravelData : BaseData
    {
        [AutoNotify] private int curTipsInfoSelectIndex;
        [AutoNotify] private int superRewardNum;

        public void RefreshCurTipsInfoSelectIndex()
        {
            RaisePropertyChanged(nameof(CurTipsInfoSelectIndex));
        }
        public bool CheckProgressFinish(int id)
        {
            return ClaimRecord.Contains(id);
        }
        
        public void SetScoreClaimed(int id)
        {
            ClaimRecord.Add(id);
        }
        
        public bool IsSelectOptionalReward(int id)
        {
            return GetOptionalSelectIndex(id) != -1;
        }

        public int GetOptionalSelectIndex(int id)
        {
            if (OptionalRecord.ContainsKey(id))
            {
                return OptionalRecord[id];
            }

            return -1;
        }
        
        public void SetOptionalSelectIndex(int id,int selectIndex)
        {
            OptionalRecord[id] = selectIndex;
        }
        
      
    }
}
