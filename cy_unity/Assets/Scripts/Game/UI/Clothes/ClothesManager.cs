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
using DHFramework;

namespace DH.Game
{
    public partial class ClothesManager : ObservableSingleton<ClothesManager>
    {
         [AutoNotify] private bool clothesRed;
         [AutoNotify] private int useClothesPart; 
         public int MergeMaxQuaId => UIHelper.GetDefinesInt(DefineCfgId.heroEquipQua_Max);

         
         [AutoNotify] public bool isMergeSelect;
         
         private ClothesUI curTabType = ClothesUI.HeroEquip;
         public ClothesUI CurTabType
         {
             get => curTabType;
             set
             {
                 if (curTabType == value) return;
                 Set(ref curTabType, value);
             }
         }

         public void StartUseClothesAnimation(long uid)
         {
             var data = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
             if(data==null) return;
             var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(data.Id);
             if(cfg==null) return;
             useClothesPart = cfg.PartId;
             RaisePropertyChanged(nameof(UseClothesPart));
         }

         public int GetRewardUid(int id, int pos)
        {
            return id * 1000 + pos;
        }

        public long ResolveRewardUid(long id)
        {
            return id / 1000;
        }

        public bool IsMergeNeedIdEqual(int quaId)
        {
            var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(quaId);
            return quaCfg?.Type == 2;
        }

        public int GetMaxLevel(int qua)
        {
            var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(qua);
            return quaCfg?.MaxLv ?? 999;
        }
        
        public bool IsMaxLevel(HeroEquipData data)
        {
            if (data == null) return false;
            var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(data.QuaId);
            if (quaCfg == null) return false;
            return quaCfg.MaxLv <=data.Lv;
        }

        public List<Reward> GetMergeAllItem()
        {
            return ItemManager.Instance.GetRewardsByType((int)GameConst.ItemType.HeroEquipMerge, true);
        }
        
        public bool IsMergeMaxQua(int mergeQua)
        {
            var list = ConfigCenter.HeroEquipQuaUpCfgColl.DataItems.ToList();
            return mergeQua>=list.Last().Id;
        }

        public int GetHeroEquipPart(int id)
        {
            var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(id);
            return cfg?.PartId ?? 0;
        }


        public int GetMergeNeedNum(HeroEquipData data)
        {
            if (data == null) return 0;
            var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(data.QuaId);
            if (quaCfg != null)
            {
                return quaCfg.Num;
            }

            return 0;
        }

        public string GetPartEquipMinIcon(int id,int qua=0)
        {
            var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(id);
            if (cfg != null)
            {
                var cfgPart = ConfigCenter.HeroEquipPartCfgColl.GetDataById(cfg.PartId);
                if (qua == 0)
                {
                    var rareCfg = ConfigCenter.HeroEquipRareCfgColl.GetDataById(cfg.RareType);
                    qua = rareCfg.QuaId;
                }

                return cfgPart!=null?$"{ cfgPart.HeroEquipIconMin.Replace("]","")}_{GetQuaSmallByQuaId(qua)}]" : UIHelper.NoneImagePath();
            }

            return UIHelper.NoneImagePath();
        }

        public string GetPartEquipBgIcon(int id)
        {
            var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(id);
            if (cfg != null)
            {
                var cfgPart = ConfigCenter.HeroEquipPartCfgColl.GetDataById(cfg.PartId);
                return cfgPart?.HeroEquipIcon ?? UIHelper.NoneImagePath();
            }

            return UIHelper.NoneImagePath();
        }

        public string GetPartEquipBgIconByPartId(int partId)
        {
            var cfgPart = ConfigCenter.HeroEquipPartCfgColl.GetDataById(partId);
            return cfgPart?.HeroEquipIcon ?? UIHelper.NoneImagePath();
        }

        public string GetRareIcon(int id)
        {
            var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(id);
            if (cfg != null)
            {
                if (cfg.RareType == 1) return UIHelper.NoneImagePath();
                var rareCfg = ConfigCenter.HeroEquipRareCfgColl.GetDataById(cfg.RareType);
                return rareCfg?.Icon ?? UIHelper.NoneImagePath();
            }

            return UIHelper.NoneImagePath();
        }
        
        public string GetRareIconByRareId(int id)
        {
            var rareCfg = ConfigCenter.HeroEquipRareCfgColl.GetDataById(id);
            return rareCfg?.Icon ?? UIHelper.NoneImagePath();
        }
        
