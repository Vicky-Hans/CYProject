using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerBullet300Rain : BaseBullet
    {
        public AttackBox attackBox;
        private float bulletSpeed;
        private Vector3 startPos;
        private float range;
        private Vector3 direction;
        private Skill skillData;
        private int pierce = 0;
        private float releaseTime = 2.23f;
        private float time = 0f;
        private float rangeDmgDis;
        private readonly float DefaultSize = 2f;
        
        public override void InitWithTarget(DamageArgs damageArgs, Func<string, float> paramGetter,
            Vector3 startPosition, Vector3 targetPosition, IPool<GameObject> pool)
        {
            // attackBox.ignoreAttack = true;
            base.InitWithTarget(damageArgs, paramGetter, startPosition, targetPosition, pool);
            skillData = damageArgs.skillData;
            startPos = startPosition;
            rangeDmgDis = skillData.attrMgr.Calc(AttributeType.RangeDmgDis);
            cacheTransform.localScale = Vector3.one * rangeDmgDis / DefaultSize;
            cacheTransform.position = startPosition;
            var rangeDmg = skillData.attrMgr.Calc(AttributeType.RangeDmg);
            attackBox.ResetData();
            damageArgs.dmgPercent = rangeDmg;
            attackBox.damageArgs = damageArgs;
            attackBox.Damage += AttackBoxOnDamage;
            
            // var atk = skillData.CalcWeaponAtk(WeaponModelId);
            // var dmg = atk * rangeDmg;
            // var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.BowHitTarget, AttributeType.Vertigo);
            // var layerMask = LayerMask.GetMask("Enemy");
            // var count = Physics2D.OverlapCircleNonAlloc(startPos, rangeDmgDis , PhysicsUtility.CacheCollider, layerMask);
            // for (int i = 0; i < count; ++i)
            // {
            //     var target = PhysicsUtility.CacheCollider[i];
            //     if (target == null) continue;
            //     var monster = target.GetComponent<MonsterController>();
            //     if (monster == null) continue;
            //     monster.DecHp((int)dmg);
            //     if (trigger != null)
            //     {
            //         CheckVertigo(monster, trigger);
            //     }
            // }
        }

        private void AttackBoxOnDamage(DamageArgs arg1, IDamageable arg2, DamageStatus arg3)
        {
            if((arg3 & DamageStatus.Ignore) != 0)return;
            var unit = arg2 as MonsterController;
            if(unit == null)return;
            var unitPos = unit.transform.position;
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.BowHitTarget, AttributeType.Vertigo);
            if(trigger!=null)
            {
                CheckVertigo(unit, trigger);
            }
        }

        private void CheckVertigo(MonsterController monsterController, SkillTrigger trigger)
        {
            if(monsterController.CheckMonsterIsDead())return;
            trigger.trigger.TryGetValue("prob", out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1) > prob)return;
            var vertigoTime = skillData.attrMgr.Calc(AttributeType.VertigoTime) * GameConst.TimeDivisor;
            var buff = new Buff
            {
                id = (int)AttributeType.Vertigo,
                attrName = AttributeName.Vertigo,
                startTime = GameTime.Instance.GTime,
                duration = vertigoTime,
                interval = 0,
                value = 1f,
                valueType = BuffValueType.Negative,
                multi = true,
            };
            monsterController.AddVertigoBuff(buff);
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