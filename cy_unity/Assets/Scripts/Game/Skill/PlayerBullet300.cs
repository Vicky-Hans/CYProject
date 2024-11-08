using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerBullet300 : BaseBullet
    {
        public AttackBox attackBox;
        private float bulletSpeed;
        private Vector3 startPos;
        private float range;
        private Vector3 direction;
        private Skill skillData;
        private int pierce = 0;

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
            // 306 降低敌人伤害
            if (WeaponModelId == 306)
            {
                var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.DarkBowHit, AttributeType.MonsterAtkDown);
                if (trigger != null)
                {
                    var monsterAtkDown = trigger.attrMgr.Calc(AttributeType.MonsterAtkDown);
                    var duration = trigger.attrMgr.Calc(AttributeType.Duration) * GameConst.TimeDivisor;
                    var buff = new Buff
                    {
                        id = (int)AttributeType.MonsterAtkDown,
                        attrName = AttributeName.MonsterAtkDown,
                        value = monsterAtkDown,
                        valueType = BuffValueType.Negative,
                        startTime = GameTime.Instance.GTime,
                        duration = 99999,
                        multi = false,
                    };
                    unit.Data.buffMgr.AddBuff(buff);
                }
            }
            // 穿透次数
            if (immunePierce || pierce < arg1.dmgCount)
            {
                Recycle();
            }
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