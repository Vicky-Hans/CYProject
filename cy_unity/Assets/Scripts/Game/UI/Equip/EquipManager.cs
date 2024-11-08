using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Proto;
using DH.UIFramework;
using DHFramework;
using UnityEngine;

namespace DH.Game
{
    public partial class EquipManager : ObservableSingleton<EquipManager>
    {
        public int BaseEquipId = 0;
        public int GoldEquipId = 9;
        [AutoNotify] public int curSelectEquipId;
        [AutoNotify] private int replaceEquipId;

        public void SwitchTabClear()
        {
            CurSelectEquipId = 0;
            ReplaceEquipId = 0;
        }
        
        
        public Dictionary<string,int> GetEquipAttrList(int id)
        {
            Dictionary<string,int> dic = new();
            var equipCfg = ConfigCenter.EquipCfgColl.GetDataById(id);
            var equipData = DataCenter.equipData.GetEquipItemData(id);
            if (equipCfg != null)
            {
                if (equipCfg.Attr != null)
                {
                    foreach (var item in equipCfg.Attr)
                    {
                        if (!dic.ContainsKey(item.Type))
                        {
                            dic.Add(item.Type,item.Value);
                        }
                        else
                        {
                            dic[item.Type] += item.Value;
                        }
                    }
                }

                var level = equipData?.Lv ?? 1;
                for (int i = 1; i <=level; i++)
                {
                    var lvCfg = ConfigCenter.EquipLvCfgColl.GetDataById(i);
                    if(lvCfg?.AttrAdd == null) continue;
                    for (int j = 0; j < lvCfg.AttrAdd.Count; j++)
                    {
                        if (lvCfg.AttrAdd[j].Content == id)
                        {
                            if (!dic.ContainsKey(lvCfg.AttrAdd[j].Type))
                            {
                                dic.Add(lvCfg.AttrAdd[j].Type,lvCfg.AttrAdd[j].Value);
                            }
                            else
                            {
                                dic[lvCfg.AttrAdd[j].Type] += lvCfg.AttrAdd[j].Value;
                            }
                        }
                    }
                }
            }
            return dic;
        }
        
        public Dictionary<string,int> GetEquipAttrListByModelId(int modelId)
        {
            Dictionary<string, int> dic = new();
            var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(modelId);
            if (modelCfg == null)
            {
                DHLog.Error($"没有 配置信息 EquipModelCfg  请检查配置 id {modelId}");
                return dic;
            }
            var equipCfg = ConfigCenter.EquipCfgColl.GetDataById(modelCfg.Equip);
            if (equipCfg == null)
            {
                DHLog.Error($"没有 配置信息 EquipCfg  请检查配置 id {modelCfg.Equip}");
                return dic;
            }
            dic = GetEquipAttrList(modelCfg.Equip);
            
            int GetPropertyIndex(string type)
            {
                var index = -1;
                if (equipCfg.AttrType == null) return index;
                for (int i = 0; i < equipCfg.AttrType.Count; i++)
                {
                    if (equipCfg.AttrType[i] == type)
                    {
                        index = i;
                        break;
                    }
                }
                return index;
            }

            Dictionary<string, int> ret = new();
            // 基础属性 
            if (equipCfg.Attr != null && equipCfg.Attr.Count > 0)
            {
                foreach (var item in equipCfg.Attr)
                {
                    var tempValue = dic.TryGetValue(item.Type, out var value) ? value : item.Value;
                    // 根据基础值 的加 属性
                    var index = GetPropertyIndex(item.Type);
                    if (index != -1)
                    {
                        tempValue = Mathf.FloorToInt(tempValue * modelCfg.Compose[index] * GameConst.AttributeDivisor + 0.5f);
                    }
                    ret.Add(item.Type, tempValue);
                }
            }
            return ret;
        }
        
        /// <summary>
        /// 获取装备模型等级
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetEquipModelLevel(int modelId)
        {
            var cfg = ConfigCenter.EquipModelCfgColl.GetDataById(modelId);
            if (cfg == null)
            {
                DHLog.Error($"没有 配置信息 EquipModelCfg  请检查配置 id {modelId}");
                return 0;
            }

            return cfg.Class;
        }


