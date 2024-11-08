using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;

namespace DH.Data
{
    [ProtoWrap(typeof(GiftPack))]
    public partial class GiftPackData : BaseData
    {
        [AutoNotify] private ObservableDictionary<int,int> optionSelect = new();
        [AutoNotify] private ObservableDictionary<int,int> optionStageSelect = new();
        public long GetCanGetTime()
        {
            return ZeroBuy;
        }

        public bool IsBuyGift()
        {
            return ZeroBuy > 0;
        }

        public bool IsGetReward(int id)
        {
            return ZeroClaim.Contains(id);
        }
        
        public bool IsSelectOptionalReward(int id)
        {
            return GetOptionalSelectIndex(id) != -1;
        }

        public int GetOptionalSelectIndex(int id)
        {
            if (OptionSelect.ContainsKey(id))
            {
                return OptionSelect[id];
            }
            return -1;
        }
        
        public void SetOptionalSelectIndex(int id,int selectIndex)
        {
            OptionSelect[id] = selectIndex;
        }
        
        public bool IsSelectStateOptionalReward(int id)
        {
            return GetStateOptionalSelectIndex(id) != -1;
        }

        public int GetStateOptionalSelectIndex(int id)
        {
            if (optionStageSelect.ContainsKey(id))
            {
                return optionStageSelect[id];
            }
            return -1;
        }
        
        public void SetStateOptionalSelectIndex(int id,int selectIndex)
        {
            optionStageSelect[id] = selectIndex;
        }
    }
}
