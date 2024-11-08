using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Proto;
using DHFramework;
using Game.UI.MainUi;
using UnityEngine;

namespace DH.Game
{
    public class TriggerGiftManager : ObservableSingleton<TriggerGiftManager>
    {
        public string TriggerGiftFirstStr = "TriggerGiftFirstRecord";

        public List<int> MainAddPopList = new List<int>();

        public bool CheckMainAddPopList(int type)
        {
            return MainAddPopList.Contains(type);
        }
        
        public void AddMainAddPopList(int type)
        {
            MainAddPopList.Add(type);
        }

        public bool IsOpenTriggerGiftShop() 
        {
            return MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.TriggerGiftShop);// && GetSatisfyConditionsTypeList().Count > 0;
        }

        public List<int> GetSatisfyConditionsTypeList()
        {
            List<int> showType = new List<int>();
            var typeList = ConfigCenter.TriggerGiftTypeCfgColl.DataItems.ToList();
            foreach (var item in typeList)
            {
                var showList = GetSatisfyConditionsList(item.Id);
                if (showList.Count > 0) showType.Add(item.Id);
            }
            return showType;
        }

        public List<TriggerGiftCfg> GetSatisfyConditionsList(int showType = 0)
        {
            List<TriggerGiftCfg> giftList = new List<TriggerGiftCfg>();
            var giftCfgList = ConfigCenter.TriggerGiftCfgColl.DataItems.ToList();
            foreach (var item in giftCfgList)
            {
                var type = ConfigCenter.TriggerGiftTypeCfgColl.GetDataById(item.Type);
                if (type == null)
                {
                    DHLog.Error($"没有触发礼包类型 检查配置表 TriggerGiftCfg 的 Type {item.Type}  和 配置表 TriggerGiftTypeCfg ");
                    continue;
                }

                if (!(type.Trigger is 1 or 2 or 5)) continue;
                if (showType != 0 && item.Type != showType) continue;
                if ( DataCenter.triggerGiftData.IsBuyTriggerGift(item.Type, item.Id)) continue;
                if (item.FrontId != 0)
                {
                    var frontCfg = ConfigCenter.TriggerGiftCfgColl.GetDataById(item.FrontId);
                    if (frontCfg != null && !DataCenter.triggerGiftData.IsBuyTriggerGift(frontCfg.Type, frontCfg.Id)) continue;
                }

                if (DataCenter.triggerGiftData.IsOpenTriggerGift(item.Type))
                {
                    giftList.Add(item);
                }
            }

            return giftList;
        }

        public List<TriggerGiftCfg> GetWeekSatisfyConditionsList()
        {
            List<TriggerGiftCfg> giftList = new List<TriggerGiftCfg>();
            var giftCfgList = ConfigCenter.TriggerGiftCfgColl.DataItems.ToList();
            foreach (var item in giftCfgList)
            {
                var type = ConfigCenter.TriggerGiftTypeCfgColl.GetDataById(item.Type);
                if (type == null)
                {
                    DHLog.Error($"没有触发礼包类型 检查配置表 TriggerGiftCfg 的 Type {item.Type}  和 配置表 TriggerGiftTypeCfg ");
                    continue;
                }
                if (type.Trigger != 3) continue;
                if ( DataCenter.triggerGiftData.IsBuyTriggerGift(item.Type, item.Id)) continue;
                if (item.FrontId != 0)
                {
                    var frontCfg = ConfigCenter.TriggerGiftCfgColl.GetDataById(item.FrontId);
                    if (frontCfg != null && !DataCenter.triggerGiftData.IsBuyTriggerGift(frontCfg.Type, frontCfg.Id)) continue;
                }

                if (DataCenter.triggerGiftData.IsOpenTriggerGift(item.Type))
                {
                    giftList.Add(item);
                }
            }

            return giftList;
        }  
        public List<TriggerGiftCfg> GetDaySatisfyConditionsList()
        {
            List<TriggerGiftCfg> giftList = new List<TriggerGiftCfg>();
            var giftCfgList = ConfigCenter.TriggerGiftCfgColl.DataItems.ToList();
            foreach (var item in giftCfgList)
            {
                var type = ConfigCenter.TriggerGiftTypeCfgColl.GetDataById(item.Type);
                if (type == null)
                {
                    DHLog.Error($"没有触发礼包类型 检查配置表 TriggerGiftCfg 的 Type {item.Type}  和 配置表 TriggerGiftTypeCfg ");
                    continue;
                }
                if (type.Trigger != 6) continue;
                if ( DataCenter.triggerGiftData.IsBuyTriggerGift(item.Type, item.Id)) continue;
                if (item.FrontId != 0)
                {
                    var frontCfg = ConfigCenter.TriggerGiftCfgColl.GetDataById(item.FrontId);
                    if (frontCfg != null && !DataCenter.triggerGiftData.IsBuyTriggerGift(frontCfg.Type, frontCfg.Id)) continue;
                }

                if (DataCenter.triggerGiftData.IsOpenTriggerGift(item.Type))
                {
                    giftList.Add(item);
                }
            }

            return giftList;
        }
        
