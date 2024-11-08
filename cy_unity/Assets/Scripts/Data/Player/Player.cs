using System.Collections.Generic;
using System.Linq;
using DH.Config;
using DH.UIFramework;
using UnityEngine;

namespace DH.Data
{
    public partial class Player : UnitBase
    {
        [AutoNotify] private bool healFlag;
        public Dictionary<long, float> FireWeaponAtkDic = new();
        public Dictionary<long, bool> AddedWeapon56 = new(); //已经解锁过的模型5和6
        public void Init(long id)
        { 
            Id = id;
            InitAttr();
            InitPlayerSkill();
            InitPlayerTalent();
            InitClothesSkill();
        }
        public void InitAttr()
        {
            var roleAttr = GameDataManager.Instance.RoleAttr;
            attr.Modify(roleAttr);
            // global skills
            var gSkills = GameDataManager.Instance.GlobalSkills;
            if (gSkills != null)
            {
                foreach (var gSkill in gSkills)
                {
                    triggerSkill.AddAllTrigger(gSkill);
                }
            }
            // 上阵英雄属性
            var heroActive = GameDataManager.Instance.HeroActive;
            if (heroActive != null)
            {
                attr.Modify(heroActive.Attr);
                if (heroActive.Skills != null)
                {
                    foreach (var itemSkill in heroActive.Skills)
                    {
                        triggerSkill.AddAllTrigger(itemSkill);
                    }
                }
            }
            // 每日挑战buff属性
            if (GameDataManager.Instance.CurStageType == EStateType.StageTypeChallenge)
            {
                var buffList = DataCenter.dailyFightData.Buff;
                if (buffList != null)
                {
                    foreach (var buffId in buffList)
                    {
                        var buffCfg = ConfigCenter.DailyStageBuffCfgColl.GetDataById(buffId);
                        if (buffCfg == null)continue;
                        var skillId = buffCfg.SkillId;
                        var skillCfg = ConfigCenter.SkillCfgColl.GetDataById(skillId);
                        if (skillCfg == null)continue;
                        if(skillCfg.Target == 0 && skillCfg.Result != null)
                        {
                            attr.Modify(skillCfg.Result);
                        }
                    }
                }
            }
            var hp = PlayerStats.Instance.Hp;
            var maxHp = PlayerStats.Instance.MaxHp;
            resource.Init(hp, maxHp);
        }
        //初始化技能数据
        private void InitPlayerSkill()
        {
            // 所有已上阵武器
            var weaponDic = GameDataManager.Instance.EquipsAttr;
            foreach (var (wid,_) in weaponDic)
            {
                var eSkillIdCfg = ConfigCenter.EquipCfgColl.GetDataById(wid);
                if (eSkillIdCfg == null) continue;
                var skillIdCfg = ConfigCenter.EquipSkillCfgColl.GetDataById(eSkillIdCfg.EquipSkill1);
                if (skillIdCfg == null) continue;
                skill.AddSkill(skillIdCfg.SkillId);

            }
        }
        private void InitPlayerTalent()
        {
            var talents = GameDataManager.Instance.GetChooseTalent(ETalentType.TalentTypeEquip);
            if(talents == null) return;
            foreach (var talent in talents)
            {
                var talentId = talent.Key;
                var talentCount = talent.Value;
                for (int i = 0; i < talentCount; i++)
                {
                    AddTalent(talentId);
                }
            }
        }
        /// <summary>
        /// 初始化服饰技能
        /// </summary>
        private void InitClothesSkill()
        {
            var heroEquips = GameDataManager.Instance.GetHeroEquips();
            if (heroEquips is not { Count: > 0 }) return;
            foreach (var heroEquipItem in heroEquips)
            {
                if (heroEquipItem.Value.Attr != null)
                {
                    clothesSkillAttr.Modify(heroEquipItem.Value.Attr);
                }
                if (heroEquipItem.Value.Skills.Count <= 0) continue;
                foreach (var equipItemSkill in heroEquipItem.Value.Skills)
                {
                    var skillCfg = ConfigCenter.SkillCfgColl.GetDataById(equipItemSkill);
                    if (skillCfg == null) continue;
                    clothesSkillAttr.Modify(skillCfg.Result);
                    HandleClothesSkillTrigger(skillCfg);
                }
            }
            ClothesResource.InitTriggers();
        }
        /// <summary>
        /// 服饰触发器处理
        /// </summary>
        /// <param name="skillCfg"></param>
        private void HandleClothesSkillTrigger(SkillCfg skillCfg)
        {
            if (skillCfg.TriggerGroup != null && skillCfg.AddTrigger is { Count: > 0 })
            {
                if (clothesTriggerSkill.clothTriggerMap.TryGetValue(skillCfg.TriggerGroup, out var tmpTrigger))
                {
                    foreach (var itemAttr in skillCfg.AddTrigger)
                    {
                        tmpTrigger.trigger[itemAttr] += skillCfg.Tigger1[itemAttr];
                    }
                }
                else
                {
                    if(skillCfg.Tigger1 != null)
                    {
                        var skillTrigger = new SkillTrigger(clothesTriggerSkill, skillCfg.Tigger1, skillCfg.Complete1);
                        clothesTriggerSkill.triggerList.Add(skillTrigger);
                        clothesTriggerSkill.clothTriggerMap.Add(skillCfg.TriggerGroup, skillTrigger);
                    }
                }
                if(skillCfg.Tigger2!=null)
                {
                    clothesTriggerSkill.AddTrigger(skillCfg.Tigger2, skillCfg.Complete2);
                }
                if(skillCfg.Tigger3!=null)
                {
                    clothesTriggerSkill.AddTrigger(skillCfg.Tigger3, skillCfg.Complete3);
                }
            }
            else
            {
                clothesTriggerSkill.AddAllTrigger(skillCfg);
            }
        }
        
