using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerBullet1700 : BaseBullet
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
            direction = (targetPosition - startPos).normalized;
            bulletSize = skillData.attrMgr.Calc(AttributeType.BulletSize);
            var angle = Lodash.Direction2Angle(targetPosition - startPos);
            cacheTransform.localEulerAngles = new Vector3(0,0,angle);
            cacheTransform.localScale = Vector3.one * bulletSize;
            attackBox.ResetData();
            attackBox.damageArgs = damageArgs;
            attackBox.Damage += AttackBoxOnDamage;
        }

        private void AttackBoxOnDamage(DamageArgs arg1, IDamageable arg2, DamageStatus arg3)
        {
            if((arg3 & DamageStatus.Ignore) != 0) return;
            var unit = arg2 as MonsterController;
            if(unit == null) return;
            var immunePierce = unit.MonsterData.ImmunePierce();
            var unitPos = unit.transform.position;
            CheckRangeDmg(unitPos);//范围伤害检测
            //检测击退
            var repelTrigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.Repel);
            if(repelTrigger!=null) CheckRepel(unit,repelTrigger,arg1);
            //检测眩晕
            var vertigoTrigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.Vertigo);
            if(vertigoTrigger!=null) CheckVertigo(unit, vertigoTrigger);
            // 穿透次数
            if (immunePierce || skillData.Pierce() < arg1.dmgCount) Recycle();
        }
        /// <summary>
        /// 检测范围伤害
        /// </summary>
        /// <param name="pos"></param>
        private void CheckRangeDmg(Vector3 pos)
        {
            if(skillData.attrMgr.Calc(AttributeType.EnableRangeDmg) < 0.5f)return;
            var rangeDmg = skillData.attrMgr.Calc(AttributeType.RangeDmg);
            var maxRangeDmgDis = skillData.attrMgr.Calc(AttributeType.RangeDmgDis);
            var minRangeDmgDis = skillData.attrMgr.Calc(AttributeType.RangeDmgDisMin);
            PlayRangeDmgEffect(maxRangeDmgDis, pos);
            var atk = skillData.CalcWeaponAtk(WeaponModelId);
            var layerMask = LayerMask.GetMask("Enemy");
            var count = Physics2D.OverlapCircleNonAlloc(pos, maxRangeDmgDis , PhysicsUtility.CacheCollider, layerMask);
            for (var i = 0; i < count; ++i)
            {
                var target = PhysicsUtility.CacheCollider[i];
                if (target == null) continue;
                var monster = target.GetComponent<MonsterController>();
                if (monster == null || monster.CheckMonsterIsDead()) continue;
                var mPos = monster.transform.position;
                if ((mPos - pos).sqrMagnitude < minRangeDmgDis * minRangeDmgDis) continue;
                monster.DecHp(Lodash.RoundToInt(atk * rangeDmg), WeaponModelId);
            }
        }
        private void PlayRangeDmgEffect(float range, Vector3 pos)
        {
            var effectRadius = 2f;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var obj = InstantiateObj(rangeDmgPath, pos, Quaternion.identity, fightingManager.fightPanelTrans);
            if (!obj) return;
            obj.transform.localScale = Vector3.one * range/effectRadius;
            fightingManager.AddAutoReleaseUnit(obj, 2f, this);
        }
        /// <summary>
        /// 检测击退效果
        /// </summary>
        /// <param name="monsterController"></param>
        /// <param name="trigger"></param>
        /// <param name="args"></param>
        private void CheckRepel(MonsterController monsterController, SkillTrigger trigger,DamageArgs args)
        {
            if(monsterController == null || monsterController.CheckMonsterIsDead()) return;
            trigger.trigger.TryGetValue("prob", out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1) > prob) return;
            var maxRepelRange = skillData.attrMgr.Calc(AttributeType.RepelMaxRange) * GameConst.AttributeDivisor;
            var minRepelRange = skillData.attrMgr.Calc(AttributeType.RepelMinRange) * GameConst.AttributeDivisor;
            var repelRange = Lodash.RandRangeFloat(minRepelRange, maxRepelRange + 1);
            monsterController.Repeled(repelRange);
        }
        /// <summary>
        /// 检测晕眩
        /// </summary>
        /// <param name="monsterController"></param>
        /// <param name="trigger"></param>
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
                duration = Lodash.RandRangeFloat(0, vertigoTime + 1),
                interval = 0,
                value = 1f,
                valueType = BuffValueType.Negative,
                multi = true,
            };
            monsterController.AddVertigoBuff(buff);
        }
        public override void OnUpdate(float elapseSeconds)
        {
            var position = cacheTransform.position;
            position += direction * (bulletSpeed * elapseSeconds);
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