        public bool IsRareShowById(int id)
        {
            var rareCfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(id);
            return rareCfg != null && rareCfg.RareType != 1;
        }

        public string GetClothesQuaBgSmallPath(int qua)
        {
            switch (qua)
            {
                case 4:
                {
                    return "item[item_icon_purple3]";
                }
                case 5:
                {
                    return "item[item_icon_yellow3]";
                }
                case 6:
                {
                    return "item[item_icon_red3]";
                }
                default:
                {
                    return "item[item_icon_purple3]";
                }
            }
        }

        public string GetClothesQuaBgPath(int id, HeroEquipData data = null)
        {
            if (data != null)
            {
                var quaCfg = ConfigCenter.QuaCfgColl.GetDataById(GetQuaSmallByQuaId(data.QuaId));
                return quaCfg?.ItemBg ?? "common[commom_equipbg_1]";
            }

            var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(id);
            if (cfg != null)
            {
                var cfgRare = ConfigCenter.HeroEquipRareCfgColl.GetDataById(cfg.RareType);
                if (cfgRare != null)
                {
                    var quaCfg = ConfigCenter.QuaCfgColl.GetDataById(cfg.QuaType);
                    return quaCfg?.ItemBg ?? "common[commom_equipbg_1]";
                }
            }
            return $"common[commom_equipbg_1]"; //;
        }

        public string GetClothesIconPath(int id)
        {
            var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(id);
            if (cfg != null)
            {
                return cfg.Icon;
            }

            return UIHelper.NoneImagePath();
        }

        public int GetBaseQua(int id)
        {
            var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(id);
            if (cfg == null) return 0;
            var cfgRare = ConfigCenter.HeroEquipRareCfgColl.GetDataById(cfg.RareType);
            if (cfgRare == null) return 0;
            return cfgRare.QuaId;
        }

        public int GetQuaSmallById(int id, HeroEquipData heroEquipData = null)
        {
            if (heroEquipData != null && heroEquipData.QuaId != 0)
            {
                return heroEquipData.QuaId;
            }

            var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(id);
            if (cfg == null) return 0;
            var cfgRare = ConfigCenter.HeroEquipRareCfgColl.GetDataById(cfg.RareType);
            if (cfgRare == null) return 0;
            return cfgRare.QuaId; //GetQuaSmall(quaId);
        }

        public int GetQuaSmallByQuaId(int quaId)
        {
            var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(quaId);
            if (quaCfg != null)
            {
                return quaCfg.QuaId;
            }

            return 0;
        }
        
        public int GetQuaUpStateStart(int quaId)
        {
            var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(quaId);
            if (quaCfg != null)
            {
                var quaList = ConfigCenter.HeroEquipQuaUpCfgColl.DataItems.ToList();
                foreach (var item in quaList)
                {
                    if (item.QuaId == quaCfg.QuaId && item.QuaStage == 0)
                    {
                        return item.Id;
                    }
                }
            }

            return quaId;
        }


        public bool CheckStateStart(int quaId)
        {
            var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(quaId);
            if (quaCfg != null)
            {
                return quaCfg.QuaStage==0;
            }

            return false;
        }

        public int GetQuaSmallPos(int quaId)
        {
            var cfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(quaId);
            return cfg?.QuaStage ?? 0;
            // var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataByQuaId(quaId);
            // List<int> recordList = new();
            // if (quaCfg != null)
            // {
            //     var quaCfgs = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataByQuaId(quaId);
            //     if (quaCfgs != null && quaCfgs.Count > 0)
            //     {
            //         for (int i = 0; i < quaCfgs.Count; i++)
            //         {
            //             recordList.Add(quaCfgs[i].NextId);
            //         }
            //
            //         var startId = 0;
            //         for (int i = 0; i < quaCfgs.Count; i++)
            //         {
            //             if (!recordList.Contains(quaCfgs[i].Id))
            //             {
            //                 startId = quaCfgs[i].Id;
            //             }
            //         }
            //
            //         return GetSmallPos(startId,quaId);
            //     }
            // }
            // return 0;
        }

        public int GetSmallPos(int quaId,int endQuaId,int endPos = 0)
        {
            if (quaId == endQuaId) return endPos;
            var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(quaId);
            if (quaCfg != null)
            {
                endPos += 1;
                return GetSmallPos(quaCfg.NextId,endQuaId,endPos);
            }
            return 0;
        }

