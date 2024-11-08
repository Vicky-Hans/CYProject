using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using DH.Proto;
using DH.UIFramework;

namespace DH.Game
{
    public partial class ActivityUIManager : ObservableSingleton<ActivityUIManager>
    {
        #region 幸运扭蛋 变量

        [AutoNotify] private bool luckEggDrawIng;
        #endregion
        
        public List<ActivityFundCfg> GetActivityFund(ActivityFund type)
        {
            return  ConfigCenter.ActivityFundCfgColl.GetDataByType((int)type);
        }

        #region 幸运之旅

        public bool CheckLuckTravelOpen()
        {
            return ServerTime.Instance.IsOpenTime(DataCenter.luckyTravelData.StartStamp,DataCenter.luckyTravelData.EndStamp);
        }

        public bool CheckLuckTravelRed()
        {
            return CheckLuckTravelProgressRed() || CheckLuckTravelFreeRed();
        }

        public bool CheckLuckTravelFreeRed()
        {
            var cfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.LuckyJourney_04);
            var limit = cfg.Content[0] - DataCenter.luckyTravelData.TodayDraw;
            return DataCenter.luckyTravelData.AdCount == 0 && limit>0;
        }

        public bool CheckLuckTravelProgressRed()
        {
            var list = UIHelper.GetAllStageRewardList(ActivityStageType.LuckTravel);
            foreach (var item in list)
            {
                if (!DataCenter.luckyTravelData.CheckProgressFinish(item.Id) && item.Level<= DataCenter.luckyTravelData.Progress)
                {
                    return true;
                }
            }
            return false;
        }

        public async UniTaskVoid SendLuckTravelTipseRank(Action<List<LuckyTripRecord>,List<Resource>> succeedAction=null,Action failAction=null)
        {
            var result = await GameNetworkManager.Instance.SendAsync<RspLuckyTripPullRecord>(new ReqLuckyTripPullRecord());
            NetHelper.CheckNetErrorMessage(result.rsp, true, () =>
            {
                succeedAction?.Invoke(result.rsp.Record.ToList(),result.rsp.Reward.ToList());
            },failAction);
        }

        public async UniTaskVoid SendDrawLuckTravel(int op, int count, Action<RspLuckyTrip> succeedAction = null, Action failAction = null)
        {
            var req = new ReqLuckyTrip
            {
                Op = op,
                Count = count
            };

            var result = await GameNetworkManager.Instance.SendAsync<RspLuckyTrip>(req);
            NetHelper.CheckNetErrorMessage(result.rsp, true, () =>
            {
                DataCenter.luckyTravelData.Progress = result.rsp.Progress;
                Lodash.DealRewards(result.rsp.Reward.ToList(), result.rsp.Cost.ToList());
                Lodash.DealRewards(result.rsp.Prize.ToList());
                if (op == 1)
                {
                    DataCenter.luckyTravelData.AdCount += count;
                }

                DataCenter.luckyTravelData.TodayDraw += count;
                succeedAction?.Invoke(result.rsp);
            }, failAction);
        }

        #endregion

        #region 0元购
        
        public bool CheckAllGetReward()
        {
            var isBuy = DataCenter.giftPackData.IsBuyGift();
            if (!isBuy) return false;
            var cfgList = ConfigCenter.PurchaseRewardCfgColl.DataItems.ToList();
            for (int i = 0; i < cfgList.Count; i++)
            {
                if (!DataCenter.giftPackData.IsGetReward(cfgList[i].Id))
                {
                    return false;
                }
            }
            return true;
        }
        
        public bool CheckFreeBuyRed()
        {
            var isBuy = DataCenter.giftPackData.IsBuyGift();
            if (!isBuy)
            {
                var list = ConfigCenter.PackageCfgColl.GetDataByRule((int)EPackageType.FreeBuy);
                if (list != null && list.Count > 0)
                {
                    var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(list[0].Id);
                    if (packageCfg != null)
                    {
                        return UIHelper.CheckRewardIsEnough(packageCfg.Cost);
                    }
                }
            }
            var cfgList = ConfigCenter.PurchaseRewardCfgColl.DataItems.ToList();
            for (int i = 0; i < cfgList.Count; i++)
            {
                if (!DataCenter.giftPackData.IsGetReward(cfgList[i].Id) && !ServerTime.Instance.IsOpenTime(DataCenter.giftPackData.ZeroBuy + cfgList[i].Time * 3600))
                {
                    return true;
                }
            }
            return false;
        }

