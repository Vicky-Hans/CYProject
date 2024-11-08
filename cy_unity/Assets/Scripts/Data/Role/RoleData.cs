using System;
using System.Collections.Generic;
using System.Linq;
using DH.Config;
using DH.Proto;
using DHFramework;

namespace DH.Data
{
    [ProtoWrap(typeof(Hero))]
    public partial class RoleData : BaseData
    {
        /// <summary>
        /// 英雄是否解锁
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public bool IsUnlock(int roleId)
        {
            return Heroes.ContainsKey(roleId);
        }

        /// <summary>
        /// 获得英雄等级
        /// </summary>
        /// <returns></returns>
        public int GetHeroLevel(int roleId)
        {
            if (Heroes.ContainsKey(roleId))
            {
                return Heroes[roleId].Lv;
            }
            return 0;
        }
        /// <summary>
        /// 获得英雄星级
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public int GetHeroStar(int roleId)
        {
            if (Heroes.ContainsKey(roleId))
            {
                return Heroes[roleId].Star;
            }
            return 0;
        }

        /// <summary>
        /// 是否可以升星 （激活之后）
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public bool IsCanStarUp(int roleId)
        {
            if (!Heroes.ContainsKey(roleId)) return false;
            if (IsMaxStar(roleId))return false;
            var mainCfg = ConfigCenter.HeroMainCfgColl.GetDataById(roleId);
            var star = GetHeroStar(roleId); 
            var mStarCfg = ConfigCenter.HeroStarCfgColl.GetDataById(star);
            Reward Rew = new Reward(RewardType.Item,mainCfg.ItemId,mStarCfg.StarCost);
            return Lodash.CheckRewardIsEnough(Rew);
        }
        /// <summary>
        /// 是否可以升级
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public bool IsCanLevelUp(int roleId)
        {
            if (!Heroes.ContainsKey(roleId)) return false;
            if (IsMaxLevel(roleId) || IsMaxLevel2(roleId))return false;
            var lv = GetHeroLevel(roleId); 
            var mLvCfg = ConfigCenter.HeroLevelCfgColl.GetDataById(lv);
            var mainCfg = ConfigCenter.HeroMainCfgColl.GetDataById(roleId);
            var res  = mLvCfg.LevelCost[mainCfg.Qlt switch
            {
                3 => 0,
                4 => 1,
                _ => 2
            }];
            return Lodash.CheckRewardIsEnough(res);

        }
        /// <summary>
        /// 是否可以激活 
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public bool IsCanGettingRole(int roleId)
        {
            if (Heroes.ContainsKey(roleId)) return false;
            var mainCfg = ConfigCenter.HeroMainCfgColl.GetDataById(roleId);
            Reward Rew = new Reward(RewardType.Item,mainCfg.ItemId,mainCfg.UnlockItemNum);
            return Lodash.CheckRewardIsEnough(Rew);
        }

        /// <summary>
        /// 是不是当前星级的最大等级
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public bool IsMaxLevel(int roleId)
        {
            var star = GetHeroStar(roleId);
            var maxLevel = ConfigCenter.HeroStarCfgColl.GetDataById(star).LevelLimitAdd;
            return GetHeroLevel(roleId) == maxLevel;
        }


        /// <summary>
        /// 最大等级（与星级无关）
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public bool IsMaxLevel2(int roleId)
        {
            var lv = GetHeroLevel(roleId);
            var maxLevel = ConfigCenter.HeroLevelCfgColl.DataItems.Last();
            return lv >= maxLevel.Id;
        }
        
        public bool IsMaxStar(int roleId)
        {
            var star = GetHeroStar(roleId);
            var maxStarCfg = ConfigCenter.HeroStarCfgColl.DataItems.Last();
            return star >= maxStarCfg.Id;
        }
        public int HeroLevelUpId;
        public void HeroLevelUp(int roleId)
        {
            HeroLevelUpId = roleId;
            RaisePropertyChanged(nameof(HeroLevelUpId));
        }
        public int HeroStarUpId;
        public void HeroStarUp(int roleId)
        {
            HeroStarUpId = roleId;
            RaisePropertyChanged(nameof(HeroStarUpId));
        }
        /// <summary>
        /// 英雄上阵
        /// </summary>
        /// <param name="roleId"></param>
        public void HeroInBattle(int roleId)
        {
            FmtHero = roleId;
        }