        public string GetClothesItemName(int id)
        {
            var cfg = ConfigCenter.HeroEquipResourceLanguageCfgColl.GetDataById(id);
            return cfg?.Name ?? string.Empty;
        }

        //暂无描述
        public string GetClothesItemDesc(int id)
        {
            var cfg = ConfigCenter.HeroEquipResourceLanguageCfgColl.GetDataById(id);
            return cfg.Name ?? string.Empty;
        }

        public int GetClothesMaxLevel(int quaId)
        {
            var cfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(quaId);
            return cfg?.MaxLv ?? 999;
        }
        
        public int GetClothesMaxLevel()
        {
            var cfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(MergeMaxQuaId);
            return cfg?.MaxLv ?? 999;
        }

        //获得合成系数
        public float GetQuaValue(int quaId)
        {
            var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(quaId);
            if (quaCfg != null)
            {
                return quaCfg.Value / 10000f;
            }

            return 0;
        }

        public string GetClothesSkillDesc(int skillId)
        {
            try
            {
                var cfg = ConfigCenter.HeroEquipSkillCfgColl.GetDataById(skillId);
                var cfgL = ConfigCenter.HeroEquipSkillLanguageCfgColl.GetDataById(skillId);
                if (cfg != null && cfgL != null)
                {
                    string desc = cfgL.Dec;
                    UIHelper.GetDescLinkInfo(ref desc,SkillPreviewSkillType.HeroEquip,cfg.BuffId);
                    switch (cfg.Value?.Count)
                    {
                        case 1: return DHUtility.Format(desc, cfg.Value[0]);
                        case 2: return DHUtility.Format(desc, cfg.Value[0], cfg.Value[1]);
                        case 3: return DHUtility.Format(desc, cfg.Value[0], cfg.Value[1], cfg.Value[2]);
                        case 4: return DHUtility.Format(desc, cfg.Value[0], cfg.Value[1], cfg.Value[2], cfg.Value[3]);
                        case 5:
                            return DHUtility.Format(desc, cfg.Value[0], cfg.Value[1], cfg.Value[2], cfg.Value[3],
                                cfg.Value[4]);
                        default: return desc;

                    }
                }
            }
            catch (Exception e)
            {
                DHLog.Error($"技能Id： {skillId}   描述内需要的参数 与实际参数不一致");
            }

            return string.Empty + $"Skill Id {skillId}";
        }

        public string GetQuaBgSmall(int id, HeroEquipData data = null)
        {
            var qua = GetQuaSmallById(id, data);
            var cfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(qua);
            if (cfg != null)
            {
                switch (cfg.QuaId)
                {
                    case 1: return "item[item_panel_white]";
                    case 2: return "item[item_panel_green]";
                    case 3: return "item[item_panel_blue]";
                    case 4: return "item[item_panel_purple]";
                    case 5: return "item[item_panel_yellow]";
                    case 6: return "item[item_panel_red]";
                    default: return "item[item_panel_white]";
                }
            }

            return UIHelper.NoneImagePath();
        }

        public string GetQuaName(int id, HeroEquipData data = null,bool isNeedColor = true,bool isBase = true)
        {
            var qua = GetQuaSmallById(id, data);
            var cfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(qua);
            if (cfg != null)
            {
                var nameStr = UIHelper.GetQuaName(cfg.QuaId);
                if (cfg.QuaStage != 0)
                {
                    nameStr += $"+{cfg.QuaStage}";
                }

                if (isNeedColor)
                {
                    return UIHelper.GetQuaColor(nameStr, cfg.QuaId,isBase);
                }
                else
                {
                    return nameStr;
                }

            }

            return string.Empty;
        }
        
        public string GetPartNameById(int id)
        {
            var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(id);
            if (cfg != null)
            {
                var partCfgL = ConfigCenter.HeroEquipPartLanguageCfgColl.GetDataById(cfg.PartId);
                return partCfgL?.Name ?? string.Empty;
            }

            return string.Empty;
        }
        
        public string GetHeroEquipNameById(int id)
        {
            var cfgL = ConfigCenter.HeroEquipResourceLanguageCfgColl.GetDataById(id);
            return cfgL?.Name ?? string.Empty;
        }

