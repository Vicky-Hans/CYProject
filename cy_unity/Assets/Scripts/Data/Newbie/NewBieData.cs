using DH.Proto;

namespace DH.Data
{
    [ProtoWrap(typeof(FirstCharge))]
    public partial class NewBieData : BaseData
    {
        /// <summary>
        /// 是否已经够买
        /// </summary>
        public bool IsBuy => BuyStamp != 0;
        

        public void BuyGift()
        {
            if (IsBuy)return;
            BuyStamp = ServerTime.Instance.GetNowTime();
            Claim = 1;
            RaisePropertyChanged(nameof(IsBuy));
        }

        /// <summary>
        /// 活动结束
        /// </summary>
        /// <returns></returns>
        public bool Isover()
        {
            return Claim >= 3;
        }

        public bool IsGetAward(int day)
        {
            return Claim >= day;
        }

        /// <summary>
        /// 红点用
        /// </summary>
        /// <returns></returns>
        public bool IsCnaGetAward()
        {
            if (!IsBuy) return false;
            return ((int)(ServerTime.Instance.GetNowTime()- BuyStamp) / 86400) +1 > Claim;
        }

    }

}