        /// <summary>
        /// 单个英雄是否有红点
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public bool HeroRed(int roleId)
        {
            return IsCanLevelUp(roleId) || IsCanStarUp(roleId);
        }
        
        public bool AllHeroRed()
        {
            foreach (var Item in Heroes)
            {
                if (IsCanLevelUp(Item.Key) || IsCanStarUp(Item.Key))
                {
                    return true;
                }
            }

            var items = ConfigCenter.HeroMainCfgColl.DataItems;
            for (int i = 0; i < items.Count; i++)
            {
                if ( IsCanGettingRole(items[i].Id))
                {
                    return true;
                }
            }
            return false;
        }


        #region 英雄属性相关 显示用

        public decimal GetAttribute(int roleId ,int id)
        {
            AttributeMgr attrMgr = new AttributeMgr(null);
            attrMgr.Modify(HeroAttr(roleId));
            attrMgr.Modify(OtherHeroSkillAttr(roleId));
            
            switch ((AttributeType)id)
            {
                case AttributeType.Hp:
                    var hpTemp = attrMgr.AllAttributes.ContainsKey((int)AttributeType.HpBonus) ? attrMgr.AllAttributes[(int)AttributeType.HpBonus].DValueBase : 0;
                    var hpTemp2 = attrMgr.AllAttributes.ContainsKey((int)AttributeType.RoleHpBonus) ? attrMgr.AllAttributes[(int)AttributeType.RoleHpBonus].DValueBase : 0;
                    return attrMgr.AllAttributes[id].DValueBase*(1+hpTemp+hpTemp2);
                case AttributeType.Atk:
                    var atkTemp = attrMgr.AllAttributes.ContainsKey((int)AttributeType.AttackBonus) ? attrMgr.AllAttributes[(int)AttributeType.AttackBonus].DValueBase : 0;
                    var atkTemp2 = attrMgr.AllAttributes.ContainsKey((int)AttributeType.RoleAttackBonus) ? attrMgr.AllAttributes[(int)AttributeType.RoleAttackBonus].DValueBase : 0;
                    return attrMgr.AllAttributes[id].DValueBase*(1+atkTemp+atkTemp2);
            }
            return attrMgr.AllAttributes[id].DValueBase;
        }
        public Attribute GetLvAttribute(int roleId ,int id,int Lv)
        {
            AttributeMgr attrMgr = new AttributeMgr(null);
            if (IsMaxLevel2(roleId)) return null;
            var lvCfg = ConfigCenter.HeroLevelCfgColl.GetDataById(Lv);
            var mainCfg = ConfigCenter.HeroMainCfgColl.GetDataById(roleId);
            attrMgr.Modify(lvCfg.AttrAdd[mainCfg.Qlt switch
            {
                3 => 0,
                4 => 1,
                _ => 2
            }]);
            return attrMgr.AllAttributes[id];
        }

        
        
        
        