        public Dictionary<string, int> GetEquipNextAddAttrList(int id)
        {
            Dictionary<string,int> dic = new();
            var equipData = DataCenter.equipData.GetEquipItemData(id);
            if (equipData is { Lv: > 0 })
            {
                var lvCfg = ConfigCenter.EquipLvCfgColl.GetDataById(equipData.Lv+1);
                if(lvCfg==null) return dic;
                for (int j = 0; j < lvCfg.AttrAdd.Count; j++)
                {
                    if (lvCfg.AttrAdd[j].Content == id)
                    {
                        if (!dic.ContainsKey(lvCfg.AttrAdd[j].Type))
                        {
                            dic.Add(lvCfg.AttrAdd[j].Type,lvCfg.AttrAdd[j].Value);
                        }
                        else
                        {
                            dic[lvCfg.AttrAdd[j].Type] += lvCfg.AttrAdd[j].Value;
                        }
                    }
                }
            }

            return dic;
        }

        public List<EquipCfg> GetEquipLockList()
        {
            List<EquipCfg> lockList = new();
            var allEquipList = ConfigCenter.EquipCfgColl.DataItems.ToList();
            foreach (var item in allEquipList)
            {
                if (!DataCenter.equipData.IsOwn(item.Id))
                {
                    lockList.Add(item);
                }
            }

            return lockList;
        }
        
        public List<EquipSlotsCfg> GetEquipSlotsList()
        {
            return ConfigCenter.EquipSlotsCfgColl.DataItems.ToList();
        }

        public bool IsExistNoneSlots()
        {
            int ownSlots = 0;
            var slots = GetEquipSlotsList();
            for (int i = 0; i < slots.Count; i++)
            {
                if (DataCenter.mainStageData.IsPassChapter(slots[i].Unlock))
                {
                    ownSlots += 1;
                }
            }

            var battleNum = DataCenter.equipData.GetBattleEquipCount();
            return ownSlots > battleNum;
        }
        
        public int GetSlotsCnt()
        {
            int ownSlots = 0;
            var slots = GetEquipSlotsList();
            for (int i = 0; i < slots.Count; i++)
            {
                if (DataCenter.mainStageData.IsPassChapter(slots[i].Unlock))
                {
                    ownSlots += 1;
                }
            }
            return ownSlots;
        }
        

        public string GetEquipName(int id)
        {
             var cfg = ConfigCenter.EquipCfgColl.GetDataById(id);
             return cfg!=null ? GetModelName(GetEquipIconShowModelId(cfg.Id)):string.Empty;
        }
        
        public string GetIconPath(int id,int level = -1)
        {
            return GetIconPath(ConfigCenter.EquipCfgColl.GetDataById(id),level);
        }
        public string GetIconPath(EquipCfg cfg, int level = -1)
        {
            if (cfg != null)
            {
                return GetModelIconPath(GetEquipIconShowModelId(cfg.Id, level));
            }

            return UIHelper.NoneImagePath();
        }
        
        public int GetEquipIconShowModelId(int id, int level = -1)
        {
            int showModelId = 0;
            var cfg = ConfigCenter.EquipCfgColl.GetDataById(id);
            int curLevel = level;
            if (level == -1)
            {
                var data = DataCenter.equipData.GetEquipItemData(id);
                curLevel = data?.Lv ?? 1;
            }
            if (cfg?.Model != null)
            {
                foreach (var item in cfg.Model)
                {
                    foreach (var modelId in item)
                    {
                        if (showModelId==0)
                        {
                            showModelId = modelId;
                        }
                        else
                        {
                            var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(modelId);
                            if (modelCfg != null && curLevel >= modelCfg.ShowLv)
                            {
                                showModelId = modelId;
                            }
                        }
                    }
                }
            }

            return showModelId;
        }
        
        public string GetBgPathByEquipId(int equipId)
        {
            var cfg  = ConfigCenter.EquipCfgColl.GetDataById(equipId);
            if (cfg == null)
            {
                return $"common[commom_equipbg_1]";
                //return UIHelper.NoneImagePath();
            }
            return $"common[commom_equipbg_{cfg.Quality}]";
        }
        
        public int GetQualityByEquipModelId(int equipModelId)
        {
            var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(equipModelId);
            if (modelCfg != null)
            {
                var cfg  = ConfigCenter.EquipCfgColl.GetDataById(modelCfg.Equip);
                if (cfg == null)
                {
                    return cfg.Quality;
                }

                return 0;
            }

            return 0;
        }
        
        public string GetBgPathByEquipModelId(int equipModelId)
        {
            var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(equipModelId);
            if (modelCfg != null)
            {
                return GetBgPathByEquipId(modelCfg.Equip);
            }
            return UIHelper.NoneImagePath();
        }

        public string GetBgPath(int id)
        {
            return GetBgPath(ConfigCenter.EquipCfgColl.GetDataById(id));
        }
        public string GetBgPath(EquipCfg cfg)
        {
            if (cfg != null)
            {
                //根据品质获得具体背景
                return $"equip[equip_panel_{cfg.Quality}]";
            }
            return UIHelper.NoneImagePath();
        }
        
