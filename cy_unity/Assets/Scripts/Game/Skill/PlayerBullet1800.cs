using System;
using DH.Data;
using UnityEngine;
namespace DH.Game
{
    public partial class PlayerBullet1800 : BaseBullet
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
            if(unit == null) return;
            var immunePierce = unit.MonsterData.ImmunePierce();
            var unitPos = unit.transform.position;
            CheckRangeDmg(unit, unitPos);
            // 穿透次数
            if (immunePierce || skillData.Pierce() < arg1.dmgCount) Recycle();
        }
        private void CheckRangeDmg(MonsterController unit, Vector3 pos)
        {
            if(skillData.attrMgr.Calc(AttributeType.EnableRangeDmg) < 0.5f)return;
            var maxRangeDmgDis = skillData.attrMgr.Calc(AttributeType.RangeDmgDis);
            PlayRangeDmgEffect(maxRangeDmgDis, pos);
            var rangeDmg = skillData.attrMgr.Calc(AttributeType.RangeDmg);
            var atk = skillData.CalcWeaponAtk(WeaponModelId);
            var dmg = atk * rangeDmg;
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget,AttributeType.Decelerate);
            var layerMask = LayerMask.GetMask("Enemy");
            var count = Physics2D.OverlapCircleNonAlloc(pos, maxRangeDmgDis , PhysicsUtility.CacheCollider, layerMask);
            for (var i = 0; i < count; ++i)
            {
                var target = PhysicsUtility.CacheCollider[i];
                if (target == null) continue;
                var monster = target.GetComponent<MonsterController>();
                if (monster == null || monster.CheckMonsterIsDead()) continue;
                monster.DecHp((int)dmg, WeaponModelId);
                if (trigger != null) CheckDecelerate(unit,trigger);
            }
        }
        private void PlayRangeDmgEffect(float range, Vector3 pos)
        {
            var effectRadius = 1.5f;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var obj = InstantiateObj(rangeDmgPath, pos, Quaternion.identity, fightingManager.fightPanelTrans);
            if (!obj) return;
            obj.transform.localPosition = new Vector3(obj.transform.localPosition.x-0.5f,obj.transform.localPosition.y+1.5f,obj.transform.localPosition.z);
            obj.transform.localScale = Vector3.one * range/effectRadius;
            fightingManager.AddAutoReleaseUnit(obj, 2f, this);
        }
        private void CheckDecelerate(MonsterController unit, SkillTrigger trigger)
        {
            if(trigger == null)return;
            trigger.trigger.TryGetValue("prob", out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1) > prob) return;
            var decelerateTime = trigger.attrMgr.Calc(AttributeType.DecelerateTime) * GameConst.TimeDivisor;
            var downAtkSpdTime = trigger.attrMgr.Calc(AttributeType.DownAtkSpdTime) * GameConst.TimeDivisor;
            var downSpd = trigger.attrMgr.Calc(AttributeType.DownSpd);
            var downAtkSpd = trigger.attrMgr.Calc(AttributeType.DownAtkSpd);
            var downSpdBuff = new Buff
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
            unit.AddDecelerateBuff(downSpdBuff);
            var buffDecAtkSpd = new Buff
            {
                id = (int)AttributeType.DownAtkSpd,
                attrName = AttributeName.DownAtkSpd,
                startTime = GameTime.Instance.GTime,
                duration = downAtkSpdTime,
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