        public  List<Config.Attribute> HeroAttr(int roleId)
        {
            var attrs= new List<Config.Attribute>();
            
            var heroCfg = ConfigCenter.HeroMainCfgColl.GetDataById(roleId);
            var heroLvCfg = ConfigCenter.HeroLevelCfgColl.GetDataById(GetHeroLevel(roleId)==0?1:GetHeroLevel(roleId));
            var heroStarCfg = ConfigCenter.HeroStarCfgColl.GetDataById(GetHeroStar(roleId));
            
            
            if (heroCfg.Attr!=null)
            {
                for (int i = 0; i < heroCfg.Attr.Count; i++)
                {
                    var attr = new Config.Attribute(heroCfg.Attr[i].Type,heroCfg.Attr[i].Value);
                    attrs.Add(attr);
                }
            }
            
            //等级基础加成
            if (heroLvCfg.AttrAdd!=null)
            {
                attrs.AddRange(heroLvCfg.AttrAdd[heroCfg.Qlt switch
                {
                    3 => 0,
                    4 => 1,
                    _ => 2
                }]);
            }
            //星级基础加成
            if (heroStarCfg.AttrAdd!=null)
            {
                attrs.AddRange(heroStarCfg.AttrAdd[heroCfg.Qlt switch
                {
                    3 => 0,
                    4 => 1,
                    _ => 2
                }]);
            }

            //主动技能
            var mainSkillCfg  = GetSkillCfg(roleId,1,1);
            if (mainSkillCfg!=null && mainSkillCfg.SkillId !=0 && mainSkillCfg.HeroAttr == 1)
            {
                var skillConfig = ConfigCenter.SkillCfgColl.GetDataById(mainSkillCfg.SkillId);
                if (skillConfig.Result!=null)
                {
                    attrs.AddRange(skillConfig.Result);
                }
            }
            //被动技能
            var passiveSkillCfg  = GetSkillCfg(roleId,4,1);
            if (passiveSkillCfg!=null && passiveSkillCfg.SkillId !=0 && passiveSkillCfg.HeroAttr == 1)
            {
                var skillConfig = ConfigCenter.SkillCfgColl.GetDataById(passiveSkillCfg.SkillId);
                if (skillConfig.Result!=null)
                {
                    attrs.AddRange(skillConfig.Result);
                }
            }
            
            //等级技能
            var items = ConfigCenter.HeroSkillCfgColl.DataItems;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].HeroId==roleId && items[i].SkillType ==3 && GetHeroLevel(roleId)>= items[i].LvAstrict)
                {
                    var lvSkillCfg  = GetSkillCfg(roleId,3, items[i].LvAstrict);
                    if (lvSkillCfg!=null && lvSkillCfg.SkillId !=0  && lvSkillCfg.HeroAttr == 1)
                    {
                        var skillConfig = ConfigCenter.SkillCfgColl.GetDataById(lvSkillCfg.SkillId);
                        if (skillConfig.Result!=null)
                        {
                            attrs.AddRange(skillConfig.Result);
                        }
                    }
                }
            }
            
            //星级技能
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].HeroId == roleId && items[i].SkillType == 2 &&
                    GetHeroStar(roleId) >= items[i].LvAstrict)
                {
                    var starSkillCfg = GetSkillCfg(roleId, 2, items[i].LvAstrict);
                    if (starSkillCfg != null && starSkillCfg.SkillId != 0  && starSkillCfg.HeroAttr == 1)
                    {
                        var skillConfig =
                            ConfigCenter.SkillCfgColl.GetDataById(starSkillCfg.SkillId);
                        if (skillConfig.Result != null)
                        {
                            attrs.AddRange(skillConfig.Result);
                        }
                    }
                }
            }

            return attrs;
        }
        
        /// <summary>
        /// 除特定英雄 其他英雄技能加成
        /// </summary>
        /// <param name="thisRoleId"></param>
        /// <returns></returns>
        public  List<Config.Attribute> OtherHeroSkillAttr(int thisRoleId)
        {
            var attrs= new List<Config.Attribute>();

            foreach (var item in Heroes)
            {
                if (thisRoleId == item.Key)continue;
                var roleId = item.Key;
                
                //主动技能
                var mainSkillCfg  = GetSkillCfg(roleId,1,1);
                if (mainSkillCfg!=null && mainSkillCfg.SkillId !=0 &&  mainSkillCfg.Effect == -1 && mainSkillCfg.HeroAttr == 1)
                {
                    var skillConfig = ConfigCenter.SkillCfgColl.GetDataById(mainSkillCfg.SkillId);
                    if (skillConfig.Result!=null)
                    {
                        attrs.AddRange(skillConfig.Result);
                    }
                }
                //被动技能
                var passiveSkillCfg  = GetSkillCfg(roleId,4,1);
                if (passiveSkillCfg!=null && passiveSkillCfg.SkillId !=0 &&  passiveSkillCfg.Effect == -1 && passiveSkillCfg.HeroAttr == 1)
                {
                    var skillConfig = ConfigCenter.SkillCfgColl.GetDataById(passiveSkillCfg.SkillId);
                    if (skillConfig.Result!=null)
                    {
                        attrs.AddRange(skillConfig.Result);
                    }
                }
            
                //等级技能
                var items = ConfigCenter.HeroSkillCfgColl.DataItems;
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].HeroId == roleId && items[i].SkillType == 3 &&
                        GetHeroLevel(roleId) >= items[i].LvAstrict)
                    {
                        var lvSkillCfg = GetSkillCfg(roleId, 3,  items[i].LvAstrict);
                        if (lvSkillCfg != null && lvSkillCfg.SkillId != 0 &&
                            lvSkillCfg.Effect == -1 && lvSkillCfg.HeroAttr == 1 )
                        {
                            var skillConfig =
                                ConfigCenter.SkillCfgColl.GetDataById(lvSkillCfg.SkillId);
                            if (skillConfig.Result != null)
                            {
                                attrs.AddRange(skillConfig.Result);
                            }
                        }
                    }
                }

                //星级技能
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].HeroId == roleId && items[i].SkillType == 2 &&
                        GetHeroStar(roleId) >= items[i].LvAstrict)
                    {
                        var starSkillCfg = GetSkillCfg(roleId, 2, items[i].LvAstrict);
                        if (starSkillCfg != null && starSkillCfg.SkillId != 0 &&
                            starSkillCfg.Effect == -1 && starSkillCfg.HeroAttr == 1 )
                        {
                            var skillConfig =
                                ConfigCenter.SkillCfgColl.GetDataById(starSkillCfg.SkillId);
                            if (skillConfig.Result != null)
                            {
                                attrs.AddRange(skillConfig.Result);
                            }
                        }
                    }
                }

            }
            return attrs;
        }
        
        /// <summary>
        /// 获取英雄技能表
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="skilltype">技能类型 1主动 2星级 3等级 4被动</param>
        /// <param name="nums">数值</param>
        /// <returns></returns>
        public HeroSkillCfg GetSkillCfg(int roleId,int skillType,int nums)
        {
            var items = ConfigCenter.HeroSkillCfgColl.DataItems;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].HeroId == roleId && items[i].SkillType == skillType && items[i].LvAstrict == nums)
                {
                    return items[i];
                }
            }

            return null;
        }

        #endregion


        #region 英雄资源相关
        
        public string HeroHeadIcon(int roleId)
        {
            var mainCfg = ConfigCenter.HeroMainCfgColl.GetDataById(roleId);
            if (mainCfg == null)
            {
                DHLog.Error("英雄Id不存在"+roleId);
                return "common[common_alpha]";
            }

            return mainCfg.HeadIcon;
        }
        
        public string HeroCardIcon(int roleId)
        {
            var mainCfg = ConfigCenter.HeroMainCfgColl.GetDataById(roleId);
            if (mainCfg == null)
            {
                DHLog.Error("英雄Id不存在"+roleId);
                return "common[common_alpha]";
            }

            return mainCfg.Card;
        }     
        
        public string HeroModel(int roleId)
        {
            var mainCfg = ConfigCenter.HeroMainCfgColl.GetDataById(roleId);
            if (mainCfg == null)
            {
                DHLog.Error("英雄Id不存在"+roleId);
                return "common[common_alpha]";
            }

            return mainCfg.Model;
        }

        public HeroMainCfg GetNowHero()
        {
            var cfg = ConfigCenter.HeroMainCfgColl.GetDataById(FmtHero);
            return cfg;
        }

        #endregion
    }
}