        public Dictionary<string, float> GetClothesAllAttr()
        {
            var wearList = DataCenter.clothesData.Wear;
            Dictionary<string,float> dic = new();
            dic.TryAdd(AttributeName.Atk, 0);
            dic.TryAdd(AttributeName.Hp, 0);
            foreach (var item in wearList)
            {
                var itemDic = GetClothesAttrList(item.Value);
                foreach (var attr in itemDic)
                {
                    if (!dic.ContainsKey(attr.Key))
                    {
                        dic.Add(attr.Key,UIHelper.GetRoundResult(attr.Key,attr.Value,false));
                    }
                    else
                    {
                        dic[attr.Key] += UIHelper.GetRoundResult(attr.Key,attr.Value,false);
                    }
                }
            }

            var changedAttr = GetEquipAllAttrList();

            if (dic.ContainsKey(AttributeName.Atk))
            {
                var value = changedAttr.Calc(AttributeType.HeroEquipAttackBonus);
                dic[AttributeName.Atk] = UIHelper.GetRoundResult(AttributeName.Atk,(1 + value)*dic[AttributeName.Atk]);
            }
            
            if (dic.ContainsKey(AttributeName.Hp))
            {
                var value = changedAttr.Calc(AttributeType.HeroEquipHpBonus);
                dic[AttributeName.Hp] = UIHelper.GetRoundResult(AttributeName.Hp,(1 + value)*dic[AttributeName.Hp]);
            }

            //英雄基础等级技能属性

            List<string> attrNames = new();
            foreach (var item in dic)
            {
                attrNames.Add(item.Key);
            }
            for (int i = 0; i < attrNames.Count; i++)
            {
                var data = DataCenter.roleData;
                var nameCfg = ConfigCenter.AttributesCfgColl.GetDataByName(attrNames[i]);
                var attr = data.GetAttribute(data.FmtHero,nameCfg.Id);
                dic[attrNames[i]] += (float)attr;
            }
            
            return dic;
        }

        public Dictionary<string,float> GetClothesAttrList(long uid,bool isNeedValue=true)
        {
            return GetClothesAttrList(DataCenter.clothesData.GetHeroEquipDataByUid(uid),isNeedValue);
        }

        public Dictionary<string,float> GetClothesAttrList(HeroEquipData data,bool isNeedValue=true)
        {
            Dictionary<string,float> dic = new();
            if (data == null) return dic;
            var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(data.Id);
            if (cfg != null)
            {
                var rareCfg = ConfigCenter.HeroEquipRareCfgColl.GetDataById(cfg.RareType);
                for (int i = 0; i <= data.Lv; i++)
                {
                    var upCfg = ConfigCenter.HeroEquipLevelUpCfgColl.GetDataById(i);
                    if (upCfg!=null)
                    {
                        var attrList = upCfg.EquipAttrAdd1;
                        if (attrList != null)
                        {
                            foreach (var item in attrList)
                            {
                                if (item.PartId == cfg.PartId)
                                {
                                    if (!dic.ContainsKey(item.AttrType))
                                    {
                                        dic.Add(item.AttrType,upCfg.Id==1? item.Value * rareCfg.Value/10000f:item.Value);
                                    }
                                    else
                                    {
                                        dic[item.AttrType] += upCfg.Id==1?item.Value * rareCfg.Value/10000f:item.Value;
                                    }
                                }
                            }
                        } 
                    }
                } 
            }

            if (isNeedValue)
            {
                var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(data.QuaId);
                if (quaCfg != null)
                {
                    var keyList = dic.Keys.ToList();
                    for (int i = 0; i < dic.Keys.Count; i++)
                    {
                        dic[keyList[i]] *= quaCfg.Value / 10000f;
                    }
                }
            }
            return dic;
        }

