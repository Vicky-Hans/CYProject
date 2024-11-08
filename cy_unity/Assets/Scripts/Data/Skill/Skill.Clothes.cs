using DH.Config;
using UnityEngine;
using UnityEngine.Pool;

namespace DH.Data
{
    public partial class Skill
    {
        /// <summary>
        /// 获取服饰带来的攻击力
        /// </summary>
        /// <returns></returns>
        public int GetClothesAtk()
        {
            var atk = owner.clothesSkillAttr.Calc(AttributeType.Atk);
            var clothesUpAtk = GetClothesUpAtk();
            var atkBonus = GetClothesAtkBonus();
            return Lodash.RoundToInt(atk * (1 + clothesUpAtk + atkBonus));
        }
        /// <summary>
        /// 获取服饰攻击力提升
        /// </summary>
        /// <returns></returns>
        private float GetClothesUpAtk()
        {
            var clothesUpAtk = owner.clothesSkillAttr.Calc(AttributeType.HeroEquipAttackBonus);
            return clothesUpAtk;
        }
        /// <summary>
        /// 服饰技能，复活后提供的攻击力加成/闪避后提升的攻击力加成
        /// </summary>
        /// <returns></returns>
        public float GetClothesAtkBonus()
        {
            var atkUpBonus = 0f;
            atkUpBonus += owner.ClothesResource.ReviveAttackBonus;
            atkUpBonus += owner.ClothesResource.MissAttackBonus;
            return atkUpBonus;
        }
        /// <summary>
        /// 获取服饰带来的血量
        /// </summary>
        /// <returns></returns>
        public int GetClothesHp()
        {
            var hp = owner.clothesSkillAttr.Calc(AttributeType.Hp);
            var hpBonus = owner.clothesSkillAttr.Calc(AttributeType.HeroEquipHpBonus);
            return Lodash.RoundToInt(hp * (1 + hpBonus));
        }
        /// <summary>
        /// 获取服饰伤害提升
        /// </summary>
        /// <returns></returns>
        private float GetClothesUpDmg()
        {
            var upDmg = 0f;
            upDmg += GetPhyClothesUpDmg();
            upDmg += GetMagicClothesUpDmg();
            return upDmg;
        }
        /// <summary>
        /// 物理服饰伤害提升
        /// </summary>
        /// <returns></returns>
        private float GetPhyClothesUpDmg()
        {
            var upDmg = 0f;
            var bagPhyEquipTrigger = owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.BagPhyEquip, AttributeType.UpDmg);
            if (bagPhyEquipTrigger == null) return upDmg;
            var maxUpDmg = bagPhyEquipTrigger.attrMgr.Calc(AttributeType.UpDmgMax);
            var perUpDmg = bagPhyEquipTrigger.attrMgr.Calc(AttributeType.UpDmg);
            // 背包中每有1个物理武器，伤害+1%，上限10%
            var weaponList = GameDataManager.Instance.BackpackWeaponList;
            var phyEquipCount = 0;
            foreach (var weapon in weaponList)
            {
                var tmpEquipCfg = ConfigCenter.EquipCfgColl.GetDataById(weapon.EquipId);
                if(tmpEquipCfg is not { AtkType: (int)EquipAtkType.Physic} ) continue; 
                phyEquipCount++;
            }
            upDmg = phyEquipCount * perUpDmg;
            if (phyEquipCount*perUpDmg > maxUpDmg) upDmg = maxUpDmg;
            return upDmg;
        }
        /// <summary>
        /// 魔法服饰伤害提升
        /// </summary>
        /// <returns></returns>
        private float GetMagicClothesUpDmg()
        {
            var upDmg = 0f;
            var bagMagicEquipTrigger = owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.BagMagicEquip, AttributeType.UpDmg);
            if (bagMagicEquipTrigger == null) return upDmg;
            var maxUpDmg = bagMagicEquipTrigger.attrMgr.Calc(AttributeType.UpDmgMax);
            var perUpDmg = bagMagicEquipTrigger.attrMgr.Calc(AttributeType.UpDmg);
            // 背包中每有1个魔法武器，伤害+1%，上限10%
            var weaponList = GameDataManager.Instance.BackpackWeaponList;
            var magicEquipCount = 0;
            foreach (var weapon in weaponList)
            {
                var tmpEquipCfg = ConfigCenter.EquipCfgColl.GetDataById(weapon.EquipId);
                if(tmpEquipCfg is not { AtkType: (int)EquipAtkType.Magic} ) continue; 
                magicEquipCount++;
            }
            upDmg = magicEquipCount * perUpDmg;
            if (magicEquipCount*perUpDmg > maxUpDmg) upDmg = maxUpDmg;
            return upDmg;
        }
        /// <summary>
        /// 每百分比血量，额外增加的伤害
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        private float GetS910071UpDmg(UnitBase unit)
        {
            if(unit == null) return 0;
            if(owner.ClothesResource.S910071ExtraHp < 0.001f) return 0;
            if (unit.resource.Progress <= owner.ClothesResource.S910071HpMore) return 0f;
            var extraCount = (int)((unit.resource.Progress- owner.ClothesResource.S910071HpMore)/owner.ClothesResource.S910071ExtraHp);
            return extraCount * owner.ClothesResource.S910071UpDmg;
        }
        /// <summary>
        /// 对满血敌人，造成额外伤害
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        private float GetS910073ExtraDmg(UnitBase unit)
        {
            if(unit == null) return 0;
            if (unit.resource.Progress < 0.9999f) return 0f;
            return owner.ClothesResource.S910073ExtraDmg;
        }
        /// <summary>
        /// 获取飞沙阵造成的额外伤害
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        private float GetSandCircleExtraDmg(UnitBase unit)
        {
            if(unit == null || unit.IsDead()) return 0f;
            var extraDmg = 0f;
            var buff = unit.buffMgr.FindBuffById((int)AttributeType.PassSandCircle);
            if (buff == null) return extraDmg;
            extraDmg = buff.value;
            buff.duration = 0;
            return extraDmg;
        }
        /// <summary>
        /// 相邻同名武器，攻击加成
        /// </summary>
        /// <param name="weaponSkill"></param>
        /// <returns></returns>
        private float GetS910075SameEquipAtkBonus(WeaponSkill weaponSkill)
        {
            if (weaponSkill == null) return 0f;
            var weaponUid = weaponSkill.WeaponUid;
            var atkBonus = 0f;
            var wList = ListPool<BackpackWeaponData>.Get();
            var weaponList = GameDataManager.Instance.GetWeaponNearbyList(weaponUid, wList);
            foreach (var w in weaponList)
            {
                if(w.EquipId == weaponSkill.EquipId)
                {
                    atkBonus = owner.clothesSkillAttr.Calc(AttributeType.SameEquipAttackBonus);
                    break;
                }
            }
            ListPool<BackpackWeaponData>.Release(wList);
            return atkBonus;
        }
        /// <summary>
        /// 自身血量高于50%时攻击增加20%
        /// </summary>
        /// <returns></returns>
        private float GetS910081AtkBonus()
        {
             var heroHpValueMoreTrigger = owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.HeroHpValueMore,AttributeType.AttackBonus);
             if (heroHpValueMoreTrigger == null) return 0;
             heroHpValueMoreTrigger.trigger.TryGetValue(AttributeName.HeroHpValueMore,out var haValue);
             if (owner.resource.Progress < haValue*GameConst.AttributeDivisor) return 0f;
             var upAtk = heroHpValueMoreTrigger.attrMgr.Calc(AttributeType.AttackBonus);
             return upAtk;
        }
        /// <summary>
        /// 流血属性最大叠加层数
        /// </summary>
        /// <returns></returns>
        public int GetBleedMaxSuperpose()
        {
            var maxSuperpose = Mathf.RoundToInt(owner.clothesSkillAttr.Calc(AttributeType.BleedNum));
            if (maxSuperpose <= 0) maxSuperpose = 1;
            return maxSuperpose;
        }
        /// <summary>
        /// 中毒属性最大叠加层数
        /// </summary>
        /// <returns></returns>
        public int GetPoisonMaxSuperpose()
        {
            var maxSuperpose = Mathf.RoundToInt(owner.clothesSkillAttr.Calc(AttributeType.PoisonNum));
            if (maxSuperpose <= 0) maxSuperpose = 1;
            return maxSuperpose;
        }
        /// <summary>
        /// 检测怪物击杀触发的条件属性
        /// </summary>
        /// <param name="killNum"></param>
        public void CheckKillMonsterAttrTrigger(int killNum)
        {
            if (killNum <= 0) return;
            var hpRevertTrigger = owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.KillMonsternum, AttributeType.HpRevert);
            var immuneDmgTrigger = owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.KillMonsternum, AttributeType.ImmuneDmgNum);
            var bleedTimeTrigger = owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.KillMonsternum, AttributeType.BleedTime);
            var killProbTrigger = owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.KillMonsternum, AttributeType.KillProb);
            if (hpRevertTrigger!= null)//每击败20只怪物，恢复5%生命
            {
                hpRevertTrigger.trigger.TryGetValue(AttributeName.KillMonsternum, out var killMonsterValue);
                if (killMonsterValue > 0 && killNum%killMonsterValue == 0 && owner.resource.Progress < 0.9999f)//玩家为残血
                {
                    var hpRevert = hpRevertTrigger.attrMgr.Calc(AttributeType.HpRevert);
                    var newHp = Lodash.RoundToInt(owner.resource.Hp * (1 + hpRevert));
                    if (newHp < owner.resource.MaxHp)
                    {
                        owner.resource.Hp = newHp;
                    }
                    else
                    {
                        owner.resource.Hp = owner.resource.MaxHp;
                    }
                }
            }
            if (immuneDmgTrigger!= null)//每击败20只怪物，获得1次免疫伤害效果
            {
                immuneDmgTrigger.trigger.TryGetValue(AttributeName.KillMonsternum, out var killMonsterValue);
                if (killMonsterValue > 0 && killNum%killMonsterValue == 0)
                {
                    var immuneDmgNum = immuneDmgTrigger.attrMgr.Calc(AttributeType.ImmuneDmgNum);
                    owner.ClothesResource.ImmuneEveryCount += Lodash.RoundToInt(immuneDmgNum);
                }
            }
            if (bleedTimeTrigger!= null)//每击败20只怪物，流血效果持续时长+0.2秒，最多增加1秒
            {
                bleedTimeTrigger.trigger.TryGetValue(AttributeName.KillMonsternum, out var killMonsterValue);
                if (killMonsterValue > 0 && killNum%killMonsterValue == 0)
                {
                    var bleedTime = bleedTimeTrigger.attrMgr.Calc(AttributeType.BleedTime)*GameConst.TimeDivisor;
                    var bleedTimeUpMax = bleedTimeTrigger.attrMgr.Calc(AttributeType.BleedTimeUpMax)*GameConst.TimeDivisor;
                    var maxTimes = Mathf.RoundToInt(bleedTimeUpMax / bleedTime);
                    if (owner.ClothesResource.BleedTimeSuperposeCount < maxTimes)
                    {
                        owner.ClothesResource.BleedTimeSuperposeCount += 1;
                        owner.ClothesResource.BleedSuperposeTime = bleedTime;
                    }
                }
            }
            if (killProbTrigger!= null)//每击败20只怪物，秒杀概率+0.5%，最多增加至2.5%
            {
                killProbTrigger.trigger.TryGetValue(AttributeName.KillMonsternum, out var killMonsterValue);
                if (killMonsterValue <= 0 || killNum % killMonsterValue != 0) return;
                var killProb = killProbTrigger.attrMgr.Calc(AttributeType.KillProb);
                var killProUpMax = killProbTrigger.attrMgr.Calc(AttributeType.KillProUpMax);
                if (owner.ClothesResource.SecKillCount < Mathf.RoundToInt(killProUpMax/killProb))
                {
                    owner.ClothesResource.SecKillCount += 1;
                }
            }
        }
    }
}