        public string GetTypeIconPath(int id)
        {
            return GetTypeIconPath(ConfigCenter.EquipCfgColl.GetDataById(id));
        }
        public string GetTypeIconPath(EquipCfg cfg)
        {
            if (cfg != null)
            {
                //根据品质获得具体背景
                var gridCfg = ConfigCenter.EquipGridCfgColl.GetDataById(cfg.GridType);
                if (gridCfg != null)
                {
                    return gridCfg.EquipGridIcon;
                }
            }

            return UIHelper.NoneImagePath();
        }
        
        public string GetAttrIconPath(int id)
        {
            return GetAttrIconPath(ConfigCenter.EquipCfgColl.GetDataById(id));
        }
        public string GetAttrIconPath(EquipCfg cfg)
        {
            if (cfg != null)
            {
                
            }

            return UIHelper.NoneImagePath();
        }

        public int GetEquipAttrNum(int id)
        {
            int attrNum = 0;
            var cfg = ConfigCenter.EquipCfgColl.GetDataById(id);
            if (cfg != null)
            {
                for (int i = 0; i < cfg.Model.Count; i++)
                {
                    if (attrNum < cfg.Model[i].Count)
                    {
                        attrNum = cfg.Model[i].Count;
                    }
                }
            }

            return attrNum;
        }
        