        public Dictionary<string, float> GetClothesNextAddAttrList(long uid,bool isNeedValue = true)
        {
            Dictionary<string,float> dic = new();
            var data = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
            if (data == null) return dic;
            var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(data.Id);
            if (cfg != null)
            {
                var upCfg = ConfigCenter.HeroEquipLevelUpCfgColl.GetDataById(data.Lv+1);
                if (upCfg!=null)
                {
                    var attrList =upCfg.EquipAttrAdd1;
                    if (attrList != null)
                    {
                        foreach (var item in attrList)
                        {
                            if (item.PartId == cfg.PartId)
                            {
                                if (!dic.ContainsKey(item.AttrType))
                                {
                                    dic.Add(item.AttrType,item.Value);
                                }
                                else
                                {
                                    dic[item.AttrType] += item.Value;
                                }
                            }
                        }
                    } 
                }
            }
            
            if (isNeedValue)
            {
                var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(data.QuaId);
                if (quaCfg != null)
                {
                    var keyList = dic.Keys.ToList();
                    for (int i = 0; i < dic.Keys.Count; i++)
                    {
                        dic[keyList[i]] *= quaCfg.Value / 10000f;
                    }
                }
            }
            return dic;
        }

        public AttributeMgr  GetEquipAllAttrList()
        {
            var attrMgr = new AttributeMgr(null);
            var partList = ConfigCenter.HeroEquipPartCfgColl.DataItems.ToList();
            foreach (var item in partList)
            {
                var equipData = DataCenter.clothesData.GetHeroEquipDataByPart(item.Id);
                if (equipData != null)
                {
                    var allList = GetClothesSkillList(equipData.Id);
                    if (allList == null) continue;
                    foreach (var skillCfg in allList)
                    {
                        if (skillCfg.QuaId <= GetQuaSmallByQuaId(equipData.QuaId))
                        {
                            var skillInfo = ConfigCenter.SkillCfgColl.GetDataById(skillCfg.SkillId);
                            if(skillInfo!=null && skillInfo.Result!=null)
                                attrMgr.Modify(skillInfo.Result);
                        }
                    }
                }
            }

            return attrMgr;
        }


        public List<HeroEquipSkillCfg> GetClothesSkillList(int id)
        {
            return ConfigCenter.HeroEquipSkillCfgColl.GetDataByHeroEquipId(id);
        }

        public int GetSkillIdByQuaId(int id,int quaId)
        {
            var lastId = GetLastQuaId(quaId);
            var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(quaId);

            if (lastId != 0)
            {
                var lastQuaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(lastId);
                if (quaCfg == null || lastQuaCfg == null || lastQuaCfg.QuaId == quaCfg.QuaId)
                {
                    return 0;
                }
            }
            
            var skillList = ConfigCenter.HeroEquipSkillCfgColl.GetDataByHeroEquipId(id);
            foreach (var item in skillList)
            {
                if (item.QuaId == quaCfg.QuaId)
                {
                    return item.Id;
                }
            }

            return 0;
        }

        public int GetNeedItemId(int id,bool isMerge = false,int quaId = 0)
        {
            var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(id);
            if (cfg != null)
            {
                var partCfg = ConfigCenter.HeroEquipPartCfgColl.GetDataById(cfg.PartId);
                if (partCfg != null)
                {
                    if (isMerge)
                    {
                        var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(quaId);
                        if (quaCfg != null)
                        {
                            int pos = quaCfg.QuaId - 4;
                            if (cfg.RareType == 2)
                            {
                                if (pos>=0 && pos < partCfg.QuaItemId_S.Count)
                                {
                                    return partCfg.QuaItemId_S[pos];
                                }
                            }
                            else
                            {
                                if (pos>=0 && pos < partCfg.QuaItemId.Count)
                                {
                                    return partCfg.QuaItemId[pos];
                                }
                            }


                        }
                    }
                    else
                    {
                        return partCfg.LvItemId;
                    }
                }
            }
            return 0;
        }

        public List<Reward> GetUpLevelNeedRewardList(long uid)
        {
            List<Reward> rewards = new();
            var data = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
            if (data == null) return rewards;
            var upCfg = ConfigCenter.HeroEquipLevelUpCfgColl.GetDataById(data.Lv);
            if (upCfg!=null)
            {
               rewards.AddRange(upCfg.CoinNum);
               rewards.Add(new Reward(RewardType.Item,GetNeedItemId(data.Id),upCfg.ItemNum));
            }
            
            return rewards;
        }

