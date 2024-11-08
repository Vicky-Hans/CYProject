
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using DH.Proto;
using DH.UIFramework;
using Game.UI.MainUi;

namespace DH.Game
{
    public enum ShopTitle
    {
        None = 0,
        Draw=1,
        DisCount=2,
        Currency=3
    }

    public enum ShopBuyState
    {
        None,
        NoneFree,
        NoneAdv,
        Free,
        Adv,
        Item,
        Money,
        Finish,
    }

    public partial class ShopManager : ObservableSingleton<ShopManager>
    {
        [AutoNotify] public ShopTitle jumpSelectShopTile;
        [AutoNotify] private ShopTitle selectShopTitle;
        [AutoNotify] public bool isShowDrawRewardAnimation = false; 
        [AutoNotify] public bool isExistUpState = false; 
        [AutoNotify] public int curSelectChapterPos;

        public string GetTitleName(int id)
        {
            var cfgL = ConfigCenter.ShopLanguageCfgColl.GetDataById(id);
            return cfgL?.Name ?? string.Empty;
        }

        #region 招募相关

        public ShopBuyState GetEquipChestState(int id,bool isShowItem = true)
        {
            bool monthOpen = false;
            if (id == 3)
            {
                return ShopBuyState.Item;
            }

            if (id == 1)
            {
                if (DataCenter.shopData.Recruit.CommFree > 0)
                {
                    return monthOpen?ShopBuyState.Free:ShopBuyState.Adv;
                }
            }else if (id == 2)
            {
                if (DataCenter.shopData.Recruit.CollectFree > 0)
                {
                    return monthOpen?ShopBuyState.Free:ShopBuyState.Adv;
                }
            }

            if (!isShowItem)
            {
                return monthOpen?ShopBuyState.NoneFree:ShopBuyState.NoneAdv;
            }
            else
            {
                return ShopBuyState.Item;
            }

        }

        public Reward GetEquipChestItem(int id,bool isTen = false)
        {
            var cfg = ConfigCenter.EquipChestCfgColl.GetDataById(id);
            if (cfg != null)
            {
                if (!isTen)
                {
                    var reward = new Reward(RewardType.Item,cfg.ItemId,1);
                    var itemEnough = DataCenter.itemsData.CheckItemIsEnough(reward);
                    return itemEnough ? reward : UIHelper.GetDiamond(cfg.Purchase1);
                }
                else
                {
                    var reward = new Reward(RewardType.Item,cfg.ItemId,10);
                    var itemEnough = DataCenter.itemsData.CheckItemIsEnough(reward);
                    return itemEnough ? reward : UIHelper.GetDiamond(cfg.Purchase2);
                }
            }
            return null;
        }

        public List<EquipChestCfg> GetEquipChestList()
        {
            return ConfigCenter.EquipChestCfgColl.DataItems.ToList();
        }

        public string GetEquipChestIconPath(int id)
        {
            var cfg = ConfigCenter.EquipChestCfgColl.GetDataById(id);
            return GetEquipChestIconPath(cfg);
        }

        public string GetEquipChestIconPath(EquipChestCfg cfg)
        {
            if (cfg != null)
            {
                return cfg.Icon;
            }

            return UIHelper.NoneImagePath();
        }
        
        public string GetEquipChestName(int id)
        {
            return GetEquipChestName(ConfigCenter.EquipChestCfgColl.GetDataById(id));
        }
        public string GetEquipChestName(EquipChestCfg cfg)
        {
            if (cfg == null) return string.Empty;
            var cfgL = ConfigCenter.EquipChestLanguageCfgColl.GetDataById(cfg.Id);
            if (cfgL == null) return string.Empty;
            return cfgL.Name;
        }
        
        public string GetEquipChestDesc(EquipChestCfg cfg)
        {
            if (cfg == null) return string.Empty;
            var cfgL = ConfigCenter.EquipChestLanguageCfgColl.GetDataById(cfg.Id);
            if (cfgL == null) return string.Empty;
            return cfgL.Dec;
        }
        
        public string GetEquipChestBg(int id)
        {
            switch (id)
            {
                case 1: return "shop[shop_panel_21]";
                case 2: return "shop[shop_panel_20]";
                case 3: return "shop[shop_panel_5]";
                default:return "shop[shop_panel_21]";
            }
        }
        
        public string GetEquipChestBg(EquipChestCfg cfg)
        {
            return GetEquipChestBg(cfg?.Id ?? 1);
        }
        
