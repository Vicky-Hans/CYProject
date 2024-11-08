using System.ComponentModel;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;

namespace DH.Data
{
    [ProtoWrap(typeof(Shop))]
    public partial class ShopData : BaseData
    {
        [AutoNotify] private ObservableDictionary<int, int> packageRecord = new(); 
        protected override void InitData()
        {
          
        }

        protected override void ClearData()
        {
            base.ClearData();
        }

        public void RefreshShopGoods()
        {
            RaisePropertyChanged(nameof(DailyShop));
        }

        public void SetDrawFreeCnt(int id)
        {
            if (id == 1 && Recruit.CommFree>0)
            {
                Recruit.CommFree -= 1;
            }else if (id == 2 && Recruit.CollectFree>0)
            {
                Recruit.CollectFree -= 1;
            }
        }

        public void SetBoxInfo(int lv, int exp)
        {
            Recruit.Lv = lv;
            Recruit.Exp = exp;
        }
        
        public int GetBoxInfoLv()
        {
            return Recruit.Lv;
        }
        
        public int GetBoxInfoExp()
        {
            return Recruit.Exp;
        }

        public void SetChapterGift(int chapterId)
        {
            ChapterGift[chapterId] = 0;
        }
        
        public bool CheckChapterGift(int chapterId)
        {
            return ChapterGift.ContainsKey(chapterId);
        }

        public ShopGoodsData GetDailyData(int dailyId)
        {
            if (DailyShop.TryGetValue(dailyId, out ShopGoodsData value))
            {
                return value;
            }
            return null;
        }

        public void SetShopGoodsLimit(int dailyId,int cnt=1)
        {
            if (DailyShop.TryGetValue(dailyId, out ShopGoodsData data))
            {
                if (data.Limit > 0)
                {
                    data.Limit -= cnt;
                }
            }
        }

        public void SetDiamondShop(int rechargeId)
        {
            if (!DiamondDouble.ContainsKey(rechargeId))
            {
                DiamondDouble.Add(rechargeId,1);
            }
            else
            {
                DiamondDouble[rechargeId] += 1;
            }
        }

        public bool CheckDiamondShop(int rechargeId)
        {
            return DiamondDouble.ContainsKey(rechargeId);
        }

        public int GetGoldCnt()
        {
            var limitCnt = 3 - CoinFree;
            return CoinFree>=0?limitCnt:4;
        }
        
        public void SetAddGoldCnt()
        {
            CoinFree--;
        }

        public void SetGuarantee(int min,int max)
        {
            Recruit.MinGuarantee = min;
            Recruit.MaxGuarantee = max;
        }

        public long GetDiamondBuyCnt(int packageId)
        {
            if (DiamondDouble.TryGetValue(packageId, out var value))
            {
                return value;
            }

            return 0;
        }
        
        public bool IsSelectOptionalReward(int id)
        {
            return GetOptionalSelectIndex(id) != -1;
        }

        public int GetOptionalSelectIndex(int id)
        {
            if (PackageRecord.ContainsKey(id))
            {
                return PackageRecord[id];
            }

            return -1;
        }
        
        public void SetOptionalSelectIndex(int id,int selectIndex)
        {
            PackageRecord[id] = selectIndex;
        }
    }
    
    [ProtoWrap(typeof(Recruit))]
    public partial class RecruitData : BaseData
    {
        
    }
    
    [ProtoWrap(typeof(ShopGoods))]
    public partial class ShopGoodsData : BaseData
    {
        
    }
}