        public List<Reward> GetResetBackRewards(long uid,bool isQua = false)
        {
            List<Reward> rewards = new();
            var data = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
            if (data == null) return rewards;
            var needItemId = GetNeedItemId(data.Id);
            for (int i = 0; i < data.Lv; i++)
            {
                var upCfg = ConfigCenter.HeroEquipLevelUpCfgColl.GetDataById(i);
                if (upCfg != null)
                {
                    rewards.Add(new Reward(RewardType.Item,needItemId,upCfg.ItemNum));
                    rewards.AddRange(upCfg.CoinNum);
                }

            }

            if (isQua)
            {
                var quaUpCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(data.QuaId);
                var needItemId1 = GetNeedItemId(data.Id,true,data.QuaId);
                if (quaUpCfg != null)
                {
                    var quaList = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataByQuaId(quaUpCfg.QuaId);
                    for (int i = 0; i < quaList.Count; i++)
                    {
                        if (quaList[i].Id == data.QuaId)
                        {
                            break;
                        }
                        else
                        {
                            if (quaList[i].Type == 2)
                            {
                                rewards.Add(new Reward(RewardType.Item,needItemId1,quaList[i].Num));
                            }
                        }
                    }
                }
            }

            return UIHelper.OverlayReward(rewards);
        }

        public int GetQuaBaseId(long uid)
        {
            var data = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
            if (data != null)
            {
                var quaUpCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(data.QuaId);
                if (quaUpCfg != null)
                {
                    var quaUpList = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataByQuaId(quaUpCfg.QuaId);
                    if (quaUpList != null && quaUpList.Count > 0)
                    {
                        return quaUpList[0].Id;
                    }
                }
            }

            return 0;
        }

        public bool IsCanResetLv(long uid)
        {
            var data = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
            return data != null && data.Lv > 1;
        }
        
        public bool IsCanResetQua(long uid)
        {
            var data = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
            if (data != null)
            {
                var quaUpCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(data.QuaId);
                if (quaUpCfg != null)
                {
                    var quaUpList = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataByQuaId(quaUpCfg.QuaId);
                    if (quaUpList != null && quaUpList.Count > 0)
                    {
                        if (quaUpList.Count > 1 && quaUpList[0].Id != data.QuaId) return true;
                    }
                }
            }

            return false;
        }

        public int GetNextQuaId(int quaId)
        {
            if (IsMergeMaxQua(quaId)) return 0;
            var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(quaId);
            if (quaCfg != null)
            {
                return quaCfg.NextId;
            }

            return quaId;
        }
        
        public int GetLastQuaId(int quaId)
        {
            var quaList = ConfigCenter.HeroEquipQuaUpCfgColl.DataItems.ToList();
            foreach (var item in quaList)
            {
                if (item.NextId == quaId)
                {
                    return item.Id;
                }
            }
            return 0;
        }
        