        public string GetEquipChestBg1(int id)
        {
            return id==1?"shop[shop_panel_3]":"shop[shop_panel_5]";
        }
        
        public string GetEquipChestBg1(EquipChestCfg cfg)
        {
            return GetEquipChestBg1(cfg?.Id ?? 1);
        }
        public string GetEquipChestItemBg(int id)
        {
            switch (id)
            {
                case 1:return "shop[shop_panel_22]";
                case 2:return "shop[shop_panel_16]";
                case 3:return "shop[shop_panel_15]";
            }
            return "shop[shop_panel_15]";
        }
        #endregion
        
             
        #region 宝箱



        public bool IsEquipChestMax(int lv)
        {
            var cfg = ConfigCenter.EquipChestLvCfgColl.GetDataById(lv);
            if (cfg != null)
            {
                return cfg.Exp==0;
            }
            return false;
        }
        
        public int GetUpNeedExp(int lv)
        {
            var cfg = ConfigCenter.EquipChestLvCfgColl.GetDataById(lv);
            if (cfg != null)
            {
                return cfg.Exp;
            }
            return 0;
        }
        
        public List<Reward> GetBoxAddReward(int id,int lv)
        {
            List<Reward> list = new();
            var cfg = ConfigCenter.EquipChestLvCfgColl.GetDataById(lv);
            if (cfg != null)
            {
                switch (id)
                {
                    case 1: list.AddRange(cfg.RewardAdd1);break;
                    case 2: list.AddRange(cfg.RewardAdd2);break;
                    case 3: list.AddRange(cfg.RewardAdd3);break;
                }
            }
            return list;
        }
        
        public List<Reward> GetBoxAllReward(int id,int lv)
        {
            List<Reward> list = new();
            var cfgLv = ConfigCenter.EquipChestLvCfgColl.GetDataById(lv);
            if (cfgLv != null)
            {
                switch (id)
                {
                    case 1: list.AddRange(cfgLv.RewardAdd1); break;
                    case 2: list.AddRange(cfgLv.RewardAdd2); break;
                    case 3: list.AddRange(cfgLv.RewardAdd3); break;
                }
            }
            
            var cfg = ConfigCenter.EquipChestCfgColl.GetDataById(id);
            if (cfg != null)
            {
                foreach (var item in cfg.Item)
                {
                    bool isExist = false;
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].Type == item.Type && list[i].Id == item.Id)
                        {
                            isExist = true;
                            list[i] = new Reward(list[i].Type, list[i].Id, list[i].Count + item.Count);
                        }
                    }