        public bool IsEnoughEquipAttr(int id)
        {
            var cfg = ConfigCenter.EquipCfgColl.GetDataById(id);
            var data = DataCenter.equipData.GetEquipItemData(id);
            if (data == null) return false;
            if (cfg != null)
            {
                for (int i = 0; i < cfg.Model.Count; i++)
                {
                    if (data.Lv > i && cfg.Model.Count > 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public string GetModelIconPath(int id)
        {
            return GetModelIconPath(ConfigCenter.EquipModelCfgColl.GetDataById(id));
        }
        public string GetModelIconPath(EquipModelCfg cfg)
        {
            if (cfg != null)
            {
                return cfg.Pic;
            }

            return UIHelper.NoneImagePath();
        }
        
        public string GetModelName(int id)
        {
            var cfg = ConfigCenter.EquipModelLanguageCfgColl.GetDataById(id);
            if (cfg != null)
            {
                return cfg.WeaponName;
            }
            return UIHelper.NoneImagePath();
        }

        public string GetEquipAttrTypeIcon(int id)
        {
            var cfg = ConfigCenter.EquipAttrCfgColl.GetDataById(id);
            if (cfg != null)
            {
                return cfg.EquipAttrIcon;
            }
            return UIHelper.NoneImagePath();
        }
        
        public string GetEquipAttrTypeName(int id)
        {
            var cfg = ConfigCenter.EquipAttrCfgColl.GetDataById(id);
            if (cfg != null)
            {
                return cfg.EquipAttrName;//需要修改为多语言
            }
            return string.Empty;
        }

        public int GetEquipLvNeedItem(int id)
        {
            return GetEquipLvNeedItem(ConfigCenter.EquipCfgColl.GetDataById(id));
        }
        
        public int GetEquipLvNeedItem(EquipCfg cfg)
        {
            if (cfg != null)
            {
                var itemData = DataCenter.equipData.GetEquipItemData(cfg.Id);
                if (itemData != null)
                {
                    var lvCfg = ConfigCenter.EquipLvCfgColl.GetDataById(itemData.Lv);
                    if (lvCfg != null)
                    {
                        if (cfg.Quality == 2)
                        {
                            return lvCfg.Consume1;
                        }else if (cfg.Quality == 3)
                        {
                            return lvCfg.Consume2;
                        }
                        else if (cfg.Quality == 4)
                        {
                            return lvCfg.Consume3;
                        }
                        else if(cfg.Quality == 5)
                        {
                            return lvCfg.Consume4;
                        }
                    }
                }
            }
            return -1;
        }
        
        public List<Reward> GetEquipLvNeedGoldItem(EquipCfg cfg)
        {
            if (cfg != null)
            {
                var itemData = DataCenter.equipData.GetEquipItemData(cfg.Id);
                if (itemData != null)
                {
                    var lvCfg = ConfigCenter.EquipLvCfgColl.GetDataById(itemData.Lv);
                    if (lvCfg != null)
                    {
                        if (cfg.Quality == 2)
                        {
                            return lvCfg.ConsumeCoin1;
                        }else if (cfg.Quality == 3)
                        {
                            return lvCfg.ConsumeCoin2;
                        }
                        else if (cfg.Quality == 4)
                        {
                            return lvCfg.ConsumeCoin3;
                        }
                        else if (cfg.Quality == 5)
                        {
                            return lvCfg.ConsumeCoin4;
                        }
                    }
                }
            }
            return null;
        }

        public string GetEquipSkillIcon(int skillId)
        {
            var cfg = ConfigCenter.EquipSkillCfgColl.GetDataById(skillId);
            if (cfg != null)
            {
                return cfg.Icon;
            }

            return UIHelper.NoneImagePath();
        }
        
        public string GetEquipSkillDesc(int skillId,SkillPreviewSkillType type= SkillPreviewSkillType.None)
        {
            try
            {
                var cfg = ConfigCenter.EquipSkillCfgColl.GetDataById(skillId);
                var cfgL = ConfigCenter.EquipSkillLanguageCfgColl.GetDataById(skillId);
                if (cfg != null && cfgL!=null)
                {
                    string desc = cfgL.Dec;
                    UIHelper.GetDescLinkInfo(ref desc,SkillPreviewSkillType.EquipModel,cfg.PreviewSkill);
                    switch (cfg.Value?.Count)
                    {
                        case 1: return DHUtility.Format(desc, cfg.Value[0]);  
                        case 2: return DHUtility.Format(desc, cfg.Value[0], cfg.Value[1]);  
                        case 3: return DHUtility.Format(desc, cfg.Value[0], cfg.Value[1], cfg.Value[2]);  
                        case 4: return DHUtility.Format(desc, cfg.Value[0], cfg.Value[1], cfg.Value[2],cfg.Value[3]);  
                        case 5: return DHUtility.Format(desc, cfg.Value[0], cfg.Value[1], cfg.Value[2],cfg.Value[3],cfg.Value[4]);  
                        default: return cfgL.Dec;
                        
                    }
                }
            }
            catch (Exception e)
            {
                DHLog.Error($"技能Id： {skillId}   描述内需要的参数 与实际参数不一致");
            }
    
            return string.Empty;
        }
        
        //没有技能名称的显示需求暂时不处理
        public string GetEquipSkillName(int skillId)
        {
            // var cfg = ConfigCenter.EquipSkillCfgColl.GetDataById(skillId);
            // if (cfg != null)
            // {
            //     return "对应的名称";
            // }
            return string.Empty;
        }

        /// <summary>
        /// 检查技能是否解锁
        /// </summary>
        /// <param name="equipSkillId"></param>
        /// <returns></returns>
        public bool CheckEquipSkillUnlock(int equipSkillId)
        {
            var cfg = ConfigCenter.EquipSkillCfgColl.GetDataById(equipSkillId);
            if (cfg == null)
            {
                return false;
            }

            var tempData = DataCenter.equipData.GetEquipItemData(cfg.EquipId);
            if (tempData == null)
            {
                return false;
            }

            return tempData.Lv >= cfg.LvUnlockId;

        }
        /// <summary>
        /// 根据天赋Id获取对应技能是否解锁
        /// </summary>
        /// <param name="talentId"></param>
        /// <returns></returns>
        public bool CheckEquipSkillUnlockByTalentId(int talentId)
        {
            var talentCfg = ConfigCenter.TalentCfgColl.GetDataById(talentId);
            if (talentCfg == null) return false;
            var equipSkillCfg = ConfigCenter.EquipSkillCfgColl.GetDataById(talentCfg.EquipSkillId);
            if (equipSkillCfg == null) return false;
            var tempData = DataCenter.equipData.GetEquipItemData(equipSkillCfg.EquipId);
            if (tempData == null) return false;
            return tempData.Lv >= equipSkillCfg.LvUnlockId;
        }
        public bool IsMaxLevel(int id,int lv)
        {
            var cfg = ConfigCenter.EquipCfgColl.GetDataById(id);
            if (cfg != null)
            {
                return cfg.MaxLv <= lv;
            }
            return false;
        }

        public string GetEquipType(int id)
        {
            var cfg = ConfigCenter.EquipGridCfgColl.GetDataById(id);
            return GetEquipType(cfg);
        }
        
        public string GetEquipType(EquipGridCfg cfg)
        {
            if (cfg != null)
            {
                return cfg.EquipGridIcon;
            }
            return UIHelper.NoneImagePath();
        }
        
        public EquipItemData GetEquipDataByPos(int pos)
        {
            EquipWearFormation formation = null;
            if (DataCenter.equipData.Formations.TryGetValue(DataCenter.equipData.CurrWearFormation, out var value))
            {
                formation = value;
            }
            if (formation != null)
            {
                var sortedList = formation.Data.ToList();
                for (int i = sortedList.Count-1; i >=0; i--)
                {
                    if (sortedList[i].Value == 0)
                    {
                        sortedList.RemoveAt(i);
                    }
                }
                UIHelper.SortList(sortedList, (itemA, itemB) => itemA.Value > itemB.Value);
                for (int i = 0; i < sortedList.Count; i++)
                {
                    if (i + 1 == pos)
                    {
                        return DataCenter.equipData.GetEquipItemData(sortedList[i].Key);
                    }
                }
            }
            return null;
        }

        public string GetTargetDesc(int id)
        {
            var cfg = ConfigCenter.EquipCfgColl.GetDataById(id);
            if (cfg != null)
            {
                if (cfg.Target == 1)
                {
                    return LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_10);
                }
                if (cfg.Target == 2)
                {
                    return LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_11);
                }
                if (cfg.Target == 3)
                {
                    return LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_12);
                }
            }
            return string.Empty;
        }

        public void CheckUnLockNewEquip(int passChapter)
        {
            var allEquip = ConfigCenter.EquipCfgColl.DataItems.ToList();
            foreach (var item in allEquip)
            {
                if (item.Unlock == passChapter && !DataCenter.equipData.IsOwn(item.Id))
                {
                    DataCenter.equipData.AddEquipData(item.Id);
                }
            }
        }

        public string GetEquipAtkTypeIcon(EquipCfg cfg)
        {
            if (cfg != null && cfg.AtkType != 4)
            {
                return $"equip[icon_equipType_0{cfg.AtkType}]";
            }
            return UIHelper.NoneImagePath();
        }

        public bool CheckEquipIsCanUse(int id,int replaceId = 0)
        {
            if (!CheckEquipIsDef(id)) return true;
            var slotsCnt = GetSlotsCnt();
            var equipNum = DataCenter.equipData.GetBattleEquipCount();
            if (slotsCnt-equipNum==1)
            {
                return !CheckAllEquipIsDef();
            }else if(slotsCnt<=equipNum && replaceId!=0)
            {
                return !CheckAllEquipIsDef(replaceId);
            }

            return true;
        }

        public bool CheckAllEquipIsDef(int delId=0)
        {
            var list = DataCenter.equipData.GetOwnUseEquipList();
            foreach (var item in list)
            {
                if(item==delId) continue;
                var cfg = ConfigCenter.EquipCfgColl.GetDataById(item);
                if (cfg.AtkType != (int)EquipAtkType.Defender && cfg.AtkType != (int)EquipAtkType.Money)
                {
                    return false;
                }
            }
            return true;
        }
        
        public bool CheckEquipIsDef(int id)
        {
            var cfg = ConfigCenter.EquipCfgColl.GetDataById(id);
            if (cfg!=null && cfg.AtkType == (int)EquipAtkType.Defender)
            {
                return true;
            }

            return false;
        }


        #region 网络相关
        public async UniTask SendEquipRemoveBattle(int id)
        {
            await SendEquipWearOp(id,2);
        }
        
        public async UniTask SendEquipBattle(int newId,int oldId=0)
        {
            await SendEquipWearOp(newId,oldId==0?1:3,oldId);
        }

        public async UniTask SendEquipWearOp(int id,int op,int oldId=0)
        {
            var req = new ReqEquipWearOp()
            {
                Id = id,
                Op = op,
                OldId = oldId,
            };
            var result =await GameNetworkManager.Instance.SendAsync<RspEquipWearOp>(req);

            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                DataCenter.equipData.ChangeWearFormationData(result.rsp.Data);
            }
        }
        
        public async UniTask SendEquipLevelUp(int id,Action backAction)
        {
            var req = new ReqEquipLevelUp()
            {
                Id = id,
            };
            var result =await GameNetworkManager.Instance.SendAsync<RspEquipLevelUp>(req);

            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                Lodash.DealRewards(result.rsp.Cost.ToList(),false);
                DataCenter.equipData.ChangeEquipData(result.rsp.Data);
                backAction?.Invoke();
                AudioManager.Instance.PlayEquipLevelUp();
            }
        }
        
        public async UniTask SendEquipSwitch(int id,Action backAction=null)
        {
            var req = new ReqEquipSwitch()
            {
                Id = id,
            };
            var result =await GameNetworkManager.Instance.SendAsync<RspEquipSwitch>(req);

            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                CurSelectEquipId = 0;
                DataCenter.equipData.CurrWearFormation = id;
                backAction?.Invoke();
            }
        }


        #endregion
    }
}
