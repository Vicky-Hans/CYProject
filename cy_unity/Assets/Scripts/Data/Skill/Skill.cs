using System.Collections.Generic;
using DH.Config;
using DH.UIFramework.Observables;
using DHFramework;
using UnityEngine;
using UnityEngine.Pool;

namespace DH.Data
{
    public struct SkillHurtResult
    {
        public float dmg;
        public SkillHurtType hurtType;
    }

    public enum ESkillOwnerType
    {
        Ship,
    }
    
    public partial class Skill : ObservableObject
    {
        public int id;  // 100、200、300 ...
        private long lv;
        private SkillCfg cfg;
        public EquipCfg equipCfg { get; set; }
        public UnitBase owner;
        public AttributeMgr attrMgr;
        public List<SkillTrigger> triggerList;
        public Dictionary<string, SkillTrigger> clothTriggerMap;
        public long SkillNum { get; set; }  //技能发射次数
        public long KillNum { get; set; }  //技能击杀数
        public ESkillOwnerType SkillOwnerType { get; set; }
        public SkillCfg Cfg
        {
            get => cfg;
            private set => Set(ref cfg, value);
        }

        public long Lv
        {
            get => lv;
            set
            {
                if (!Set(ref lv, value)) return;
            }
        }
        // 密林模式 攻击距离系数
        private float forestRangeFactor = 1f;
        public Skill(int id, long lv, UnitBase owner)
        {
            SkillOwnerType = ESkillOwnerType.Ship;
            this.owner = owner;
            cfg = GetSkillCfg(id);
            if(cfg != null)  equipCfg = ConfigCenter.EquipCfgColl.GetDataById(cfg.EquipId);
            this.id = id;
            Lv = lv;
            attrMgr = new AttributeMgr(owner);
            triggerList = new List<SkillTrigger>();
            clothTriggerMap = new Dictionary<string, SkillTrigger>();
            if(Cfg!=null) AddWeaponTriggers();
            var rangeFactorCfg =
                ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_31);
            if(rangeFactorCfg != null)
            {
                forestRangeFactor = rangeFactorCfg.Content[0] * GameConst.AttributeDivisor;
            }
        }
        private void AddWeaponTriggers()
        {
            if(owner.IsMonster)
            {
                if(Cfg.Result != null) AddAttr(Cfg.Result);
                AddAllTrigger(Cfg);
                return;
            }
            var weaponAttr = GameDataManager.Instance.GetSkillProperty(Cfg.EquipId);
            AddAttr(weaponAttr);
            MergeWeaponTrigger(Cfg.EquipId);
        }

        private void MergeWeaponTrigger(int equipId)
        {
            var equipDataMap = GameDataManager.Instance.EquipsAttr;
            if(!equipDataMap.ContainsKey(equipId))return;
            var equipData = equipDataMap[equipId];
            if(equipData.Skills is not { Count: > 0 })return;
            foreach (var skillId in equipData.Skills)
            {
                var skillCfg = ConfigCenter.SkillCfgColl.GetDataById(skillId);
                AddAllTrigger(skillCfg);
            }
        }
        
        public void AddTrigger(Dictionary<string, int> cond, Dictionary<string, int> complete)
        {
            var trigger = new SkillTrigger(this, cond, complete);
            triggerList.Add(trigger);
        }

        public void AddAllTrigger(int skillId)
        {
            var skillCfg = ConfigCenter.SkillCfgColl.GetDataById(skillId);
            if (skillCfg == null) return;
            AddAllTrigger(skillCfg);
        }
        
        public void AddAllTrigger(SkillCfg skillCfg)
        {
            if(skillCfg == null) return;
            if (skillCfg.Tigger1 != null)
            {
                AddTrigger(skillCfg.Tigger1, skillCfg.Complete1);
            }
            if (skillCfg.Tigger2 != null)
            {
                AddTrigger(skillCfg.Tigger2, skillCfg.Complete2);
            }
            if (skillCfg.Tigger3 != null)
            {
                AddTrigger(skillCfg.Tigger3, skillCfg.Complete3);
            }
        }

        private SkillCfg GetSkillCfg(int skillId)
        {
            return ConfigCenter.SkillCfgColl.GetDataById(skillId);
        }
        private void AddAttr(List<DH.Config.Attribute> list)
        {
            attrMgr.Modify(list);
        }

        public void AddAttr(Dictionary<string, int> dic)
        {
            attrMgr.Modify(dic);
        }
        public float AttackInterval(WeaponSkill weaponSkill)
        {
            var cd = attrMgr.Calc(AttributeType.Cd);
            var upAtkSpd = attrMgr.Calc(AttributeType.UpAtkSpd);
            upAtkSpd += owner.attr.Calc(AttributeType.UpAtkSpd);
            if (!owner.IsMonster)
            {
                var atkSpdGp = owner.buffMgr.GetBuffsMaxValue((int)AttributeType.AtkSpdGp);
                upAtkSpd += atkSpdGp;
            }
            if (weaponSkill != null)
            {
                upAtkSpd += GetAboutPhAtkSpd(weaponSkill);
                upAtkSpd += Get1205UpAtkSpd(weaponSkill); // 1205-攻击速度提升
                upAtkSpd += Get1105UpAtkSpd(weaponSkill); // 1105-攻击速度提升II
                upAtkSpd += Get2507UpAtkSpd(weaponSkill); // 2507-老魔杖攻击速度提升
                if (weaponSkill.equipCfg?.Id == 16 && !owner.IsMonster)//时间沙漏
                {
                    upAtkSpd += owner.attr.Calc(AttributeType.UpAtkSpd);
                }
            }
            var downAtkSpd = attrMgr.Calc(AttributeType.DownAtkSpd);
            downAtkSpd += owner.attr.Calc(AttributeType.DownAtkSpd);
            if (owner is Player && GameDataManager.Instance.IsSecretFightData()) //密林模式下的攻速药水：拾取后1分钟内所有装备攻速+50%
            {
                var circleBuff = owner.buffMgr.FindBuffById((int)AttributeType.UpAtkSpd);
                if (circleBuff != null) upAtkSpd += circleBuff.value;
            }
            return cd / (1 + upAtkSpd - downAtkSpd) * GameConst.TimeDivisor;
        }

