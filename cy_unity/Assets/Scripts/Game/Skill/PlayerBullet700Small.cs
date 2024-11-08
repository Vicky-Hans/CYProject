using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public class PlayerBullet700Small : BaseBullet
    {
        public AttackBox attackBox;
        private float bulletSpeed;
        private Vector3 startPos;
        private float range;
        private Vector3 direction;
        private Skill skillData;
        private int pierce;
        public MonsterController originalMonster { get; set; }

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
            cacheTransform.localScale = Vector3.one * 0.6f;
            pierce = skillData.Pierce();
            var smallDmg = skillData.attrMgr.Calc(AttributeType.SmallDmg);
            damageArgs.dmgPercent = smallDmg;
            // var angle = Lodash.Direction2Angle(direction);
            // cacheTransform.rotation = Quaternion.Euler(0, 0, angle+90);
            cacheTransform.up = direction;
            // cacheTransform.localScale = Vector3.one;
            attackBox.ResetData();
            if (originalMonster != null)
            {
                attackBox.AddDamagable(originalMonster);
            }
            attackBox.damageArgs = damageArgs;
            attackBox.Damage += AttackBoxOnDamage;
        }

        private void AttackBoxOnDamage(DamageArgs arg1, IDamageable arg2, DamageStatus arg3)
        {
            if((arg3 & DamageStatus.Ignore) != 0)return;
            var unit = arg2 as MonsterController;
            if(unit == null)return;
            var immunePierce = unit.MonsterData.ImmunePierce();
            CheckDecelerate(unit, arg1);
            // 穿透次数
            if (immunePierce || pierce < arg1.dmgCount)
            {
                Recycle();
            }
        }

        private void CheckDecelerate(MonsterController unit, DamageArgs args)
        {
            if(unit == null || unit.CheckMonsterIsDead())return;
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.SmallHitTarget, AttributeType.Decelerate);
            if (trigger == null) return;
            trigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1) > prob)return;
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
        
        public override void OnUpdate(float elapseSeconds)
        {
            var position = cacheTransform.position;
            position += direction.normalized * (bulletSpeed * elapseSeconds);
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