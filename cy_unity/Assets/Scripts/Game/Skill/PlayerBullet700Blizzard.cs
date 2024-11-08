using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerBullet700Blizzard : BaseBullet
    {
        public AttackBox attackBox;
        private float bulletSpeed;
        private Vector3 startPos;
        private float range;
        private Vector3 direction;
        private Skill skillData;
        private int pierce;
        private bool inited;
        private float time = 0f;
        private float triggerTime;
        private float blizzardInterval;
        private float blizzardTime;
        private float blizzardRange;
        private float blizzardDmg;
        private float triggerCount;
        private float upBlizzardDmg;
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
            blizzardInterval = skillData.attrMgr.Calc(AttributeType.BlizzardInterval) * GameConst.TimeDivisor;
            blizzardTime = skillData.attrMgr.Calc(AttributeType.BlizzardTime) * GameConst.TimeDivisor;
            blizzardRange = skillData.attrMgr.Calc(AttributeType.BlizzardRange);
            blizzardDmg = skillData.attrMgr.Calc(AttributeType.BlizzardDmg);
            CheckUpBlizzardDmg();
            cacheTransform.localScale = Vector3.one * blizzardRange / DefaultSize;
            inited = true;
        }

        private void CheckUpBlizzardDmg()
        {
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.UseBlizzard, AttributeType.UpBlizzardDmg);
            if (trigger == null) return;
            upBlizzardDmg = trigger.attrMgr.Calc(AttributeType.UpBlizzardDmg);
        }

        private void TriggerDmg()
        {
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.EnableBlizzard);
            var atk = skillData.CalcWeaponAtk(WeaponModelId);
            var dmg = atk * blizzardDmg * (1 + upBlizzardDmg * triggerCount);
            var layerMask = LayerMask.GetMask("Enemy");
            var count = Physics2D.OverlapCircleNonAlloc(startPos, blizzardRange , PhysicsUtility.CacheCollider, layerMask);
            for (int i = 0; i < count; ++i)
            {
                var target = PhysicsUtility.CacheCollider[i];
                if (target == null) continue;
                var monster = target.GetComponent<MonsterController>();
                if (monster == null) continue;
                monster.DecHp((int)dmg, WeaponModelId);
                if (trigger != null)
                {
                    CheckDecelerate(monster, trigger);
                }
            }
            triggerCount++;
        }

        

        private void CheckDecelerate(MonsterController unit, SkillTrigger trigger)
        {
            if(triggerCount > 0)return;
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
            triggerTime += elapseSeconds;
            if (triggerTime > blizzardInterval)
            {
                triggerTime = 0;
                TriggerDmg();
            }
            if(time > blizzardTime)
            {
                Recycle();
            }
        }
        
        protected override void Recycle()
        {
            base.Recycle();
        }
        
    }
}