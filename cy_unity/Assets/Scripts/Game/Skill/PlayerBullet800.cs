using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerBullet800 : BaseBullet
    {
        public AttackBox attackBox;
        [AssetPath] public string rangePoisonPath;
        private float bulletSpeed;
        private Vector3 startPos;
        private float range;
        private Vector3 direction;
        private Skill skillData;
        private int pierce;

        public override void InitWithTarget(DamageArgs damageArgs, Func<string, float> paramGetter,
            Vector3 startPosition, Vector3 targetPosition, IPool<GameObject> pool)
        {
            base.InitWithTarget(damageArgs, paramGetter, startPosition, targetPosition, pool);
            skillData = damageArgs.skillData;
            startPos = startPosition;
            bulletSpeed = skillData.BulletSpeed();
            range = skillData.Range();
            direction = targetPosition - startPos;
            bulletSize = skillData.attrMgr.Calc(AttributeType.BulletSize);
            cacheTransform.up = direction;
            cacheTransform.localScale = Vector3.one * bulletSize;
            attackBox.ResetData();
            attackBox.damageArgs = damageArgs;
            attackBox.Damage += AttackBoxOnDamage;
        }

        private void AttackBoxOnDamage(DamageArgs arg1, IDamageable arg2, DamageStatus arg3)
        {
            if((arg3 & DamageStatus.Ignore) != 0)return;
            var unit = arg2 as MonsterController;
            if(unit == null)return;
            var immunePierce = unit.MonsterData.ImmunePierce();
            var unitPos = unit.transform.position;
            CheckPoison(unit, arg1);
            CheckRangePoison(unit, unitPos, arg1);
            CheckPalsy(unit);
            CheckStrongPoison(unit, arg1);
            // 穿透次数
            if (immunePierce || pierce < arg1.dmgCount)
            {
                Recycle();
            }
        }

        private void CheckPoison(MonsterController unit, DamageArgs args)
        {
            var poisonDmg = skillData.attrMgr.Calc(AttributeType.PoisonDmg);
            var poisonTime = skillData.attrMgr.Calc(AttributeType.PoisoningTime) * GameConst.TimeDivisor;
            var poisonInterval = skillData.attrMgr.Calc(AttributeType.PoisonInterval) * GameConst.TimeDivisor;
            var atk = skillData.CalcWeaponAtk(WeaponModelId);
            var buff = new Buff
            {
                id = (int)AttributeType.Poisoning,
                attrName = AttributeName.Poisoning,
                startTime = GameTime.Instance.GTime,
                duration = poisonTime,
                interval = poisonInterval,
                value = atk * poisonDmg,
                valueType = BuffValueType.Negative,
                multi = true,
                equipModelId = WeaponModelId,
            };
            unit.AddPoisonBuff(buff);
        }

        private void PlayRangePoisonEffect(float range, Vector3 pos)
        {
            var effectRadius = 2f;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var obj = InstantiateObj(rangePoisonPath, pos, Quaternion.identity, fightingManager.fightPanelTrans);
            if (obj)
            {
                obj.transform.localScale = Vector3.one * range/effectRadius;
                fightingManager.AddAutoReleaseUnit(obj, 2f, this);
            }
        }
        
        private void CheckRangePoison(MonsterController unit, Vector3 pos, DamageArgs args)
        {
            if(skillData.attrMgr.Calc(AttributeType.EnableRangePoison) < 0.5f)return;
            var rangeDmgDis = skillData.attrMgr.Calc(AttributeType.RangeDmgDis);
            PlayRangePoisonEffect(rangeDmgDis, pos);
            var poisonDmg = skillData.attrMgr.Calc(AttributeType.RangePoisonDmg);
            var poisonTime = skillData.attrMgr.Calc(AttributeType.RangePoisonTime) * GameConst.TimeDivisor;
            var poisonInterval = skillData.attrMgr.Calc(AttributeType.RangePoisonInterval) * GameConst.TimeDivisor;
            var enableRangeDmg = skillData.attrMgr.Calc(AttributeType.EnableRangeDmg) > 0.5f;
            var rangeDmg = skillData.attrMgr.Calc(AttributeType.RangeDmg);
            var atk = skillData.CalcWeaponAtk(WeaponModelId);
            var layerMask = LayerMask.GetMask("Enemy");
            var count = Physics2D.OverlapCircleNonAlloc(pos, rangeDmgDis , PhysicsUtility.CacheCollider, layerMask);
            for (int i = 0; i < count; ++i)
            {
                var target = PhysicsUtility.CacheCollider[i];
                if (target == null) continue;
                var monster = target.GetComponent<MonsterController>();
                if (monster == null || monster == unit) continue;
                if (enableRangeDmg)
                {
                    monster.DecHp((int)(atk * rangeDmg), WeaponModelId);
                }
                var buff = new Buff
                {
                    id = (int)AttributeType.Poisoning,
                    attrName = AttributeName.Poisoning,
                    startTime = GameTime.Instance.GTime,
                    duration = poisonTime,
                    interval = poisonInterval,
                    value = atk * poisonDmg,
                    valueType = BuffValueType.Negative,
                    multi = true,
                    equipModelId = WeaponModelId,
                };
                monster.AddPoisonBuff(buff);
            }
        }

        private void CheckStrongPoison(MonsterController unit, DamageArgs args)
        {
            if(WeaponModelId != 806)return;
            if(unit == null || unit.CheckMonsterIsDead())return;
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.WhenPoisoning, AttributeType.EnablePosionOverlay);
            if (trigger == null) return;
            trigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            prob += skillData.attrMgr.Calc(AttributeType.PoisonProb);
            if (Lodash.RandRangeFloat(0, 1f) > prob) return;
            var atk = skillData.CalcWeaponAtk(WeaponModelId);
            var poisonDmg = skillData.attrMgr.Calc(AttributeType.PoisonDmg);
            var poisonTime = skillData.attrMgr.Calc(AttributeType.PoisoningTime) * GameConst.TimeDivisor;
            var poisonInterval = skillData.attrMgr.Calc(AttributeType.PoisonInterval) * GameConst.TimeDivisor;
            var downPoisonInterval = skillData.attrMgr.Calc(AttributeType.DownPoisonInterval);
            poisonInterval *= (1 - downPoisonInterval);
            var overlay = Lodash.RoundToInt(skillData.attrMgr.Calc(AttributeType.Overlay));
            for (int i = 0; i < overlay; i++)
            {
                var buff = new Buff
                {
                    id = (int)AttributeType.Poisoning,
                    attrName = AttributeName.Poisoning,
                    startTime = GameTime.Instance.GTime,
                    duration = poisonTime,
                    interval = poisonInterval,
                    value = atk * poisonDmg,
                    valueType = BuffValueType.Negative,
                    multi = true,
                    equipModelId = WeaponModelId,
                };
                unit.AddPoisonBuff(buff);
            }
        }
        
        private void CheckPalsy(MonsterController unit)
        {
            if(WeaponModelId != 805)return;
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.Palsy);
            if (trigger == null) return;
            trigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            prob += skillData.attrMgr.Calc(AttributeType.PalsyProb);
            if(Lodash.RandRangeFloat(0, 1f) > prob) return;
            var palsyTime = skillData.attrMgr.Calc(AttributeType.PalsyTime) * GameConst.TimeDivisor;
            var downSpd = skillData.attrMgr.Calc(AttributeType.DownSpd);
            var downAtkSpd = trigger.attrMgr.Calc(AttributeType.DownAtkSpd);
            var buff = new Buff
            {
                id = (int)AttributeType.Palsy,
                attrName = AttributeName.Palsy,
                startTime = GameTime.Instance.GTime,
                duration = palsyTime,
                interval = 0,
                value = 1f,
                valueType = BuffValueType.Negative,
                multi = true,
            };
            unit.AddPalsyBuff(buff);
            var buffDecelerate = new Buff
            {
                id = (int)AttributeType.Decelerate,
                attrName = AttributeName.Decelerate,
                startTime = GameTime.Instance.GTime,
                duration = palsyTime,
                interval = 0,
                value = downSpd,
                valueType = BuffValueType.Negative,
                multi = true,
            };
            unit.AddDecelerateBuff(buffDecelerate);
            var buffDecAtkSpd = new Buff
            {
                id = (int)AttributeType.DownAtkSpd,
                attrName = AttributeName.DownAtkSpd,
                startTime = GameTime.Instance.GTime,
                duration = palsyTime,
                interval = 0,
                value = downAtkSpd,
                valueType = BuffValueType.Negative,
                multi = true,
            };
            unit.Data.buffMgr.AddBuff(buffDecAtkSpd);
        }
        
        public override void OnUpdate(float elapseSeconds)
        {
            var position = cacheTransform.position;
            position += cacheTransform.up * (bulletSpeed * elapseSeconds);
            cacheTransform.position = position;
            if (Vector3.Distance(startPos, position) > range) Recycle();
        }
        
        protected override void Recycle()
        {
            attackBox.Damage -= AttackBoxOnDamage;
            base.Recycle();
        }
        
    }
}