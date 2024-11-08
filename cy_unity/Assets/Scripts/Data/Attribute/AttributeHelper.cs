using System;
using DH.Config;

namespace DH.Data
{
    [Serializable]
    public class AttributeInit
    {
        public long Lv;
        public long Hp;
        public long Atk;
        public long Def;
        public long Aspd;
        public long Speed;
    }

    public enum SkillFaction
    {
        None = 1,
        Fire = 2,
        Thunder = 3,
        Ice = 4,
        Nature = 5
    }

    public static class AttributeHelper
    {
        /// <summary>
        /// 暴击率最大80%
        /// </summary>
        public const long MaxCritRate = 800;

        /// <summary>
        /// 最大减少CD
        /// </summary>
        public const long MaxCdDecrease = 2000;

        public static long CalculateDamagePoint(UnitBase unitBase, SkillFaction skillFaction)
        {
            var atk = unitBase.attr.Calc(AttributeType.Atk);
            var skillAtkRatio = unitBase.attr.Calc(AttributeType.Atk);
            var skillFactionAttr = (AttributeType)((long)skillFaction * 100 + (long)AttributeType.Atk);
            var factionAtkRatio = skillFaction == SkillFaction.None ? 0 : unitBase.attr.Calc(skillFactionAttr);

            return (long)(atk * (1 + (factionAtkRatio + skillAtkRatio) * 0.001f));
        }

        public static int SkillMulticast(UnitBase unitBase)
        {
            // long doubleCast = unitBase.attr.Calc(AttributeType.MultiCast);
            // long tripleCast = unitBase.attr.Calc(AttributeType.TripleCast);
            // long quatraCast = unitBase.attr.Calc(AttributeType.QuatraCast);
            // if (Common.CheckRate(quatraCast))
            // {
            //     return 4;
            // }
            //
            // if (Common.CheckRate(tripleCast))
            // {
            //     return 3;
            // }
            //
            // if (Common.CheckRate(doubleCast))
            // {
            //     return 2;
            // }

            return 1;
        }

        /// <summary>
        /// 暴击率数值范围 0-1000
        /// </summary>
        /// <param name="damagePoint"></param>
        /// <param name="unitBase"></param>
        /// <returns></returns>
        public static long CalculateCritDamage(long damagePoint, AttributeMgr attr, out bool crit)
        {
            var p = attr.Calc(AttributeType.CritRate);
            if (p > MaxCritRate) p = MaxCritRate;

            long current = Lodash.Random.Next(0, 1000);
            if (current > p)
            {
                crit = false;
                return damagePoint;
            }

            var critDamageRatio = attr.Calc(AttributeType.CritDmg);
            damagePoint = (long)(damagePoint * (1.5 + critDamageRatio * 0.001));
            crit = true;
            return damagePoint;
        }

        /// <summary>
        /// 致死攻击
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        public static bool CalculateDeadStrike(AttributeMgr attr)
        {
            return true;
        }

        public static long CalculateRange(long range, long increaseRatio, AttributeMgr attr, SkillFaction skillFaction)
        {
            
            return 0;
        }

        public static float SkillAttackCd(Skill skill, long aSpd, AttributeMgr attributeMgr, SkillFaction skillFaction)
        {
            // long cd = skill.Cd;
            // var ratio = attributeMgr.Calc(AttributeType.Cd);
            // var attrType = (AttributeType)((long)skillFaction * 100 + (long)AttributeType.Cd);
            // var factionCdRatio = skillFaction == SkillFaction.None ? 0 : attributeMgr.Calc(attrType);
            // var cdTime = cd * 1000 / (1000.0f + ratio + factionCdRatio) * 0.001f;
            // return cdTime;
            return 0;
        }

        public static AttributeValueType GetValueType(string name)
        {
            var collection = ConfigCenter.AttributesCfgColl;
            var cfg = collection.GetDataByName(name);
            if (cfg == null) throw new InvalidOperationException($"Invalid attr type {name}");
            return (AttributeValueType)cfg.Type;
        }

        public static AttributeType GetAttributeTypeByName(string name)
        {
            // var collection = ConfigCenter.AttributesCfgColl;
            // var cfg = collection.GetDataByName(name);
            // if (cfg == null) throw new InvalidOperationException($"Invalid attr type {name}");
            // return (AttributeType)cfg.Id;
            return (AttributeType)AttributeName.AttrNameToId[name];
        }

        public static string GetAttrName(AttributeType type)
        {
            // var collection = ConfigCenter.AttributesCfgColl;
            // var cfg = collection.GetDataById((int)type);
            // if (cfg == null) throw new InvalidOperationException($"Invalid attr type {type}");
            // return cfg.Name;
            return AttributeName.AttrIdToName[(int)type];
        }

        public static float GetAttrValue(string name, long value)
        {
            float result = value;
            var type = GetValueType(name);
            if (type == AttributeValueType.ValMul)
            {
                result = result * GameConst.AttributeDivisor;
            }

            return result;
        }
    }
}