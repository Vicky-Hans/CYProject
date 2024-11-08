using System;
using DH.Data;
using UnityEngine;
namespace DH.Game
{
    public partial class PlayerClothesBullet910058 : BaseBullet
    {
        private Vector3 startPos;
        private UnitBase ownerData;
        private float range = 1f;
        private float poisonPassTime;
        private float poisonPassSecDmg;
        private float dmgTime = 0f;
        private float startTime = 0f;
        private float durationTime = 1f;
        private Vector2 boxSize = Vector2.one;
        private readonly float intervalTime = 1f;
        public LayerMask layerMask;
        public override void InitWithTarget(DamageArgs damageArgs, Func<string, float> paramGetter,
            Vector3 startPosition, Vector3 targetPosition, IPool<GameObject> pool)
        {
            base.InitWithTarget(damageArgs, paramGetter, startPosition, targetPosition, pool);
            startPos = startPosition;
            ownerData = damageArgs.sender.Data;
            var poisonPassRangeTrigger = ownerData.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.PoisonPassRange);
            if (poisonPassRangeTrigger != null)
            {
                var poisonPassDmgProp = 0f;
                range = poisonPassRangeTrigger.attrMgr.Calc(AttributeType.PoisonPassRange);
                boxSize *= range;
                durationTime = poisonPassRangeTrigger.attrMgr.Calc(AttributeType.PoisonPassTime)*GameConst.TimeDivisor;
                durationTime += ownerData.clothesSkillAttr.Calc(AttributeType.PoisonPassTime);
                poisonPassDmgProp = poisonPassRangeTrigger.attrMgr.Calc(AttributeType.PoisonPassSecDmg);
                poisonPassDmgProp += ownerData.clothesSkillAttr.Calc(AttributeType.PoisonPassSecDmg);
                poisonPassSecDmg = ownerData.attr.Calc(AttributeType.Atk)*poisonPassDmgProp;
            }
            ApplyDamageOnMonster();
        }
        public override void OnUpdate(float elapseSeconds)
        {
            if(Recycled)return;
            dmgTime += elapseSeconds;
            startTime += elapseSeconds;
            if (dmgTime > intervalTime)
            {
                dmgTime = 0;
                ApplyDamageOnMonster();
            }
            if (startTime > durationTime) Recycle();
        }
        /// <summary>
        /// 检测范围内的怪物
        /// </summary>
        private void ApplyDamageOnMonster()
        {
            var count = Physics2D.OverlapBoxNonAlloc(startPos, Vector2.one * range, transform.eulerAngles.z, PhysicsUtility.CacheCollider, layerMask);
            if (count == 0) return;
            for (var i = 0; i < count; i++)
            {
                var target = PhysicsUtility.CacheCollider[i];
                var monster = target.GetComponent<MonsterController>();
                if (monster == null) continue;
                if (monster.CheckMonsterIsDead()) continue;
                var poisonBuffs = monster.Data.buffMgr.FindBuffsById((int)AttributeType.PoisonPassSecDmg);
                if (poisonBuffs is { Count: > 0 }) continue;
                monster.DecHp(Mathf.RoundToInt(poisonPassSecDmg));
                if (monster.CheckMonsterIsDead()) continue;
                monster.Data.buffMgr.AddBuff(new Buff
                {
                    id = (int)AttributeType.PoisonPassSecDmg,
                    attrName = AttributeName.PoisonPassSecDmg,
                    startTime = GameTime.Instance.GTime,
                    duration = 1, interval = 1,
                    value = poisonPassSecDmg,
                    valueType = BuffValueType.Negative,
                    multi = true, clothesId = 2,
                });
            }
        }
    }
}