        public List<TriggerGiftCfg> GetMonthSatisfyConditionsList()
        {
            List<TriggerGiftCfg> giftList = new List<TriggerGiftCfg>();
            var giftCfgList = ConfigCenter.TriggerGiftCfgColl.DataItems.ToList();
            foreach (var item in giftCfgList)
            {
                var type = ConfigCenter.TriggerGiftTypeCfgColl.GetDataById(item.Type);
                if (type == null)
                {
                    DHLog.Error($"没有触发礼包类型 检查配置表 TriggerGiftCfg 的 Type {item.Type}  和 配置表 TriggerGiftTypeCfg ");
                    continue;
                }
                if (type.Trigger != 4) continue;
                if ( DataCenter.triggerGiftData.IsBuyTriggerGift(item.Type, item.Id)) continue;
                if (item.FrontId != 0)
                {
                    var frontCfg = ConfigCenter.TriggerGiftCfgColl.GetDataById(item.FrontId);
                    if (frontCfg != null && !DataCenter.triggerGiftData.IsBuyTriggerGift(frontCfg.Type, frontCfg.Id)) continue;
                }

                if (DataCenter.triggerGiftData.IsOpenTriggerGift(item.Type))
                {
                    giftList.Add(item);
                }
            }

            return giftList;
        }
        public int GetShowTypeFirstTriggerGift(int showType)
        {
            var giftCfgList = ConfigCenter.TriggerGiftCfgColl.DataItems.ToList();
            foreach (var item in giftCfgList)
            {
                if (showType != 0 && item.Type != showType) continue;
                var buy = DataCenter.triggerGiftData.IsBuyTriggerGift(item.Type, item.Id);
                if (buy) continue;
                if (item.FrontId != 0)
                {
                    var frontCfg = ConfigCenter.TriggerGiftCfgColl.GetDataById(item.FrontId);
                    if (frontCfg != null &&
                        !DataCenter.triggerGiftData.IsBuyTriggerGift(frontCfg.Type, frontCfg.Id)) continue;
                }

                if (DataCenter.triggerGiftData.IsOpenTriggerGift(item.Type))
                {
                    return item.Id;
                }
            }

            return 0;
        }

        public void SaveTriggerTypeFirst(int Type)
        {
            var str = PlayerPrefs.GetString(TriggerGiftFirstStr+"/"+DataCenter.charcaterData.Digest.RoleId, string.Empty);
            PlayerPrefs.SetString(TriggerGiftFirstStr+"/"+DataCenter.charcaterData.Digest.RoleId, str + "/" + Type);
        }

        public bool CheckTriggerTypeFirst(int showType)
        {
            if (!MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.TriggerGiftShop)) return false;
            if (CheckMainAddPopList(showType)) return false;
            if (DataCenter.triggerGiftData.CheckPreData(showType)) return false;
            var str = PlayerPrefs.GetString(TriggerGiftFirstStr+"/"+DataCenter.charcaterData.Digest.RoleId, string.Empty);
            var typeList = str.Split("/");
            if (typeList is { Length: > 0 })
            {
                if (!typeList.Contains(showType.ToString()))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }

            return true;
        }

        public bool GiftIsCanBuy(int id)
        {
            var cfg = ConfigCenter.TriggerGiftCfgColl.GetDataById(id);
            if (!DataCenter.triggerGiftData.Data.ContainsKey(cfg.Type)) return false;
            if (!ServerTime.Instance.IsOpenTime(DataCenter.triggerGiftData.Data[cfg.Type].EndStamp))return false;
            if (!DataCenter.triggerGiftData.Data[cfg.Type].Record.ContainsKey(cfg.Id)) return true;
            if (DataCenter.triggerGiftData.Data[cfg.Type].Record[cfg.Id] >= cfg.BuyLimit) return false;
            return true;
        }
        public bool GiftIsBuy(int id)
        {
            var cfg = ConfigCenter.TriggerGiftCfgColl.GetDataById(id);
            if (!DataCenter.triggerGiftData.Data.ContainsKey(cfg.Type)) return false;
            if (DataCenter.triggerGiftData.Data[cfg.Type].Record.ContainsKey(cfg.Id)) return true;
            return false;
        }
        public string GetTriggerGiftName(int id)
        {
            var cfgL = ConfigCenter.TriggerGiftLanguageCfgColl.GetDataById(id);
            return cfgL?.Name ?? string.Empty;
        }

        public string GetWeekTriggerGiftTime()
        {
            var time = ServerTime.Instance.RemainTime(DataCenter.triggerGiftData.WeekRefreshStamp);
            if (time < 0) return string.Empty;
            
            if (time >= 86400)
            {
                return  UIHelper.ConvertTimeSecondToString(time, ETimeFormatType.TimeFormatTypeHourMinuteWithUnit);
            }
            return  ServerTime.Instance.Seconds2Hhmmss(time); 
        }
        public string GetMonthTriggerGiftTime()
        {
            var time = ServerTime.Instance.RemainTime(DataCenter.triggerGiftData.MonthRefreshStamp);
            if (time < 0) return string.Empty;
            if (time >= 86400)
            {
                return  UIHelper.ConvertTimeSecondToString(time, ETimeFormatType.TimeFormatTypeHourMinuteWithUnit);
            }
            return  ServerTime.Instance.Seconds2Hhmmss(time); 
        }
        public string GetDayTriggerGiftTime()
        {
            var time = ServerTime.Instance.RemainTime(DataCenter.triggerGiftData.DayRefreshStamp);
            if (time < 0) return string.Empty;
            if (time >= 86400)
            {
                return  UIHelper.ConvertTimeSecondToString(time, ETimeFormatType.TimeFormatTypeHourMinuteWithUnit);
            }
            return  ServerTime.Instance.Seconds2Hhmmss(time); 
        }
        
        public string GetProgressTime()
        {
            var time = ServerTime.Instance.RemainTime(DataCenter.triggerGiftData.BuyDayRefreshStamp);
            if (time < 0) return string.Empty;
            if (time >= 86400)
            {
                return  UIHelper.ConvertTimeSecondToString(time, ETimeFormatType.TimeFormatTypeHourMinuteWithUnit);
            }
            return  ServerTime.Instance.Seconds2Hhmmss(time); 
        }
        public string GetTriggerTitleIcon(int id)
        {
            var cfg = ConfigCenter.TriggerGiftCfgColl.GetDataById(id);
            if (cfg != null)
            {
                var cfgType = ConfigCenter.TriggerGiftTypeCfgColl.GetDataById(cfg.Type);
                if (cfgType.Trigger == 1)
                {
                    return EquipManager.Instance.GetIconPath(cfgType.TriggerNum[0]);
                }else if (cfgType.Trigger == 2)
                {
                    return DataCenter.roleData.HeroCardIcon(cfgType.TriggerNum[0]);
                }
            }
            return UIHelper.NoneImagePath();
        }
        
        public string GetTriggerTitleBgPath(int id)
        {
            var cfg = ConfigCenter.TriggerGiftCfgColl.GetDataById(id);
            if (cfg != null)
            {
                var cfgType = ConfigCenter.TriggerGiftTypeCfgColl.GetDataById(cfg.Type);
                if (cfgType.Trigger is 1 or 5)
                {
                    return "specialshop[specialshop_panel_1]";
                }
                if (cfgType.Trigger == 2)
                {
                    return "specialshop[specialshop_panel_2]";
                }
                if (cfgType.Trigger == 3)//周礼包
                {
                    return "specialshop[specialshop_panel_6]";
                }
                if (cfgType.Trigger == 4)//月礼包
                {
                    return "specialshop[specialshop_panel_8]";
                }
                if (cfgType.Trigger == 6)//日礼包
                {
                    return "specialshop[specialshop_panel_10]";
                }
            }
            return UIHelper.NoneImagePath();
        }

        public async UniTask SendBuyTriggerGift(int id,Action<int> backAction)
        {
            var req = new ReqTriggerGiftConditionBuy()
            {
                Id = id,
            };
            var result =await GameNetworkManager.Instance.SendAsync<RspTriggerGiftConditionBuy>(req);

            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                Lodash.DealRewards(result.rsp.Rewards.ToList());
                UIHelper.OpenCommonRewardView(result.rsp.Rewards.ToList());
                backAction?.Invoke(id);
            }
        }

        public readonly string TriggerGiftRed = DataCenter.charcaterData.Digest.RoleId + "TriggerGiftRed";//触发礼包武器红点
        public bool IsRed()
        {
            return  DHUnityUtil.PlayerPrefs.GetInt(TriggerGiftRed) == 1;
        }
        public void SetTriggerGiftRed()
        {
            DHUnityUtil.PlayerPrefs.SetInt(TriggerGiftRed,1);
        }
        public void ClearTriggerGiftRed()
        {
            DHUnityUtil.PlayerPrefs.SetInt(TriggerGiftRed,0);
        }

        /// <summary>
        /// 神秘商店累充
        /// </summary>
        public void PayProgress()
        {
            if (!DataCenter.triggerGiftData.TodayBuy)
            {
                DataCenter.triggerGiftData.TodayBuy = true;
                DataCenter.triggerGiftData.BuyDay++;
            }
        }
    }
}