                    if (!isExist)
                    {
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        #endregion

        #region 章节宝箱相关

        public bool IsOwnChapterGift()
        {
            var shopCfg = ConfigCenter.ShopCfgColl.GetDataById(1);
            if (shopCfg is { Content: not null })
            {
                for (int i = 0; i < shopCfg.Content.Count; i++)
                {
                    if (shopCfg.Content[i].Type == ShopType.Package)
                    {
                        var packCfg = ConfigCenter.PackageCfgColl.GetDataById(shopCfg.Content[i].Id);
                        if (packCfg != null)
                        {
                            if (DataCenter.mainStageData.IsPassChapter(packCfg.Condition))
                            {
                                return true;
                            }

                        }
                    }

                }
            }
            return false;
        }

        public List<PackageCfg> GetChapterGiftList()
        {
            bool isNext = false;
            List<PackageCfg> list = new();
            var shopCfg = ConfigCenter.ShopCfgColl.GetDataById(1);
            if (shopCfg is { Content: not null })
            {
                for (int i = 0; i < shopCfg.Content.Count; i++)
                {
                    if (shopCfg.Content[i].Type == ShopType.Package)
                    {
                        var packCfg = ConfigCenter.PackageCfgColl.GetDataById(shopCfg.Content[i].Id);
                        if (packCfg != null)
                        {
                            if (DataCenter.mainStageData.IsPassChapter(packCfg.Condition))
                            {
                                list.Add(packCfg);
                            }
                            else
                            {
                                if (!isNext)
                                {
                                    list.Add(packCfg);
                                    isNext = true;
                                }
                            }
                        }
                    }

                }
            }
            return list;
        }
        public int GetNextChapterGiftId()
        {
            var shopCfg = ConfigCenter.ShopCfgColl.GetDataById(1);
            if (shopCfg is { Content: not null })
            {
                for (int i = 0; i < shopCfg.Content.Count; i++)
                {
                    if (shopCfg.Content[i].Type == ShopType.Package)
                    {
                        var packCfg = ConfigCenter.PackageCfgColl.GetDataById(shopCfg.Content[i].Id);
                        if (packCfg != null)
                        {
                            if (!DataCenter.mainStageData.IsPassChapter(packCfg.Condition))
                            {
                                return packCfg.Id;
                            }
                        }
                    }

                }
            }
            return 0;
        }
        
        public bool IsShowChapterGift()
        {
            bool isNext = false;
            var shopCfg = ConfigCenter.ShopCfgColl.GetDataById(1);
            if (shopCfg is { Content: not null })
            {
                for (int i = 0; i < shopCfg.Content.Count; i++)
                {
                    if (shopCfg.Content[i].Type == ShopType.Package)
                    {
                        var packCfg = ConfigCenter.PackageCfgColl.GetDataById(shopCfg.Content[i].Id);
                        if (packCfg != null)
                        {
                            if(DataCenter.shopData.CheckChapterGift(packCfg.Id)) continue;
                            if (DataCenter.mainStageData.IsPassChapter(packCfg.Condition))
                            {
                                return true;
                            }
                            else
                            {
                                if (!isNext)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                }
            }
            return false;
        }


        #endregion

        #region 每日商店相关

        public List<DailyShopCfg> GetDailyList()
        {
            return ConfigCenter.DailyShopCfgColl.DataItems.ToList().FindAll(item=>MainUiManager.Instance.CheckFunctionIsUnlock(item.FunctionId));
        }

        // public string GetDailyName(int id,Reward reward)
        // {
        //     var cfg = ConfigCenter.DailyShopCfgColl.GetDataById(id);
        //     if (cfg != null && cfg.)
        //     {
        //         
        //     }
        //
        //     return string.Empty;
        // }

        public string GetDailyDiscountDesc(int id)
        {
            var cfgL = ConfigCenter.DiscountCfgColl.GetDataByDiscount(id);
            return cfgL?[0].Tag ?? string.Empty;
        }

        #endregion

        #region 钻石 金币商店

        public int GetLimitBuyCnt()
        {
            var cfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.shop_purchaseLimit);
            return cfg?.Content[0] ?? -1;
        }

        public List<int> GetDiamondShopList()
        {
            List<int> diamondList = new();
            var cfg = ConfigCenter.ShopCfgColl.GetDataById(4);
            if (cfg is { Content: { Count: > 0 } })
            {
                foreach (var item in cfg.Content)
                {
                    if(item.Type == ShopType.Package)
                        diamondList.Add(item.Id);
                }
            }

            return diamondList;
        }
        
        public List<int> GetGoldShopList()
        {
            List<int> goldList = new();
            var cfg = ConfigCenter.ShopCfgColl.GetDataById(5);
            if (cfg is { Content: { Count: > 0 } })
            {
                foreach (var item in cfg.Content)
                {
                    if(item.Type == ShopType.Package)
                        goldList.Add(item.Id);
                }
            }

            return goldList;
        }

        #endregion

        #region 服饰商店

        public string GetClothesDrawTips(bool big=false)
        {
            var cfg = ConfigCenter.EquipChestCfgColl.GetDataById((int)EquipChestId.Clothes);
            var cfgL = ConfigCenter.EquipChestLanguageCfgColl.GetDataById((int)EquipChestId.Clothes);
            // if (big)
            // {
            //     return $"{cfg.TCount[1] - DataCenter.shopData.Recruit.MaxGuarantee}次内必得S级装备";
            // }
            // else
            // {
            //     return $"{cfg.TCount[0] - DataCenter.shopData.Recruit.MinGuarantee}次内必得紫色装备";
            // }
            if (cfg != null && cfgL != null)
            {
                return string.Format(cfgL.Dec, cfg.TCount[1] - DataCenter.shopData.Recruit.MaxGuarantee,
                    cfg.TCount[0] - DataCenter.shopData.Recruit.MinGuarantee);
            }

            return string.Empty;
        }

        public string GetJackpotName(int id)
        {
            var cfg = ConfigCenter.JackpotLanguageCfgColl.GetDataById(id);
            return cfg?.ItemTypeName ?? string.Empty;
        }

        #endregion
        
        #region PackageCfg相关

        public string GetPackageDiscount(int id)
        {
            var cfgL = ConfigCenter.PackageCfgColl.GetDataById(id);
            if (cfgL != null)
            {
                return cfgL.Value;
            }

            return string.Empty;
        }

        public string GetPackageName(int id)
        {
            var cfgL = ConfigCenter.PackageLanguageCfgColl.GetDataById(id);
            if (cfgL != null)
            {
                return cfgL.PayName;
            }

            return string.Empty;
        }
        
        public string GetPackageIcon(int id)
        {
            var cfg = ConfigCenter.PackageCfgColl.GetDataById(id);
            if (cfg != null)
            {
                return cfg.Icon;
            }
            return UIHelper.NoneImagePath();
        }
        
        public string GetPackageDiscountDesc(int id)
        {
            var cfgL = ConfigCenter.PackageLanguageCfgColl.GetDataById(id);
            if (cfgL != null)
            {
                return cfgL.Value;
            }
            return string.Empty;
        }

        public ShopBuyState GetPackageState(int id,int buyCnt=0)
        {
            var cfg = ConfigCenter.PackageCfgColl.GetDataById(id);
            if (cfg == null) return ShopBuyState.None;
            if (cfg.RechargeId > 0)
            {
                return ShopBuyState.Money;
            }
            if(cfg.RechargeId==0 || (cfg.RechargeId == -2 && buyCnt>=3) || cfg.RechargeId == -3)
            {
                return ShopBuyState.Item;
            }
            
            if(cfg.RechargeId==-1 || (cfg.RechargeId == -2 && buyCnt == 0 ))
            {
                return ShopBuyState.Free;
            }
            
            if(cfg.RechargeId==-1 || (cfg.RechargeId == -2 && buyCnt is 1 or 2))
            {
                return ShopBuyState.Adv;
            }

            return ShopBuyState.Item;
        }

        /// <summary>
        /// 充值购买逻辑
        /// </summary>
        /// <param name="id">packageId</param>
        /// <param name="buySucceedAction">成功的回调</param>
        /// <param name="buyCnt">当前购买的次数 在需要随购买次数修改时才更改</param>
        /// <param name="rewardIndex">选择奖励的下表</param>
        /// <param name="needBuyNum">需要购买的数量</param>
        /// <param name="dealReward">独立处理奖励逻辑，方法内不处理</param>
        public void SendBuyPackageBuyRecharge(int id,Action<int> buySucceedAction=null,int buyCnt = 0,int rewardIndex=-1,int needBuyNum=1,Action<List<Resource>,List<Resource>> dealReward = null)
        {
            var cfg = ConfigCenter.PackageCfgColl.GetDataById(id);
            if(cfg==null) return;
            if (cfg.RechargeId > 0)
            {
                if (cfg.Cost is { Count: >= 1 } && UIHelper.CheckRewardIsEnough(cfg.Cost[0]))
                {
                    var model = new BuyConfirmViewModel(cfg.Id, () =>
                    {
                        SendBuyPackageBuyDispose(cfg.Id, (packId,result) =>
                        {
                            dealReward?.Invoke(result.Rewards.ToList(),result.Cost.ToList());
                            buySucceedAction?.Invoke(id);
                        },1,needBuyNum,rewardIndex,dealReward!=null);
                    }, () =>
                    {
                        PayController.Instance.Pay(cfg.Id,rewardIndex,(result) =>
                        {
                            if (result.Status == 0)
                            {
                                if (dealReward == null)
                                {
                                    if (result.Rewards.Count > 0)
                                    {
                                        Lodash.DealRewards(result.Rewards);
                                        UIHelper.OpenCommonRewardView(result.Rewards);
                                    }

                                }
                                else
                                {
                                    dealReward(result.Rewards,null);
                                }
                                buySucceedAction?.Invoke(id);
                            }
                        });
                    });
                    UIManager.Instance.OpenDialog<BuyConfirmView>(model).Forget();

                }else{
                    PayController.Instance.Pay(cfg.Id,rewardIndex,(result) =>
                    {
                        if (result.Status == 0)
                        {
                            if (dealReward == null)
                            {
                                if (result.Rewards.Count > 0)
                                {
                                    Lodash.DealRewards(result.Rewards);
                                    UIHelper.OpenCommonRewardView(result.Rewards);
                                }
                            }
                            else
                            {
                                dealReward(result.Rewards,null);
                            }
                            buySucceedAction?.Invoke(id);
                        }
                    });
                }


            }
            else if(cfg.RechargeId==0 || (cfg.RechargeId == -2 && buyCnt==0))
            {
                SendBuyPackageBuyDispose(cfg.Id, (packId,result) =>
                {
                    dealReward?.Invoke(result.Rewards.ToList(),result.Cost.ToList());
                    buySucceedAction?.Invoke(id);
                },2,needBuyNum,rewardIndex,dealReward!=null);
            }else if(cfg.RechargeId==-1 || (cfg.RechargeId == -2 && buyCnt is 1 or 2))
            {
                UIHelper.ShowRewardAds(() =>
                {
                    SendBuyPackageBuyDispose(cfg.Id, (packId,result) =>
                    {
                        dealReward?.Invoke(result.Rewards.ToList(),result.Cost.ToList());
                        buySucceedAction?.Invoke(id);
                    },2,needBuyNum,rewardIndex,dealReward!=null);
                });
            }
            else
            {
                SendBuyPackageBuyDispose(cfg.Id, (packId,result) =>
                {
                    dealReward?.Invoke(result.Rewards.ToList(),result.Cost.ToList());
                    buySucceedAction?.Invoke(id);
                },1,needBuyNum,rewardIndex,dealReward!=null);
            }
        }

        public void SendBuyPackageBuyDispose(int id,Action<int,RspShopPackageBuy> backSucceedAction=null,int op=1,int count = 1,int rewardId = 0,bool elseDeal=false)
        {
            SendShopPackageBuy(id, (packId,result) =>
            {
                if (!elseDeal)
                {
                    Lodash.DealRewards(result.Rewards.ToList(),result.Cost.ToList());
                    if (result.Rewards.Count > 0)
                    {
                        UIHelper.OpenCommonRewardView(result.Rewards.ToList());
                    }
                  
                }
                backSucceedAction?.Invoke(packId,result);
            },op,count,rewardId).Forget();
        }

        #endregion

        #region 红点逻辑

        public bool CheckShopRed()
        {
            return CheckDrawRed() || CheckDailyRed() || CheckGoldRed();
        }

        public bool CheckDrawRed()
        {
            return DataCenter.shopData.Recruit.CommFree > 0 || DataCenter.shopData.Recruit.CollectFree > 0;
        }
        
        public bool CheckDailyRed()
        {
            var dic = DataCenter.shopData.DailyShop;
            foreach (var item in dic)
            {
                var cfg = ConfigCenter.DailyShopCfgColl.GetDataById(item.Key);
                if (cfg != null)
                {
                    if ((cfg.TypeId is -1 or 0 && item.Value.Limit > 0) || (cfg.TypeId == -2 && item.Value.Limit > 0))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool CheckGoldRed()
        {
            return DataCenter.shopData.CoinFree>0;
        }
        #endregion
   
        #region 网络相关
        
        public void SendItemDraw(int id,Reward needItem,Action<List<Resource>> succeedAction = null)
        {
            if (UIHelper.CheckRewardIsEnough(needItem,true))
            {
                if (needItem.Id == (int)GameConst.ItemIdCode.Stone)
                {
                    SendDiamondDraw(id,succeedAction);

                }
                else
                {
                    SendItemDraw(id,succeedAction);
                }
            }
        }
        public void SendAdDraw(int id,Action<List<Resource>> succeedBack=null,bool isOpenDraw = true)
        {
            //广告抽奖
            UIHelper.ShowRewardAds(() =>
            {
                SendAdFreeDraw(id,succeedBack,isOpenDraw);
            });
        }
        public void SendAdFreeDraw(int id,Action<List<Resource>> succeedBack=null,bool isOpenDraw = true)
        {
            SendShopDraw(id,1,succeedBack,isOpenDraw).Forget();
        }
        public void SendDiamondDraw(int id,Action<List<Resource>> succeedBack=null,bool isOpenDraw = true)
        {
            SendShopDraw(id,2,succeedBack,isOpenDraw).Forget();
        }
        public void SendItemDraw(int id,Action<List<Resource>> succeedBack=null,bool isOpenDraw = true)
        {
            SendShopDraw(id,3,succeedBack,isOpenDraw).Forget();
        }

        //1-广告免费，2-砖石，3-道具
        public async UniTask SendShopDraw(int id,int op,Action<List<Resource>> succeedBack=null,bool isOpenDraw = true)
        {
            var req = new ReqShopRecruitBuy()
            {
                EquipChestId = id,
                Op = op,
            };
            var result =await GameNetworkManager.Instance.SendAsync<RspShopRecruitBuy>(req);

            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                Lodash.DealRewards(result.rsp.Rewards.ToList(),result.rsp.Cost.ToList());
                if (!ShopManager.Instance.IsExistUpState)
                {
                    ShopManager.Instance.IsExistUpState = result.rsp.Lv > DataCenter.shopData.GetBoxInfoLv();
                }

                if (op == 1)
                {
                    DataCenter.shopData.SetDrawFreeCnt(id);
                }

                DataCenter.shopData.SetBoxInfo(result.rsp.Lv,result.rsp.Exp);
                if (isOpenDraw)
                {
                    UIManager.Instance.OpenDialog<ShopDrawRewardView>(new ShopDrawRewardViewModel(result.rsp.Rewards.ToList(),op==1?0:id,
                        () =>
                        {
                            if (ShopManager.Instance.IsExistUpState)
                            {
                                ShopManager.Instance.IsExistUpState = false;
                                UIManager.Instance.OpenDialog<ShopBoxUpView>(new ShopBoxUpViewModel(DataCenter.shopData.GetBoxInfoLv())).Forget();
                            }
                        })).Forget();
                }
                succeedBack?.Invoke(result.rsp.Rewards.ToList());
            }
        }

        /// <summary>
        /// 礼包统一购买协议
        /// </summary>
        /// <param name="id">packageId</param>
        /// <param name="backSucceedAction">购买成功返回</param>
        /// <param name="op">1-货币购买 2-非货币购买【广告，免费】；</param>
        /// <param name="count">默认传1即可，目前金币购买需要指定数量，免费和广告情况传1；</param>
        /// <param name="rewardIndex">可选参数：特惠商店自选商品礼包，商品index</param>
        public async UniTask SendShopPackageBuy(int id,Action<int,RspShopPackageBuy> backSucceedAction=null,int op=1,int count = 1,int rewardIndex = 0)
        {
            var req = new ReqShopPackageBuy()
            {
                PackageId = id,
                Op = op,
                Count = count,
                Index = rewardIndex,
            };
            var result =await GameNetworkManager.Instance.SendAsync<RspShopPackageBuy>(req);
            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                backSucceedAction?.Invoke(id,result.rsp);
            }
        }
        
        public async UniTask SendShopDailyBuy(int id,Action<int,RspShopDailyBuy> backSucceedAction=null,int count = 1)
        {
            var req = new ReqShopDailyBuy()
            {
                DailyId = id,
                Count = count,
            };
            var result =await GameNetworkManager.Instance.SendAsync<RspShopDailyBuy>(req);
            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                Lodash.DealRewards(result.rsp.Rewards.ToList(),result.rsp.Cost.ToList());
                UIHelper.OpenCommonRewardView(result.rsp.Rewards.ToList());
                DataCenter.shopData.SetShopGoodsLimit(id);
                DataCenter.shopData.RefreshShopGoods();
                backSucceedAction?.Invoke(id,result.rsp);
                
            }
        }

        public void SendItemDrawClothes(int id, Reward needItem, int drawCnt, Action<List<Resource>> succeedAction = null, bool isOpen = true)
        {
            if (UIHelper.CheckRewardIsEnough(needItem,true))
            {
                if (needItem.Id == (int)GameConst.ItemIdCode.Stone)
                {
                    SendShopDrawRecruitEquip(id,drawCnt==10?4:2,succeedAction,isOpen).Forget();
                }
                else
                {
                    SendShopDrawRecruitEquip(id, drawCnt == 10 ?5:3,succeedAction,isOpen).Forget();
                }
            }
        }

        public async UniTask SendShopDrawRecruitEquip(int id,int op,Action<List<Resource>> backSucceedAction=null,bool isOpen = true)
        {
            var req = new ReqShopRecruitEquip()
            {
                EquipChestId = id,
                Op = op,
            };
            var result =await GameNetworkManager.Instance.SendAsync<RspShopRecruitEquip>(req);
            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                Lodash.DealRewards(result.rsp.Rewards.ToList(),result.rsp.Cost.ToList());
                if (isOpen && result.rsp.Rewards.Count>0)
                {
                    UIManager.Instance.OpenDialog<ShopDrawRewardView>(new ShopDrawRewardViewModel(result.rsp.Rewards.ToList(), id,null,TitleShowType.Base,op is 4 or 5)).Forget();
                }
                DataCenter.shopData.SetGuarantee(result.rsp.MinGuarantee,result.rsp.MaxGuarantee);
                backSucceedAction?.Invoke(result.rsp.Rewards.ToList());
            }
        }
        #endregion
    }
}
