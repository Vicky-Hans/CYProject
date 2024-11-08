using System;
using DH.Data;
using UnityEngine;
using UnityEngine.Pool;

namespace DH.Game
{
    public partial class PlayerBullet100 : BaseBullet
    {
        public AttackBox attackBox;
        private float bulletSpeed;
        private Vector3 startPos;
        private float range;
        private Vector3 direction;
        private Skill skillData;

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
            if((arg3 & DamageStatus.Dead) != 0)
            {
                // 水之剑回复
                if(WeaponModelId == 105)
                {
                    var atk = skillData.CalcWeaponAtk(WeaponModelId);
                    var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.KillTarget,
                        AttributeType.RevertAtk);
                    if (trigger != null)
                    {
                        var revertAtk = trigger.attrMgr.Calc(AttributeType.RevertAtk);
                        skillData.owner.resource.AddHp((long)(atk * revertAtk));
                    }
                }
            }
            // 暴击了
            if (arg1.isCrit && WeaponModelId == 106)
            {
                CheckFiring(arg1);
            }

            // 穿透次数
            if (immunePierce || skillData.Pierce() < arg1.dmgCount)
            {
                Recycle();
            }
        }

        private void CheckFiring(DamageArgs damageArgs)
        {
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.CritTime,
                    AttributeType.Firing);
            if (trigger == null) return;
            var brulNum = Lodash.RoundToInt(skillData.attrMgr.Calc(AttributeType.BrulNum));
            var brulTime = skillData.attrMgr.Calc(AttributeType.BrulTime) * GameConst.TimeDivisor;
            var fireInterval = skillData.attrMgr.Calc(AttributeType.FireInterval) * GameConst.TimeDivisor;
            var firingDmg = trigger.attrMgr.Calc(AttributeType.FiringDmg);
            var buffValue = damageArgs.damagePoint * firingDmg;

            var monsterList =
                BattleManager.Instance.fightingManagerIns.GetRandMonstersInScreen(brulNum);
            monsterList.ForEach(mon =>
            {
                if(mon == null || mon.CheckMonsterIsDead())return;
                var buff = new Buff
                {
                    id = (int)AttributeType.Firing,
                    attrName = AttributeName.Firing,
                    startTime = GameTime.Instance.GTime,
                    duration = brulTime,
                    interval = fireInterval,
                    value = buffValue,
                    valueType = BuffValueType.Negative,
                    multi = true,
                    equipModelId = WeaponModelId,
                };
                mon.AddFiringBuff(buff);
            });
            ListPool<MonsterController>.Release(monsterList);
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