        public bool IsCanMerge()
        {
            var clothesList = DataCenter.clothesData.Items;
            foreach (var item in clothesList)
            {
                if (CheckIsCanMerge(item.Value))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsCanOneKeyMerge()
        {
            var clothesList = DataCenter.clothesData.Items;
            foreach (var item in clothesList)
            {
                if (CheckIsCanMerge(item.Value,true))
                {
                    return true;
                }
            }

            return false;
        }
        
        public bool CheckIsCanMerge(HeroEquipData selectMergeData,bool isOneKey=false)
        {
            if (selectMergeData == null) return false;
            if (isOneKey && selectMergeData.QuaId > 3) return false;
            if (IsMergeMaxQua(selectMergeData.QuaId)) return false;
            var quaCfg = ConfigCenter.HeroEquipQuaUpCfgColl.GetDataById(selectMergeData.QuaId);
            var partId = GetHeroEquipPart(selectMergeData.Id);
            long mergeNum = 0;
            if (quaCfg != null)
            {
                foreach (var item in DataCenter.clothesData.Items)
                {
                    if(item.Key == selectMergeData.Uid) continue;
                    var data = item.Value;
                    if (data != null)
                    {
                        if (DataCenter.clothesData.IsUseIng(data.Uid))
                        {
                            continue;
                        }

                        var selectCfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(selectMergeData.Id);
                        var dataCfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(data.Id);
                        if (selectCfg.RareType==1 && dataCfg?.RareType!=1)
                        {
                            continue;
                        }
                        var partId1 = GetHeroEquipPart(data.Id);
                        var selectQua = GetQuaSmallByQuaId(selectMergeData.QuaId);
                        var dataQua = GetQuaSmallByQuaId(data.QuaId);
                        var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(data.Id);
                        if (cfg != null)
                        {
                            if (quaCfg.Type == 1 && selectQua!=0 && selectQua == dataQua && partId == partId1 && selectMergeData.Id == data.Id && cfg.RareType == 1 && CheckStateStart(data.QuaId))
                            {
                                mergeNum ++;
                            }else if (quaCfg.Type == 2 && selectQua == dataQua && partId == partId1 && cfg.RareType == 1 && CheckStateStart(data.QuaId))
                            {
                                mergeNum ++;
                            }else if (quaCfg.Type == 3 && selectMergeData.QuaId == data.QuaId && partId == partId1 && selectMergeData.Id == data.Id)
                            {
                                mergeNum ++;
                            }
                        }
                    }
                }
                
                if (quaCfg.Type == 2)
                {
                    var needId = GetNeedItemId(selectMergeData.Id,true,selectMergeData.QuaId);
                    var ownNum = DataCenter.itemsData.GetItemCountById(needId);
                    mergeNum += ownNum;
                }
            }
            return quaCfg!=null && mergeNum>=quaCfg.Num;
        }

        public bool CheckUseRed(long uid,out ECellItemRedType redType)
        {
            bool isRed = false;
            redType = ECellItemRedType.ClothesUe;
            
            var heroEquipData = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
            if (heroEquipData != null)
            {
                var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(heroEquipData.Id);
                if (cfg != null)
                {
                    var partData = DataCenter.clothesData.GetHeroEquipDataByPart(cfg.PartId);
                    if (partData == null)
                    {
                        return true;
                    }
                    if (heroEquipData.Lv == partData.Lv)
                    {
                        isRed = heroEquipData.QuaId > partData.QuaId;
                    }
                    else if (heroEquipData.QuaId == partData.QuaId)
                    {
                        isRed = heroEquipData.Lv > partData.Lv;
                    }else if (heroEquipData.Id == partData.Id)
                    {
                        isRed = heroEquipData.QuaId > partData.QuaId;
                    }
                    else
                    {
                        isRed = heroEquipData.Lv > partData.Lv && heroEquipData.QuaId > partData.QuaId;
                    }
                }
            }

            if (isRed) return true;
            
            if (DataCenter.clothesData.CheckNewRed(uid))
            {
                redType = ECellItemRedType.Base;
                isRed = true;
            }

            return isRed;
        }

        public bool CheckIsCanUpLevel(HeroEquipData data)
        {
            if (data == null || IsMaxLevel(data)) return false;
            var rewardList = GetUpLevelNeedRewardList(data.Uid);
            foreach (var item in rewardList)
            {
                if (!UIHelper.CheckRewardIsEnough(item))
                {
                    return false;
                }
            }
            return true;
        }

        

        #region 网络相关

        public void SendClothesOneKeyUpLevel(long uid,Action succeedAction=null)
        {
            SendClothesUpOp(uid,2,succeedAction).Forget();
        }
        public void SendClothesUpLevel(long uid,Action succeedAction=null)
        {
            SendClothesUpOp(uid,1,succeedAction).Forget();
        }

        public async UniTask SendClothesUpOp(long uid,int op,Action succeedAction=null)
        {
            var req = new ReqHeroEquipLevelUp()
            {
                Uid = uid,
                Op = op,
            };
            var result =await GameNetworkManager.Instance.SendAsync<RspHeroEquipLevelUp>(req);

            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                DataCenter.clothesData.ChangeLevel(uid,result.rsp.Lv);
                Lodash.DealRewards(result.rsp.Costs.ToList(),false);
                succeedAction?.Invoke();
            }
        }

        public void SendUnLoadClothes(long uid,Action backAction=null)
        {
            if (!DataCenter.clothesData.IsUseIng(uid))
            {
                return;
            }

            var pos = DataCenter.clothesData.GetHeroEquipPos(uid);
            if (pos == -1)
            {
                return;
            }

            SendClothesUseOp(uid,2,pos,false,backAction).Forget();
        }

        public void SendUseClothes(long uid,Action backAction=null)
        {
            var data = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
            if (data == null) return;
            var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(data.Id);
            if(cfg==null) return;
            if (DataCenter.clothesData.IsNoneClothes(cfg.PartId))
            {
                SendUseClothesInherit(uid,false,backAction);
            }
            else
            {
                var partData = DataCenter.clothesData.GetHeroEquipDataByPart(cfg.PartId);
                if (partData.Lv > data.Lv && GetMaxLevel(data.QuaId) >= partData.Lv)
                {
                    var tempVm = new CommonMessageBoxViewModel(LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_27),LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_28),LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_CancelTxt),LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_ConfirmTxt), () =>
                    {
                        SendUseClothesInherit(uid,false,backAction);
                    }, () =>
                    {
                        SendUseClothesInherit(uid,true,backAction);
                    },null);
                    UIManager.Instance.OpenDialog<CommonMessageBox>(tempVm).Forget(); 
                }
                else
                {
                    SendUseClothesInherit(uid,false,backAction);
                }
            }
        }

        public void SendUseClothesInherit(long uid,bool inherit = false,Action backAction=null)
        {
            if (DataCenter.clothesData.IsUseIng(uid))
            {
                return;
            }

            var data = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
            if (data == null) return;
            var cfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(data.Id);
            if(cfg==null) return; 
            SendClothesUseOp(uid,1,cfg.PartId,inherit, backAction).Forget();
        }

        public async UniTask SendClothesUseOp(long uid,int op,int pos,bool inherit,Action succeedAction=null)
        {
            var req = new ReqHeroEquipWear()
            {
                Pos = pos,
                Uid = uid,
                Op = op,
                Inherit = inherit
            };
            var result =await GameNetworkManager.Instance.SendAsync<RspHeroEquipWear>(req);

            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                DataCenter.clothesData.ChangedClothes(pos,uid,op==1,inherit);
                succeedAction?.Invoke();
                if (op == 1)
                {
                    StartUseClothesAnimation(uid);
                }
            }
        }
        
        public async UniTask SendResetClothes(long uid,int op,Action succeedAction=null)
        {
            if (op == 1 && !IsCanResetLv(uid))
            {
                return;
            }
            
            if (op == 2 && !IsCanResetQua(uid))
            {
                return;
            }

            var req = new ReqHeroEquipDown()
            {
                Uid = uid,
                Op = op,
            };
            var result =await GameNetworkManager.Instance.SendAsync<RspHeroEquipDown>(req);

            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                Lodash.DealRewards(result.rsp.Rewards.ToList());
                var list = new List<Resource>();
                if (op == 1)
                {
                    DataCenter.clothesData.ResetLv(uid);
                }
                else
                {
                    DataCenter.clothesData.ResetQua(uid,GetQuaBaseId(uid));
                    var data = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
                    list.Add(new Resource()
                    {
                        Type = (int)RewardType.HeroEquip,
                        Id = data.Id,
                        Count = 0,
                        HeroEquip = UIHelper.HeroEquipDataToHeroEquip(data),
                    });
                }
                list.AddRange(result.rsp.Rewards.ToList());
                UIHelper.OpenCommonRewardView(list);

                succeedAction?.Invoke();
            }
        }

        public async UniTask SendMergeHeroEquip(long uid,List<long> heroEquipList,Reward reward,Action succeedAction=null)
        {

            var req = new ReqHeroEquipMerge()
            {
                Uid = uid,
                ItemCount = (int)(reward?.Count ?? 0),
            };
            req.Material.AddRange(heroEquipList);
            var result =await GameNetworkManager.Instance.SendAsync<RspHeroEquipMerge>(req);

            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                DataCenter.clothesData.RemoveHeroEquip(heroEquipList,false);
                Lodash.DealRewards(result.rsp.Rewards.ToList());
                Lodash.DealRewards(reward,false);
                var data = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
                if (data != null)
                {
                    var nextQuaId = GetNextQuaId(data.QuaId);
                    DataCenter.clothesData.ChangeQua(uid,nextQuaId);
                }
                succeedAction?.Invoke();
            }
        }
        
        public async UniTask SendOneKeyMergeHeroEquip(Action<List<HeroEquip>> succeedAction=null)
        {
            var req = new ReqHeroEquipMergeFast();
            var result =await GameNetworkManager.Instance.SendAsync<RspHeroEquipMergeFast>(req);

            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                DataCenter.clothesData.RemoveHeroEquip(result.rsp.CostUid.ToList(),false);
                DataCenter.clothesData.AddHeroEquipList(result.rsp.Equip.ToList());
                Lodash.DealRewards(result.rsp.Rewards.ToList());
                succeedAction?.Invoke(result.rsp.Equip.ToList());
            }
        }
        
        #endregion
    }
}