        public void AddTalent(int talentId)
        {
            var talentCfg = ConfigCenter.TalentCfgColl.GetDataById(talentId);
            if(talentCfg.Type != 1)return;
            var skillId = talentCfg.Param[0];
            var skillCfg = ConfigCenter.SkillCfgColl.GetDataById(skillId);
            if (skillCfg.EquipId <= 0)
            {
                if(skillCfg.Result != null)
                {
                    attr.Modify(skillCfg.Result);
                }
            }
            else
            {
                var skillData = skill.GetSkill(skillCfg.EquipId * 100);
                if (skillData != null)
                {
                    AddWeaponAttr(skillData, skillCfg);
                }
            }
            // 腰带/斗篷/魔戒天赋会影响最大血量
            if (skillCfg.EquipId is 15 or 19 or 20)
            {
                var weaponList = GameDataManager.Instance.BackpackWeaponList.ToList();
                CheckMaxHp(weaponList);
            }
        }
        public void CheckMaxHp(List<BackpackWeaponData> list)
        {
            var maxHp = attr.Calc(AttributeType.Hp);
            if(list != null)
            {
                foreach (var item in list)
                {
                    if (item.EquipId is 5 or 6 or 15 or 19 or 20)
                    {
                        var skillData = skill.GetSkill(item.EquipId * 100);
                        if(skillData == null)continue;
                        var equipHp = skillData.attrMgr.Calc(AttributeType.Hp);
                        var equipHpBonus = skillData.attrMgr.Calc(AttributeType.HpBonus);
                        equipHp *= 1f + equipHpBonus;
                        var equipModelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(item.WeaponId);
                        maxHp += equipHp * equipModelCfg.Compose[0] * GameConst.AttributeDivisor;
                    }
                }
            }
            var clothesHp = clothesTriggerSkill.GetClothesHp();
            resource.MaxHp = Lodash.RoundToInt(maxHp) + clothesHp;
            // 没开始战斗前，满血量
            if (GameDataManager.Instance.Wave == 1 && GameDataManager.Instance.WaveEnd)
            {
                resource.Hp = resource.MaxHp;
            }
        }
        /// <summary>
        /// 5\6武器添加属性，不重复添加
        /// </summary>
        /// <param name="skillData"></param>
        /// <param name="skillCfg"></param>
        public void AddWeaponAttr(Skill skillData, SkillCfg skillCfg)
        {
            if(skillCfg.Their <= 0)
            {
                skillData.attrMgr.Modify(skillCfg.Result);
                skillData.AddAllTrigger(skillCfg);
                return;
            }
            if(AddedWeapon56.ContainsKey(skillCfg.Id))return;
            AddedWeapon56.Add(skillCfg.Id, true);
            skillData.attrMgr.Modify(skillCfg.Result);
            skillData.AddAllTrigger(skillCfg);
        }
        /// <summary>
        /// 获取物理武器合成增益值
        /// </summary>
        /// <returns></returns>
        public int GetPhyEquipMergedGain()
        {
            var coinGain = 0;
            var coinGainTrigger = clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.PhyEquipMerged, AttributeType.SilverCoinsNum);
            if (coinGainTrigger != null) //物理武器合成时，银币+1
            {
                coinGain = Mathf.RoundToInt(coinGainTrigger.attrMgr.Calc(AttributeType.SilverCoinsNum));
            }
            return coinGain;
        }
        /// <summary>
        /// 获取魔法武器合成增益值
        /// </summary>
        /// <returns></returns>
        public int GetMagicEquipMergedGain()
        {
            var coinGain = 0;
            var coinGainTrigger = clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.MagicEquipMerged, AttributeType.SilverCoinsNum);
            if (coinGainTrigger != null) //魔法武器合成时，银币+1
            {
                coinGain = Mathf.RoundToInt(coinGainTrigger.attrMgr.Calc(AttributeType.SilverCoinsNum));
            }
            return coinGain;
        }
        public void AddHp(long hpValue)
        {
            resource.AddHp(hpValue);
            PlayerStats.Instance.Hp = resource.Hp;
        }
        public static Player CreateDefault(bool debug)
        {
            var player = new Player();
            player.Init(1);
            return player;
        }
    }
}