        public float Duration()
        {
            return 0f;
        }

        public int Pierce(WeaponSkill weaponSkill = null)
        {
            var pierce = Lodash.RoundToInt(attrMgr.Calc(AttributeType.Pierce));
            pierce += Lodash.RoundToInt(owner.attr.Calc(AttributeType.Pierce));
            if (weaponSkill is { WeaponModelId: 205 })
            {
                pierce += Lodash.RoundToInt(attrMgr.Calc(AttributeType.WindPierce));
            }
            pierce += GetClothesAttrPierce();
            return pierce;
        }
        /// <summary>
        /// 获取服饰属性的穿透值
        /// </summary>
        private int GetClothesAttrPierce(WeaponSkill weaponSkill = null)
        {
            if (weaponSkill?.owner == null) return 0;
            var pierce = 0;
            var tmpTrigger = weaponSkill.owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.Reverting, AttributeType.ShockWaveDmg);
            if (tmpTrigger!= null)
            {
                pierce = Mathf.RoundToInt(tmpTrigger.attrMgr.Calc(AttributeType.Pierce));
            }
            else
            {
                tmpTrigger = weaponSkill.owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.HeroUnderAttack, AttributeType.ShockWaveDmg);
                if (tmpTrigger != null) pierce = Mathf.RoundToInt(tmpTrigger.attrMgr.Calc(AttributeType.Pierce));
            }
            return pierce;
        }
        public float BulletSpeed()
        {
            var bulletSpeed = attrMgr.Calc(AttributeType.BulletSpeed);
            if (GameDataManager.Instance.IsSecretFightData())
            {
                DefinesCfg spdFactorCfg = null;
                if (owner.IsMonster)
                {
                    spdFactorCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_15);
                }
                else
                {
                    spdFactorCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_14);
                }
                if(spdFactorCfg != null)
                {
                    bulletSpeed *= spdFactorCfg.Content[0] * GameConst.AttributeDivisor;
                }
            }
            return bulletSpeed;
        }

        public float BulletSize()
        {
            var bulletSize = attrMgr.Calc(AttributeType.BulletSize);
            var upBulletSize = attrMgr.Calc(AttributeType.UpBulletSize);
            bulletSize *= (1 + upBulletSize);
            return bulletSize;
        }
        
        public float Range()
        {
            var range = attrMgr.Calc(AttributeType.Range);
            var upRange = attrMgr.Calc(AttributeType.UpRange);
            range *= (1 + upRange);
            if (GameDataManager.Instance.IsSecretFightData() && (owner is Player))
            {
                range *= forestRangeFactor;
            }
            return range;
        }

        public SkillHurtResult CalcDmg(UnitBase unit, WeaponSkill weaponSkill, DmgType dmgType = DmgType.Normal, bool canIgnoreDownDmg = false)
        {
            var weaponModelId = 0;
            if (weaponSkill != null) weaponModelId = weaponSkill.WeaponModelId;
            var result = new SkillHurtResult();
            // 基础攻击力
            var dmg = CalcWeaponAtk(weaponModelId, weaponSkill, dmgType);
            // 暴击
            var critDmg = 1f;
            var isCrit = IsCrit(unit, dmgType, weaponSkill);
            if (isCrit)
            {
                critDmg = GetCritDmg(weaponSkill, unit);
                result.hurtType = SkillHurtType.Crit;
            }
            dmg *= critDmg;
            var upDmg = GetUpDmg(unit);
            upDmg += GetUpDaggerDmg(unit);
            upDmg += GetPhysicUpDmg(unit);
            upDmg += GetMagicUpDmg(unit);
            upDmg += GetEveryEquipUpDmg();
            upDmg += GetAdJoinUpDmg(weaponSkill);
            upDmg += GetDarkWeaponUpDmg(weaponSkill);
            upDmg += GetAroundUpDmg(weaponSkill);
            upDmg += GetClothesUpDmg();//服饰伤害提升
            upDmg += GetS910071UpDmg(unit);
            upDmg += GetS910081AtkBonus();
            dmg *= (1 + upDmg);
            // extraDmg
            var extraDmg = 0f;
            if (weaponModelId == 105)
            {
                extraDmg += GetFrozenOrDecExtraDmg(unit);
            }
            else if(weaponModelId == 106)
            {
                extraDmg += GetFiringExtraDmg(unit);
            }
            else if(weaponModelId == 1106)
            {
                extraDmg += Get1106ExtraDmgOnFrozenOrDec(unit);
            }
            extraDmg += GetBossExtraDmg(unit);
            extraDmg += GetHurtExtraDmg(unit);
            extraDmg += GetExtraDmgOnElectrify(unit,weaponSkill);
            extraDmg += GetS910073ExtraDmg(unit);
            extraDmg += GetSandCircleExtraDmg(unit);
            var downDmg = 0f;
            downDmg += GetDownDmgFromBleedBuff(unit,weaponSkill);
            dmg *= (1 + extraDmg-downDmg);
            if (dmg < 1) dmg = 1;
            dmg += GetExtraDmgFromBleedBuff(unit);
            result.dmg = dmg;
            return result;
        }
        /// <summary>
        /// 获取武器模型加成
        /// </summary>
        /// <param name="weaponModelId"></param>
        /// <returns></returns>
        private float GetWeaponModelFactor(int weaponModelId)
        {
            var factor = 1f;
            var wCfg = ConfigCenter.EquipModelCfgColl.GetDataById(weaponModelId);
            if (wCfg != null && wCfg.Compose.Count > 0)
                factor = wCfg.Compose[0] * GameConst.AttributeDivisor;
            return factor;
        }
        
        public float CalcAtk()
        {
            return attrMgr.Calc(AttributeType.Atk);
        }
        
        public float CalcWeaponAtk(int weaponModelId, WeaponSkill weaponSkill = null, DmgType dmgType = DmgType.Normal)
        {
            var atk = CalcAtk();
            var wmFactor = GetWeaponModelFactor(weaponModelId);
            var atkBonus = attrMgr.Calc(AttributeType.AttackBonus);
            atkBonus += owner.attr.Calc(AttributeType.AttackBonus);
            atkBonus += GetClothesAtkBonus();
            var fireWeaponAtk = 0f;
            if(weaponSkill != null)
            {
                atkBonus += GetS910075SameEquipAtkBonus(weaponSkill);
                if (!owner.IsMonster)
                {
                    ((Player)owner).FireWeaponAtkDic.TryGetValue(weaponSkill.WeaponUid, out fireWeaponAtk);
                }
                // hero 300 关系攻击提升
                if (weaponSkill.GetAttrType() == EquipAttrType.Light)
                {
                    atkBonus += owner.attr.Calc(AttributeType.LightAtkBonus);
                }
                // 每日挑战，物理 魔法 攻击提升
                if (weaponSkill.GetAtkType() == EquipAtkType.Physic)
                {
                    atkBonus += owner.attr.Calc(AttributeType.PhyAtkBonus);
                }
                else if (weaponSkill.GetAtkType() == EquipAtkType.Magic)
                {
                    atkBonus += owner.attr.Calc(AttributeType.MagAtkBonus);
                }
            }
            if (dmgType == DmgType.Range)
            {
                var rangeDmg = attrMgr.Calc(AttributeType.RangeDmg);
                atk *= rangeDmg;
            }
            var roleAtk = GetRoleAtk();
            var clothesAtk = GetClothesAtk();
            return atk * wmFactor * (1 + atkBonus + fireWeaponAtk) + roleAtk + clothesAtk;
        }
        
        /// <summary>
        /// 获取英雄攻击力，角色+服饰
        /// </summary>
        /// <returns></returns>
        public float GetHeroAtk()
        {
            var roleAtk = owner.attr.Calc(AttributeType.Atk);
            var clothesAtk = GetClothesAtk();
            return roleAtk + clothesAtk;
        }

        /// <summary>
        /// 角色攻击力
        /// </summary>
        /// <returns></returns>
        public float GetRoleAtk()
        {
            var roleAtk = owner.attr.Calc(AttributeType.Atk);
            var atkBonus = GetClothesAtkBonus();
            return roleAtk * (1 + atkBonus);
        }
        
        public bool IsHit(UnitBase unit)
        {
            if (unit is Player)
            {
                return true;
            }
            var hitRate = attrMgr.Calc(AttributeType.Hit);
            hitRate += owner.attr.Calc(AttributeType.Hit);
            var eMiss = unit.attr.Calc(AttributeType.Miss);
            return Lodash.RandRangeFloat(0, 1) < hitRate - eMiss;
        }

        private bool IsCrit(UnitBase unit, DmgType dmgType = DmgType.Normal, WeaponSkill weaponSkill = null)
        {
            var mustCritNum = MustCritSkillNum();
            if (mustCritNum > 0 && SkillNum % mustCritNum == 0) return true;
            var critRate = attrMgr.Calc(AttributeType.CritRate);
            critRate += owner.attr.Calc(AttributeType.CritRate);
            var eResRate = unit.attr.Calc(AttributeType.ResRate);
            // 12, 对重伤的目标 暴击率提升
            if (unit.buffMgr.GetBuffCountById((int)AttributeType.Hurt) > 0)
            {
                var trigger = GetTriggerByNameAndComplete(AttributeName.HitHurt, AttributeType.CritRate);
                if (trigger != null)
                {
                    var triggerCritRate = trigger.attrMgr.Calc(AttributeType.CritRate);
                    critRate += triggerCritRate;
                }
            }
            // 风属性暴击
            if (weaponSkill?.GetAttrType() == EquipAttrType.Wind)
            {
                critRate += owner.attr.Calc(AttributeType.WindCritRate);
            }
            // 1105 每有一件土元素，暴击率增加
            if(weaponSkill?.WeaponModelId == 1105)
            {
                var landCircleCritRate = attrMgr.Calc(AttributeType.LandCircleCritRate);
                var equipCount = GetEquipAttrCount((int)EquipAttrType.Earth);
                critRate += landCircleCritRate * equipCount;
            }
            //雷神法杖
            if(weaponSkill?.WeaponModelId == 1406)
            {
                critRate += weaponSkill.SkillData.attrMgr.Calc(AttributeType.ThunderWandCrit);
            }
            critRate += GetTheRingCritDmg(weaponSkill);//魔戒提升暴击率
            if (owner is Player && GameDataManager.Instance.IsSecretFightData() //密林模式下的暴击药水buff：拾取后1分钟内所有攻击必定暴击
                && owner.buffMgr.FindBuffById((int)AttributeType.EnableCircle) != null)
            {
                return true;
            }
            return Lodash.RandRangeFloat(0, 1) < critRate - eResRate;
        }

        public int MustCritSkillNum()
        {
            var trigger = GetTriggerByNameAndComplete(AttributeName.SkillNum, AttributeType.MustCrit);
            if (trigger == null) return 0;
            if (!trigger.trigger.TryGetValue(AttributeName.SkillNum, out var skilNum))
            {
                return 0;
            }
            var num = Lodash.RoundToInt(skilNum);
            var decSkillNum = Lodash.RoundToInt(attrMgr.Calc(AttributeType.DecSkillNum));
            return num - decSkillNum;
        }
        /// <summary>
        /// 获取暴击伤害
        /// </summary>
        /// <param name="weaponSkill"></param>
        /// <returns></returns>
        private float GetCritDmg(WeaponSkill weaponSkill, UnitBase unit)
        {
            var critDmg = attrMgr.Calc(AttributeType.CritDmg);
            critDmg += owner.attr.Calc(AttributeType.CritDmg);
            critDmg += owner.buffMgr.GetBuffsValue((int)AttributeType.CritDmg);
            if (weaponSkill is { WeaponModelId: 1206 })//暗斧爆伤
            {
                critDmg += attrMgr.Calc(AttributeType.DarkAxeCritDmg);
            }
            var upCritDmg = attrMgr.Calc(AttributeType.UpCritDmg);
            upCritDmg += owner.attr.Calc(AttributeType.UpCritDmg);
            var downCritDmg = 0f;
            if(unit != null)
            {
                downCritDmg = unit.attr.Calc(AttributeType.DownCritDmg);
            }
            return critDmg * (1 + upCritDmg - downCritDmg);
        }
        public float GetUpDmg(UnitBase unit)
        {
            var upDmg = attrMgr.Calc(AttributeType.UpDmg);
            upDmg += owner.attr.Calc(AttributeType.UpDmg);
            upDmg -= unit.attr.Calc(AttributeType.DownDmg);
            return upDmg;
        }
        /// <summary>
        /// 火之剑106对灼烧目标造成额外伤害
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public float GetFiringExtraDmg(UnitBase unit)
        {
            if (unit.FiringCount <= 0) return 0f;
            var trigger = GetTriggerByNameAndComplete(AttributeName.HitFire, AttributeType.ExtraDmg);
            return trigger == null ? 0f : trigger.attrMgr.Calc(AttributeType.ExtraDmg);
        }
        
        /// <summary>
        /// 水之剑105对减速或冰冻目标造成额外伤害
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public float GetFrozenOrDecExtraDmg(UnitBase unit)
        {
            if(unit.FrozenCount <= 0 && unit.DecelerateCount <= 0) return 0f;
            var trigger = GetTriggerByNameAndComplete(AttributeName.HitFrozenOrDec, AttributeType.ExtraDmg);
            return trigger == null ? 0f : trigger.attrMgr.Calc(AttributeType.ExtraDmg);
        }

        /// <summary>
        /// 水之回旋镖对冰冻减速目标伤害+50%,对冻结目标伤害+100%
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public float Get1106ExtraDmgOnFrozenOrDec(UnitBase unit)
        {
            var extraDmg = 0f;
            if(unit.FrozenCount > 0 )
            {
                var trigger = GetTriggerByNameAndComplete(AttributeName.WaterCircleHitFrozen, AttributeType.ExtraDmg);
                if (trigger != null)
                {
                    extraDmg += trigger.attrMgr.Calc(AttributeType.ExtraDmg);
                }
            }

            if (unit.DecelerateCount > 0)
            {
                var trigger = GetTriggerByNameAndComplete(AttributeName.WaterCircleHitSlow, AttributeType.ExtraDmg);
                if (trigger != null)
                {
                    extraDmg += trigger.attrMgr.Calc(AttributeType.ExtraDmg);                 
                }
            }
            return extraDmg;
        }
        
        /// <summary>
        /// 200 匕首对同一目标造成伤害提升
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public float GetUpDaggerDmg(UnitBase unit)
        {
            if(unit == null || unit.IsDead()) return 0f;
            if (id != 200) return 0f;
            var trigger = GetTriggerByNameAndComplete(AttributeName.HitSameTarget, AttributeType.UpDaggerDmg);
            if (trigger == null) return 0f;
            var overlayNum = Lodash.RoundToInt(attrMgr.Calc(AttributeType.OverlayNum));
            var buffs = unit.buffMgr.FindBuffsById((int)AttributeType.UpDaggerDmg);
            if(buffs == null || buffs.Count == 0) return 0f;
            var count = buffs.Count;
            count = count > overlayNum ? overlayNum : count;
            var upDaggerDmg = trigger.attrMgr.Calc(AttributeType.UpDaggerDmg);
            return upDaggerDmg * count;
        }

        /// <summary>
        /// 对boss造成额外伤害
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public float GetBossExtraDmg(UnitBase unit)
        {
            if(unit == null || unit.IsDead()) return 0f;
            var monsterData = (unit as Monster);
            if (monsterData?.cfg.MonsterType != 2) return 0f;
            return owner.attr.Calc(AttributeType.BossExtraDmg);
        }

        /// <summary>
        /// 受到物理伤害提升
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public float GetPhysicUpDmg(UnitBase unit)
        {
            if (equipCfg.AtkType != (int)EquipAtkType.Physic) return 0;
            if(unit == null || unit.IsDead()) return 0f;
            var upDmg = unit.buffMgr.GetBuffsValue((int)AttributeType.Hurt);
            upDmg += owner.attr.Calc(AttributeType.UpPhAtk);
            upDmg += unit.attr.Calc(AttributeType.UpUnderPhAtk);
            upDmg -= unit.attr.Calc(AttributeType.DownUnderPhAtk);
            return upDmg;
        }
        
        /// <summary>
        /// 受到魔法伤害提升
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public float GetMagicUpDmg(UnitBase unit)
        {
            if (equipCfg.AtkType != (int)EquipAtkType.Magic) return 0;
            if(unit == null || unit.IsDead()) return 0f;
            var upDmg = owner.attr.Calc(AttributeType.UpMagicAtk);
            upDmg -= unit.attr.Calc(AttributeType.DownUnderMagicAtk);
            return upDmg;
        }

        /// <summary>
        /// 405 对重伤目标造成额外伤害
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="weaponSkill"></param>
        /// <returns></returns>
        public float GetHurtExtraDmg(UnitBase unit)
        {
            if (unit == null || unit.IsDead()) return 0f;
            if (unit.buffMgr.GetBuffCountById((int)AttributeType.Hurt) <= 0) return 0f;
            var trigger = GetTriggerByNameAndComplete(AttributeName.HitHurt, AttributeType.ExtraDmg);
            return trigger == null ? 0f : trigger.attrMgr.Calc(AttributeType.ExtraDmg);
        }

        /// <summary>
        /// 背包里每有一件装备，攻击力提升
        /// </summary>
        /// <returns></returns>
        public float GetEveryEquipUpDmg()
        {
            var everyEquipUpDmg = owner.attr.Calc(AttributeType.EveryEquipUpDmg);
            if (everyEquipUpDmg <= 0) return 0f;
            return GameDataManager.Instance.BackpackWeaponList.Count * everyEquipUpDmg;
        }

        /// <summary>
        /// 与魔法帽相邻的魔法武器攻击提升
        /// </summary>
        /// <param name="weaponSkill"></param>
        /// <returns></returns>
        public float GetAdJoinUpDmg(WeaponSkill weaponSkill)
        {
            if (GameDataManager.Instance.CurStageType == EStateType.StageTypeSecret) return 0f;
            if (weaponSkill == null) return 0f;
            if (weaponSkill.GetAtkType() != EquipAtkType.Magic) return 0f;
            var weaponUid = weaponSkill.WeaponUid;
            var wList = ListPool<BackpackWeaponData>.Get();
            GameDataManager.Instance.GetWeaponNearbyList(weaponUid, wList);
            var upDmg = 0f;
            foreach (var w in wList)
            {
                if(w.EquipId != 5)continue;
                var tmpSkill = owner.skill.GetSkill(w.EquipId * 100);
                if(tmpSkill == null) continue;
                upDmg += tmpSkill.attrMgr.Calc(AttributeType.UpAdjoinAtk);
            }
            ListPool<BackpackWeaponData>.Release(wList);
            return upDmg;
        }

        /// <summary>
        /// 魔法攻击对有磁暴buff的目标造成额外伤害
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="weaponSkill"></param>
        /// <returns></returns>
        public float GetExtraDmgOnElectrify(UnitBase unit, WeaponSkill weaponSkill)
        {
            if(weaponSkill == null) return 0f;
            if(weaponSkill.GetAtkType() != EquipAtkType.Magic) return 0f;
            if (unit == null || unit.IsDead()) return 0f;
            if (unit.buffMgr.GetBuffCountById((int)AttributeType.Electrify) <= 0) return 0f;
            var extraDmg = unit.buffMgr.GetBuffsMaxValue((int)AttributeType.Electrify);
            return extraDmg;
        }

        /// <summary>
        /// 所有暗属性的武器伤害增幅额外
        /// </summary>
        /// <param name="weaponSkill"></param>
        /// <returns></returns>
        public float GetDarkWeaponUpDmg(WeaponSkill weaponSkill)
        {
            if (weaponSkill == null) return 0f;
            if(weaponSkill.GetAttrType() != EquipAttrType.Dark) return 0f;
            var upDmg = 0f;
            var weaponList = GameDataManager.Instance.BackpackWeaponList;
            foreach (var w in weaponList)
            {
                if(w.WeaponId != 506)continue;
                var tmpSkill = owner.skill.GetSkill(w.EquipId * 100);
                if(tmpSkill == null) continue;
                upDmg += tmpSkill.attrMgr.Calc(AttributeType.UpDarkDmg);
            }
            return upDmg;
        }
        /// <summary>
        /// 获取带有流血buff造成得额外伤害
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public float GetExtraDmgFromBleedBuff(UnitBase unit)
        {
            if (unit == null || unit.IsDead()) return 0f;
            if (unit.buffMgr.GetBuffCountById((int)AttributeType.Bleed) <= 0) return 0f;
            var extraDmg = 0f;
            var bleedBuffs = unit.buffMgr.FindBuffsById((int)AttributeType.Bleed);
            if (bleedBuffs is not { Count: > 0 }) return extraDmg;
            foreach (var buffItem in bleedBuffs)
            {
                if (buffItem.clothesId <= 0) continue;
                extraDmg = buffItem.value;
                break;
            }
            return extraDmg;
        }
        /// <summary>
        /// 有流血效果的怪物造成的伤害-10%
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="weaponSkill"></param>
        /// <returns></returns>
        public float GetDownDmgFromBleedBuff(UnitBase unit,WeaponSkill weaponSkill)
        {
            if (weaponSkill?.owner == null) return 0f;
            var bleedBuffs = unit.buffMgr.FindBuffsById((int)AttributeType.Bleed);
            if (bleedBuffs is not { Count: > 0 }) return 0;
            var downDmg = 0f;
            var downTrigger = weaponSkill?.owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.Bleed,AttributeType.DownDmg);
            if (downTrigger == null) return downDmg;
            downDmg = (float)weaponSkill?.owner.clothesTriggerSkill.attrMgr.Calc(AttributeType.DownDmg);
            return downDmg;
        }
        /// <summary>
        /// 帽子506周围的武器伤害提升
        /// </summary>
        /// <param name="weaponSkill"></param>
        /// <returns></returns>
        public float GetAroundUpDmg(WeaponSkill weaponSkill)
        {
            if (GameDataManager.Instance.CurStageType == EStateType.StageTypeSecret)
            {
                return 0f;
            }
            if (weaponSkill == null) return 0f;
            var weaponUid = weaponSkill.WeaponUid;
            var upDmg = 0f;
            var wList = ListPool<BackpackWeaponData>.Get();
            var weaponList = GameDataManager.Instance.GetWeaponNearbyList(weaponUid, wList);
            foreach (var w in weaponList)
            {
                if(w.WeaponId != 506)continue;
                var tmpSkill = owner.skill.GetSkill(w.EquipId * 100);
                if(tmpSkill == null) continue;
                upDmg += tmpSkill.attrMgr.Calc(AttributeType.AroundUpDmg);
            }
            ListPool<BackpackWeaponData>.Release(wList);
            return upDmg;
        }

        /// <summary>
        /// 帽子两侧的手套叠盾提升
        /// </summary>
        /// <param name="weaponSkill"></param>
        /// <returns></returns>
        public float GetAboutArmor(WeaponSkill weaponSkill)
        {
            if (GameDataManager.Instance.CurStageType == EStateType.StageTypeSecret)
            {
                return 0f;
            }
            if (weaponSkill == null) return 0f;
            if (weaponSkill.EquipId != 6) return 0f;
            var weaponUid = weaponSkill.WeaponUid;
            var aboutArmor = 0f;
            var wList = ListPool<BackpackWeaponData>.Get();
            GameDataManager.Instance.GetWeaponSideList(weaponUid, wList);
            foreach (var w in wList)
            {
                if(w.WeaponId != 505)continue;
                var tmpSkill = owner.skill.GetSkill(w.EquipId * 100);
                if(tmpSkill == null) continue;
                aboutArmor += tmpSkill.attrMgr.Calc(AttributeType.AboutArmor);
            }
            ListPool<BackpackWeaponData>.Release(wList);
            return aboutArmor;
        }

        /// <summary>
        /// 帽子同行的手套叠盾提升
        /// </summary>
        /// <param name="weaponSkill"></param>
        /// <returns></returns>
        public float GetPeerArmor(WeaponSkill weaponSkill)
        {
            if (GameDataManager.Instance.CurStageType == EStateType.StageTypeSecret)
            {
                return 0f;
            }
            if (weaponSkill == null) return 0f;
            if (weaponSkill.EquipId != 6) return 0f;
            var weaponUid = weaponSkill.WeaponUid;
            var peerArmor = 0f;
            var wList = ListPool<BackpackWeaponData>.Get();
            GameDataManager.Instance.GetWeaponSameRowList(weaponUid, wList);
            foreach (var w in wList)
            {
                if(w.WeaponId != 505)continue;
                var tmpSkill = owner.skill.GetSkill(w.EquipId * 100);
                if(tmpSkill == null) continue;
                peerArmor += tmpSkill.attrMgr.Calc(AttributeType.PeerArmor);
            }
            ListPool<BackpackWeaponData>.Release(wList);
            return peerArmor;
        }

        /// <summary>
        /// 与手套相邻的所有物理武器速度提升
        /// </summary>
        /// <param name="weaponSkill"></param>
        /// <returns></returns>
        public float GetAboutPhAtkSpd(WeaponSkill weaponSkill)
        {
            if (GameDataManager.Instance.CurStageType == EStateType.StageTypeSecret)
            {
                return 0f;
            }
            if (weaponSkill == null) return 0f;
            if (weaponSkill.GetAtkType() != EquipAtkType.Physic) return 0f;
            var weaponUid = weaponSkill.WeaponUid;
            var aboutPhAtkSpd = 0f;
            var wList = ListPool<BackpackWeaponData>.Get();
            GameDataManager.Instance.GetWeaponNearbyList(weaponUid, wList);
            foreach (var w in wList)
            {
                if(w.EquipId != 6)continue;
                var tmpSkill = owner.skill.GetSkill(w.EquipId * 100);
                if(tmpSkill == null) continue;
                aboutPhAtkSpd += tmpSkill.attrMgr.Calc(AttributeType.AboutPhAtkSpd);
            }
            ListPool<BackpackWeaponData>.Release(wList);
            return aboutPhAtkSpd;
        }

        /// <summary>
        /// 地之斧1205攻速提升
        /// </summary>
        /// <param name="weaponSkill"></param>
        /// <returns></returns>
        private float Get1205UpAtkSpd(WeaponSkill weaponSkill)
        {
            if (weaponSkill == null) return 0f;
            if (weaponSkill.WeaponModelId != 1205) return 0f;
            var trigger = GetTriggerByNameAndComplete(AttributeName.AsHpValueTh, AttributeType.UpAtkSpd);
            if (trigger == null) return 0f;
            return !owner.resource.Full ? 0f : trigger.attrMgr.Calc(AttributeType.UpAtkSpd);
        }
        /// <summary>
        /// 老魔杖2507攻速提升
        /// </summary>
        /// <param name="weaponSkill"></param>
        /// <returns></returns>
        private float Get2507UpAtkSpd(WeaponSkill weaponSkill)
        {
            if (weaponSkill == null) return 0f;
            return weaponSkill.EquipId != 25 ? 0f : weaponSkill.SkillData.attrMgr.Calc(AttributeType.UpAtkSpd);
        }
        /// <summary>
        /// 背包内每有1件地元素装备,地之回旋镖攻速+10%
        /// </summary>
        /// <param name="weaponSkill"></param>
        /// <returns></returns>
        private float Get1105UpAtkSpd(WeaponSkill weaponSkill)
        {
            if (weaponSkill == null) return 0f;
            if (weaponSkill.WeaponModelId != 1105) return 0f;
            var landCircleAtkSpd = attrMgr.Calc(AttributeType.LandCircleAtkSpd);
            var equipCount = GetEquipAttrCount((int)EquipAttrType.Earth);
            return landCircleAtkSpd * equipCount;
        }
        public float GetEquipAttrCount(int attrType)
        {
            var weaponList = GameDataManager.Instance.BackpackWeaponList;
            var equipCount = 0;
            foreach (var weapon in weaponList)
            {
                var equipModelId = weapon.WeaponId;
                var equipModelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(equipModelId);
                if(equipModelCfg == null || equipModelCfg.AttrType != attrType)continue;
                equipCount++;
            }
            return equipCount;
        }
        public SkillTrigger GetTriggerByNameAndComplete(string name, AttributeType complete)
        {
            foreach (var skillTrigger in triggerList)
            {
                if (!skillTrigger.trigger.ContainsKey(name)) continue;
                if (skillTrigger.attrMgr.Calc(complete) > 0) return skillTrigger;
            }
            return null;
        }
        
        #region 魔戒技能暴击率提升逻辑处理
        /// <summary>
        /// 获取魔戒暴击率提升
        /// </summary>
        /// <returns></returns>
        private float GetTheRingCritDmg(WeaponSkill weaponSkill)
        {
            var upCritDmg = 0.0f;
            var isEnableCritAll = false;
            var isEnableCritSame = false;
            var isEnableSynthesisCrit = false;
            var isEnableCritSuperposition = false;
            var theRingSkill = owner.skill.GetSkill(2000);//魔戒技能
            if (theRingSkill != null)
            {
                if (theRingSkill.attrMgr.Calc(AttributeType.EnableCritAll) > 0.5f) isEnableCritAll = true; //魔法戒指对背包内所有武器均提升暴击率
                if (theRingSkill.attrMgr.Calc(AttributeType.EnableCritSame) > 0.5f) isEnableCritSame = true;//魔法戒指对同行同列的武器均提升暴击率
                if (theRingSkill.attrMgr.Calc(AttributeType.EnableSynthesisCrit) > 0.5f) isEnableSynthesisCrit = true;//魔法戒指周围的武器暴击率提升
                if (theRingSkill.attrMgr.Calc(AttributeType.EnableCritSuperposition) > 0.5f) isEnableCritSuperposition = true;//多个魔法戒指提升的暴击率可叠加
            }
            if (isEnableCritAll)//启用所有暴击率;魔法戒指对背包内所有武器均提升暴击率
            {
                var wList = ListPool<int>.Get();
                for (var i = 0; i < GameDataManager.Instance.BackpackWeaponList.Count; i++)
                {
                    if (GameDataManager.Instance.BackpackWeaponList[i].EquipId != 20) continue;
                    var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(GameDataManager.Instance.BackpackWeaponList[i].WeaponId);
                    if (modelCfg?.SynthesisAttr?[0] != null && AttributeHelper.GetAttributeTypeByName(modelCfg.SynthesisAttr[0].Type) == AttributeType.CritRate)
                    {
                        wList.Add(modelCfg.SynthesisAttr[0].Value);
                    }
                }
                if (isEnableCritSuperposition)//启用暴击率叠加;多个魔法戒指提升的暴击率可叠加
                {
                    for (var i = 0; i < wList.Count; i++)
                    {
                        upCritDmg += (wList[i] * GameConst.AttributeDivisor);
                    }
                }
                else
                {
                    if (wList.Count > 1) wList.Sort((paramA, paramB) => paramB.CompareTo(paramA));
                    if (wList.Count > 0) upCritDmg += (wList[0] * GameConst.AttributeDivisor);
                }
                ListPool<int>.Release(wList);
            }
            else if(isEnableCritSame)//启用同行同列暴击率;魔法戒指对同行同列的武器均提升暴击率
            {
                upCritDmg += GetTheRingSameRowAndColumnCritDmg(weaponSkill,isEnableCritSuperposition);
            } 
            else if (isEnableSynthesisCrit)//启用合成暴击率;魔法戒指周围的武器暴击率提升（合成1级:3%,合成2级:6%,合成3级:9%,合成4级:12%）
            {
                upCritDmg += GetTheRingAroundCritDmg(weaponSkill,isEnableCritSuperposition);
            }
            return upCritDmg;
        }
        /// <summary>
        /// 获取武器同行同列暴击率
        /// </summary>
        /// <param name="weaponSkill"></param>
        /// /// <param name="enableCritSuperposition"></param>
        /// <returns></returns>
        private float GetTheRingSameRowAndColumnCritDmg(WeaponSkill weaponSkill,bool enableCritSuperposition)
        {
            var sameRowAndColumnCritDmg = 0.0f;
            var wList = ListPool<BackpackWeaponData>.Get();
            GameDataManager.Instance.GetWeaponSameRowAndColumnList(weaponSkill.WeaponUid, wList);//获取同行同列武器
            if (enableCritSuperposition)//启用暴击率叠加;多个魔法戒指提升的暴击率可叠加
            {
                for (var i = 0; i < wList.Count; i++)
                {
                    if (wList[i].EquipId != 20) continue;
                    var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(wList[i].WeaponId);
                    if (modelCfg?.SynthesisAttr?[0] != null && AttributeHelper.GetAttributeTypeByName(modelCfg.SynthesisAttr[0].Type) == AttributeType.CritRate)
                    {
                        sameRowAndColumnCritDmg += (modelCfg.SynthesisAttr[0].Value * GameConst.AttributeDivisor);
                    }
                }
            }
            else
            {
                var maxValue = 0;
                for (var i = 0; i < wList.Count; i++)
                {
                    if (wList[i].EquipId != 20) continue;
                    var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(wList[i].WeaponId);
                    if (modelCfg?.SynthesisAttr?[0] != null && AttributeHelper.GetAttributeTypeByName(modelCfg.SynthesisAttr[0].Type) == AttributeType.CritRate)
                    {
                        if (modelCfg.SynthesisAttr[0].Value > maxValue) maxValue = modelCfg.SynthesisAttr[0].Value;
                    }
                }
                sameRowAndColumnCritDmg += (maxValue * GameConst.AttributeDivisor);
            }
            ListPool<BackpackWeaponData>.Release(wList);
            return sameRowAndColumnCritDmg;
        }
        /// <summary>
        /// 魔戒周围武器暴击率提升,需判断叠加是否开启
        /// </summary>
        /// <param name="weaponSkill"></param>
        /// <param name="enableCritSuperposition"></param>
        /// <returns></returns>
        private float GetTheRingAroundCritDmg(WeaponSkill weaponSkill,bool enableCritSuperposition)
        {
            var aroundCritDmg = 0.0f;
            var wList = ListPool<BackpackWeaponData>.Get();
            GameDataManager.Instance.GetWeaponNearbyList(weaponSkill.WeaponUid, wList);
            if (enableCritSuperposition)//启用暴击率叠加;多个魔法戒指提升的暴击率可叠加
            {
                for (var i = 0; i < wList.Count; i++)
                {
                    if (wList[i].EquipId != 20) continue;
                    var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(wList[i].WeaponId);
                    if (modelCfg?.SynthesisAttr?[0] != null && AttributeHelper.GetAttributeTypeByName(modelCfg.SynthesisAttr[0].Type) == AttributeType.CritRate)
                    {
                        aroundCritDmg += (modelCfg.SynthesisAttr[0].Value * GameConst.AttributeDivisor);
                    }
                }
            }
            else
            {
                var maxValue = 0;
                for (var i = 0; i < wList.Count; i++)
                {
                    if (wList[i].EquipId != 20) continue;
                    var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(wList[i].WeaponId);
                    if (modelCfg?.SynthesisAttr?[0] != null && AttributeHelper.GetAttributeTypeByName(modelCfg.SynthesisAttr[0].Type) == AttributeType.CritRate)
                    {
                        if (modelCfg.SynthesisAttr[0].Value > maxValue) maxValue = modelCfg.SynthesisAttr[0].Value;
                    }
                }
                aroundCritDmg += (maxValue * GameConst.AttributeDivisor);
            }
            ListPool<BackpackWeaponData>.Release(wList);
            return aroundCritDmg;
        }
        #endregion

    }
}