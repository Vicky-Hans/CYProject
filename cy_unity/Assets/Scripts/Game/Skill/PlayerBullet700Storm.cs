using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerBullet700Storm : BaseBullet
    {
        public AttackBox attackBox;
        private float bulletSpeed;
        private Vector3 startPos;
        private float range;
        private Vector3 direction;
        private Skill skillData;
        private int pierce;
        private float releaseTime = 1.84f;
        private float time = 0f;
        private readonly float DefaultSize = 2f;

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
            pierce = skillData.Pierce();
            var rangeDmg = skillData.attrMgr.Calc(AttributeType.RangeDmg);
            var crystalRange = skillData.attrMgr.Calc(AttributeType.CrystalRange);
            damageArgs.dmgPercent = rangeDmg;
            // cacheTransform.up = direction;
            cacheTransform.localScale = Vector3.one * crystalRange / DefaultSize;
            
            attackBox.ResetData();
            damageArgs.dmgType = DmgType.Ignore;
            attackBox.damageArgs = damageArgs;
            attackBox.Damage += AttackBoxOnDamage;
            
            var atk = skillData.CalcWeaponAtk(WeaponModelId);
            var dmg = atk * rangeDmg;
            // var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.EnableCrystal);
            var layerMask = LayerMask.GetMask("Enemy");
            var count = Physics2D.OverlapCircleNonAlloc(startPos, crystalRange , PhysicsUtility.CacheCollider, layerMask);
            for (int i = 0; i < count; ++i)
            {
                var target = PhysicsUtility.CacheCollider[i];
                if (target == null) continue;
                var monster = target.GetComponent<MonsterController>();
                if (monster == null) continue;
                monster.DecHp((int)dmg, WeaponModelId);
                // if (trigger != null)
                // {
                //     CheckDecelerate(monster, trigger);
                //     CheckFrozen(monster, damageArgs);
                // }
            }
        }

        private void AttackBoxOnDamage(DamageArgs arg1, IDamageable arg2, DamageStatus arg3)
        {
            if((arg3 & DamageStatus.Ignore) != 0)return;
            var unit = arg2 as MonsterController;
            if(unit == null)return;
            var unitPos = unit.transform.position;
            
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.EnableCrystal);
            if (trigger != null)
            {
                CheckDecelerate(unit, trigger);
                CheckFrozen(unit, arg1);
            }
        }



        private void CheckDecelerate(MonsterController unit, SkillTrigger trigger)
        {
            if(unit == null || unit.CheckMonsterIsDead())return;
            if (trigger == null) return;
            // trigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            // var prob = probValue * GameConst.AttributeDivisor;
            // if(Lodash.RandRangeFloat(0, 1) > prob)return;
            var decelerateTime = trigger.attrMgr.Calc(AttributeType.DecelerateTime) * GameConst.TimeDivisor;
            var downSpd = trigger.attrMgr.Calc(AttributeType.DownSpd);
            var buff = new Buff
            {
                id = (int)AttributeType.Decelerate,
                attrName = AttributeName.Decelerate,
                startTime = GameTime.Instance.GTime,
                duration = decelerateTime,
                interval = 0,
                value = downSpd,
                valueType = BuffValueType.Negative,
                multi = true,
            };
            unit.AddDecelerateBuff(buff);
        }
        
        private void CheckFrozen(MonsterController unit, DamageArgs args)
        {
            if(unit == null || unit.CheckMonsterIsDead())return;
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.CrystalHit, AttributeType.Frozen);
            if (trigger == null) return;
            trigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1) > prob)return;
            var frozenTime = trigger.attrMgr.Calc(AttributeType.FrozenTime) * GameConst.TimeDivisor;
            var buff = new Buff
            {
                id = (int)AttributeType.Frozen,
                attrName = AttributeName.Frozen,
                startTime = GameTime.Instance.GTime,
                duration = frozenTime,
                interval = 0,
                value = 0f,
                valueType = BuffValueType.Negative,
                multi = true,
            };
            unit.AddFrozenBuff(buff);
        }
        
        public override void OnUpdate(float elapseSeconds)
        {
            if(Recycled)return;
            time += elapseSeconds;
            if (time > releaseTime)
            {
                Recycle();
            }
        }
        
        protected override void Recycle()
        {
            attackBox.Damage -= AttackBoxOnDamage;
            base.Recycle();
        }
        
    }
}