        public async UniTaskVoid SendGiftPackZeroClaim(int id, int index = -1, Action succeed = null)
        {
            var req = new ReqGiftPackZeroClaim()
            {
                Id = id,
                Index = index
            };

            var result = await GameNetworkManager.Instance.SendAsync<RspGiftPackZeroClaim>(req);
            NetHelper.CheckNetErrorMessage(result.rsp, true, () =>
            {
                DataCenter.giftPackData.ZeroClaim.Add(id);
                Lodash.DealRewards(result.rsp.Reward.ToList());
                UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList());
                succeed?.Invoke();
            });
        }

        #endregion

        #region 幸运扭蛋
            
        public void OpenBuyEggCoin()
        {
            var itemCfg = ConfigCenter.ItemCfgColl.GetDataById((int)GameConst.ItemIdCode.EggCoin);
            if (itemCfg != null)
            {
                var selectModel = new ShopSelectLimitViewModel(new Reward(RewardType.Item,(int)GameConst.ItemIdCode.EggCoin,1),UIHelper.GetDiamond((long)itemCfg.GemPrice),
                    (selectNum) =>
                    {
                        if(!UIHelper.CheckRewardIsEnough(UIHelper.GetDiamond((long)itemCfg.GemPrice),true,selectNum)) return;
                        if(UIHelper.GetDefinesInt(DefineCfgId.Gachapon_02) - DataCenter.luckyEggData.DayBuyCount<=0)
                        {
                            ToastManager.ShowLanguage(GlobalLanguageId.General_tips20);
                            return;
                        }
                        SendLuckEggBuyItem(selectNum).Forget();
                    },UIHelper.GetDefinesInt(DefineCfgId.Gachapon_02) - DataCenter.luckyEggData.DayBuyCount);
                UIManager.Instance.OpenDialog<ShopSelectLimitView>(selectModel).Forget();
            }
        }
        
        /// <summary>
        /// 扭蛋界面tab
        /// </summary>
        private LuckEggShowView eggTabType = LuckEggShowView.Main;
        public LuckEggShowView EggTabType
        {
            get => eggTabType;
            set
            {
                if (eggTabType == value) return;
                Set(ref eggTabType, value);
            }
        }
        
        private MagicBingoShowType magicBingo = MagicBingoShowType.Main;
        public MagicBingoShowType BagicBingo
        {
            get => magicBingo;
            set
            {
                if (magicBingo == value) return;
                Set(ref magicBingo, value);
            }
        }
        
        
            /// <summary>
            /// 显示时间
            /// </summary>
            /// <returns></returns>
            public bool CheckLuckEggShowTime()
            {
                return ServerTime.Instance.IsOpenTime(DataCenter.luckyEggData.StartStamp,DataCenter.luckyEggData.EndExchangeStamp);
            }
            
            /// <summary>
            /// 开启时间
            /// </summary>
            /// <returns></returns>
            public bool CheckLuckEggOpenTime()
            {
                return ServerTime.Instance.IsOpenTime(DataCenter.luckyEggData.StartStamp,DataCenter.luckyEggData.EndStamp);
            }

            public bool CheckLuckEggAllRed()
            {
                return CheckLuckEggDrawRed() || DataCenter.luckyEggData.IsTaskRed() || DataCenter.luckyEggData.IsFundRed();
            }

            public bool CheckLuckEggDrawRed()
            {
                var list = UIHelper.GetAllStageRewardList(ActivityStageType.LuckEgg);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Level <= DataCenter.luckyEggData.Count && !DataCenter.luckyEggData.CheckProgressFinish(list[i].Id) && CheckLuckEggOpenTime())
                    {
                        return true;
                    }
                }
                return false;
            }


            /// <summary>
            /// 领取记录
            /// </summary>
            /// <param name="id">记录id</param>
            /// <param name="index">自选奖励</param>
            public async UniTaskVoid SendLuckEggClaim(int id,int index = -1)
            {
                if(!CheckLuckEggOpenTime()) return;
                var req = new ReqLuckyEggClaim()
                {
                    Id = id,
                    OptionalIndex = index
                };
                var result =await GameNetworkManager.Instance.SendAsync<RspLuckyEggClaim>(req);
            
                if (NetHelper.CheckNetErrorMessage(result.rsp, true))
                {
                    Lodash.DealRewards(result.rsp.Reward.ToList());
                    DataCenter.luckyEggData.SetScoreClaimed(id);
                    UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList());
                }
            }
            
            /// <summary>
            /// 扭蛋抽奖
            /// </summary>
            /// <param name="op">操作方式 1-单抽 2-十连</param>
            /// <param name="succeedAction"></param>
            public async UniTaskVoid SendLuckEggDraw(int op,Action<List<Resource>> succeedAction = null,Action failAction = null)
            {
                if(!CheckLuckEggOpenTime()) return;
                var req = new ReqLuckyEggDraw()
                {
                    Op = op,
                };
                var result =await GameNetworkManager.Instance.SendAsync<RspLuckyEggDraw>(req);
                if (NetHelper.CheckNetErrorMessage(result.rsp, true))
                {
                    Lodash.DealRewards(result.rsp.Rewards.ToList(),result.rsp.Cost.ToList());
                    DataCenter.luckyEggData.DayDrawCount += op == 2 ? 10 : 1;
                    DataCenter.luckyEggData.Count += op == 2 ? 10 : 1;
                    succeedAction?.Invoke(result.rsp.Rewards.ToList());
                }
                else
                {
                    failAction?.Invoke();
                }
            }
            
            public async UniTaskVoid SendLuckEggBuyItem(int count=1,Action<List<Resource>> succeedAction = null)
            {
                if(!CheckLuckEggOpenTime()) return;
                var req = new ReqLuckyEggBuy()
                {
                    Count = count,
                };
                var result =await GameNetworkManager.Instance.SendAsync<RspLuckyEggBuy>(req);
                if (NetHelper.CheckNetErrorMessage(result.rsp, true))
                {
                    Lodash.DealRewards(result.rsp.Reward.ToList(),result.rsp.Cost.ToList());
                    UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList());
                    DataCenter.luckyEggData.DayBuyCount += count;
                    succeedAction?.Invoke(result.rsp.Reward.ToList());
                }
            }
        #endregion 
        
        #region bingo币购买

        [AutoNotify] private bool isShowMask;
        public void OpenBuyCoin()
        {
            var itemCfg = ConfigCenter.ItemCfgColl.GetDataById((int)GameConst.ItemIdCode.BinGoCoin);
            if (itemCfg != null)
            {
                var selectModel = new ShopSelectLimitViewModel(new Reward(RewardType.Item,(int)GameConst.ItemIdCode.BinGoCoin,1),UIHelper.GetDiamond((long)itemCfg.GemPrice),
                    (selectNum) =>
                    {
                        if(!UIHelper.CheckRewardIsEnough(UIHelper.GetDiamond((long)itemCfg.GemPrice),true,selectNum)) return;
                        if(UIHelper.GetDefinesInt(DefineCfgId.Bingo_04) - DataCenter.mgicBingoData.CoinBuy <=0)
                        {
                            ToastManager.ShowLanguage(GlobalLanguageId.General_tips20);
                            return;
                        }
                        SendCoinBuyItem(selectNum).Forget();
                    },UIHelper.GetDefinesInt(DefineCfgId.Bingo_04) - DataCenter.mgicBingoData.CoinBuy);
                UIManager.Instance.OpenDialog<ShopSelectLimitView>(selectModel).Forget();
            }
        }
        public async UniTaskVoid SendCoinBuyItem(int count=1,Action<List<Resource>> succeedAction = null)
        {
            if(DataCenter.mgicBingoData.IsTimeOver()) return;
            var req = new ReqBingoCoinBuy()
            {
                Count = count,
            };
            var result =await GameNetworkManager.Instance.SendAsync<RspBingoCoinBuy>(req);
            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                Lodash.DealRewards(result.rsp.Reward.ToList(),result.rsp.Cost.ToList());
                UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList());
                DataCenter.mgicBingoData.CoinBuy += count;
                succeedAction?.Invoke(result.rsp.Reward.ToList());
            }
        }
        #endregion
        
    }
}
