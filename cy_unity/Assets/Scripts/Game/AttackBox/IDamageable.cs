using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    [Flags]
    public enum DamageEffect
    {
        None,
        /// <summary>
        /// 火系技能伤害
        /// </summary>
        Fire = 1 << 3,
        Thunder = 1 << 4,
        Ice = 1 << 5,
        Dot = 1 << 8
    }
    [Flags]
    public enum DamageStatus
    {
        None,
        Normal = 1,
        Dead = 1 << 1,
        Ignore = 1 << 2,
        Miss = 1 << 5,
    }

    [Serializable]
    public class DamageArgs
    {
        [Header("击退距离")] public float knockBackDis;
        public long damagePoint;
        [HideInInspector] public BaseMonoUnit sender;
        [HideInInspector] public GameObject target;
        public DamageEffect effect;
        public BaseSkill skillIns;
        public long skillId;
        public Skill skillData;
        public bool isCrit;
        public bool beheaded;  // 是否斩杀
        public int dmgCount;   //穿透次数
        public DmgType dmgType;
        public float dmgPercent;  // 百分比伤害
        public WeaponSkill weaponSkill;  //武器技能数据
        public int weaponModelId; //武器模型id，获取合成系数
        public int clothesId;//服饰Id
        public DamageArgs Clone()
        {
            return (DamageArgs)MemberwiseClone();
        }
        public DamageStatus ApplyDamage(IDamageable damageable)
        {
            if (dmgType == DmgType.Ignore)
            {
                return DamageStatus.Normal;
            }
            if (damagePoint < 1) damagePoint = 1;
            // 命中检测
            var monoUnit = damageable as BaseMonoUnit;
            if (monoUnit!=null && skillData!=null && !skillData.IsHit(monoUnit.Data))
            {
                return DamageStatus.Miss;
            }
            TriggersBeforeDamage(damageable);
            var result = damageable.OnDamage(this);
            return result;
        }

        /// <summary>
        /// 伤害之前 触发检测
        /// </summary>
        /// <param name="damageable"></param>
        public void TriggersBeforeDamage(IDamageable damageable)
        {
            var clothesTrigger = skillData?.owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.Kill);
            if (clothesTrigger != null)
            {
                var killProbTrigger = skillData.owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.KillMonsternum, AttributeType.KillProb);
                clothesTrigger.trigger.TryGetValue("prob", out var killProbValue);
                var killProb = killProbValue * GameConst.AttributeDivisor;
                if (killProbTrigger!= null)//每击败20只怪物，秒杀概率+0.5%，最多增加至2.5%
                {
                    var killUpProb = killProbTrigger.attrMgr.Calc(AttributeType.KillProb);
                    var killProUpMax = killProbTrigger.attrMgr.Calc(AttributeType.KillProUpMax);
                    killProb +=skillData.owner.ClothesResource.SecKillCount * killUpProb;
                    if (killProb > killProUpMax) killProb = killProUpMax;
                }
                if(Lodash.RandRangeFloat(0, 1) <= killProb && damageable is BaseMonoUnit tmpUnitMono)
                {
                    if (tmpUnitMono.MonsterData.cfg.MonsterType == 0)
                    {
                        beheaded = true;
                        damagePoint = tmpUnitMono.MonsterData.resource.Hp;
                        return;
                    }
                }
            }
            if (weaponModelId != 306) return;
            var unitMono = damageable as BaseMonoUnit;
            if (unitMono == null) return;
            var unit = unitMono.MonsterData;
            if (unit.cfg.MonsterType != 0) return;
            var trigger = skillData?.GetTriggerByNameAndComplete(AttributeName.HitTargetLittle, AttributeType.Kill);
            if (trigger == null)return;
            trigger.trigger.TryGetValue("prob", out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if (skillData != null) prob += skillData.attrMgr.Calc(AttributeType.KillProb);
            if(Lodash.RandRangeFloat(0, 1) > prob)return;
            beheaded = true;
            damagePoint = unit.resource.Hp;
        }
        public static DamageEffect SkillFactionEffect(SkillFaction faction)
        {
            switch (faction)
            {
                case SkillFaction.Fire:
                    return DamageEffect.Fire;

                case SkillFaction.Thunder:
                    return DamageEffect.Thunder;

                case SkillFaction.Ice:
                    return DamageEffect.Ice;

                default:
                    return DamageEffect.None;
            }
        }
    }

    public interface IDamageable
    {
        DamageStatus OnDamage(DamageArgs args);
        AttributeMgr AttributeMgr { get; }
    }
}