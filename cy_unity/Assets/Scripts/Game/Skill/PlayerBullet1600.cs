using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerBullet1600 : BaseBullet
    {
        public AttackBox attackBox;
        [AssetPath] public string rangeDmgPath;
        private float bulletSpeed;
        private Vector3 startPos;
        private float range;
        private Vector3 direction;
        private Skill skillData;

        public override void InitWithTarget(DamageArgs damageArgs, Func<string, float> paramGetter,
            Vector3 startPosition, Vector3 targetPosition, IPool<GameObject> pool)
        {
            damageArgs.dmgType = DmgType.Range;
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
            CheckRangeDmg(unit, unitPos, arg1);
            // 回复自身攻击x%血量
            CheckRevert();
            // 穿透次数
            if (immunePierce || skillData.Pierce() < arg1.dmgCount)
            {
                Recycle();
            }
        }
        
        private void PlayRangeDmgEffect(float range, Vector3 pos)
        {
            var effectRadius = 1.2f;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var obj = InstantiateObj(rangeDmgPath, pos, Quaternion.identity, fightingManager.fightPanelTrans);
            if (obj)
            {
                obj.transform.localScale = Vector3.one * range/effectRadius;
                fightingManager.AddAutoReleaseUnit(obj, 2f, this);
            }
        }

        private void CheckRangeDmg(MonsterController unit, Vector3 pos, DamageArgs args)
        {
            var rangeDmg = skillData.attrMgr.Calc(AttributeType.RangeDmg);
            var rangeDmgDis = skillData.attrMgr.Calc(AttributeType.RangeDmgDis);
            PlayRangeDmgEffect(rangeDmgDis, pos);
            var atk = skillData.CalcWeaponAtk(WeaponModelId, dmgType:DmgType.Range);
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget,
                AttributeType.Fainting);
            var layerMask = LayerMask.GetMask("Enemy");
            var count = Physics2D.OverlapCircleNonAlloc(pos, rangeDmgDis , PhysicsUtility.CacheCollider, layerMask);
            for (int i = 0; i < count; ++i)
            {
                var target = PhysicsUtility.CacheCollider[i];
                if (target == null) continue;
                var monster = target.GetComponent<MonsterController>();
                if (monster == null || monster.CheckMonsterIsDead()) continue;
                if (monster != unit)
                {
                    monster.DecHp((int)(atk * rangeDmg), WeaponModelId);
                }
                if (trigger != null)
                {
                    CheckRangeFaint(unit, pos, args, trigger);
                }
            }
        }

        private void CheckRangeFaint(MonsterController unit, Vector3 pos, DamageArgs args, SkillTrigger trigger)
        {
            
            trigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            prob += skillData.attrMgr.Calc(AttributeType.FaintingProb);
            var faintingTime = trigger.attrMgr.Calc(AttributeType.FaintingTime) *
                              GameConst.TimeDivisor;
            var downSpd = trigger.attrMgr.Calc(AttributeType.DownSpd);
            var downAtkSpd = trigger.attrMgr.Calc(AttributeType.DownAtkSpd);
            var downHit = trigger.attrMgr.Calc(AttributeType.DownHit);
            downHit += skillData.attrMgr.Calc(AttributeType.DownHit);
            if(Lodash.RandRangeFloat(0, 1f) > prob) return;
            var buff = new Buff
            {
                id = (int)AttributeType.Fainting,
                attrName = AttributeName.Fainting,
                startTime = GameTime.Instance.GTime,
                duration = faintingTime,
                interval = 0,
                value = 1f,
                valueType = BuffValueType.Negative,
                multi = false,
            };
            unit.AddFaintingBuff(buff);
            buff = new Buff
            {
                id = (int)AttributeType.Decelerate,
                attrName = AttributeName.Decelerate,
                startTime = GameTime.Instance.GTime,
                duration = faintingTime,
                interval = 0,
                value = downSpd,
                valueType = BuffValueType.Negative,
                multi = true,
            };
            unit.AddDecelerateBuff(buff);
            buff = new Buff
            {
                id = (int)AttributeType.DownAtkSpd,
                attrName = AttributeName.DownAtkSpd,
                startTime = GameTime.Instance.GTime,
                duration = faintingTime,
                interval = 0,
                value = downAtkSpd,
                valueType = BuffValueType.Negative,
                multi = true,
            };
            unit.Data.buffMgr.AddBuff(buff);
            buff = new Buff
            {
                id = (int)AttributeType.DownHit,
                attrName = AttributeName.DownHit,
                startTime = GameTime.Instance.GTime,
                duration = faintingTime,
                interval = 0,
                value = downHit,
                valueType = BuffValueType.Negative,
                multi = false,
            };
            unit.Data.buffMgr.AddBuff(buff);
        }

        private void CheckRevert()
        {
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.RevertAtk);
            if (trigger == null) return;
            trigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if (Lodash.RandRangeFloat(0, 1f) > prob) return;
            var atk = skillData.CalcWeaponAtk(WeaponModelId, dmgType:DmgType.Range);
            var revertAtk = trigger.attrMgr.Calc(AttributeType.RevertAtk);
            skillData.owner.resource.AddHp(Lodash.RoundToInt(atk * revertAtk));
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