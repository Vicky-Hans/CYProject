using System;
using DH.Config;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerBullet2500 : BaseBullet
    {
        private int pierce;
        private float range;
        private float atkDmg = 0f;
        private Skill skillData;
        private Vector3 startPos;
        private float bulletSpeed;
        public AttackBox attackBox;
        private DamageArgs curDamageArgs;
        private IPool<GameObject> curBulletPool;
        private Func<string, float> curParamGetter;
        [AssetPath] public string explodePath;
        [SyncAssetPath] public string trumpBulletPath;
        public override void InitWithTarget(DamageArgs damageArgs, Func<string, float> paramGetter,
            Vector3 startPosition, Vector3 targetPosition, IPool<GameObject> pool)
        {
            base.InitWithTarget(damageArgs, paramGetter, startPosition, targetPosition, pool);
            curBulletPool = pool;
            startPos = startPosition;
            curDamageArgs = damageArgs;
            curParamGetter = paramGetter;
            skillData = damageArgs.skillData;
            range = damageArgs.skillData.Range();
            pierce = damageArgs.skillData.Pierce();
            bulletSpeed = damageArgs.skillData.BulletSpeed();
            bulletSize = skillData.attrMgr.Calc(AttributeType.BulletSize);
            atkDmg = damageArgs.skillData.CalcAtk();
            var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(WeaponModelId);
            if (modelCfg != null) atkDmg = Mathf.FloorToInt(atkDmg * modelCfg.Compose[0] * GameConst.AttributeDivisor + 0.5f);
            cacheTransform.up = targetPosition - startPos;
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
            CheckEntanglement(unit);//检测缠绕属性
            CheckDeathVine(unit,unitPos);//死亡蔓藤属性
            CheckRangeDmg(unit, unitPos);//范围伤害
            // 穿透次数
            if (immunePierce || pierce < arg1.dmgCount) Recycle();
        }
        /// <summary>
        /// 检测藤曼缠绕
        /// </summary>
        /// <param name="unit"></param>
        private void CheckEntanglement(MonsterController unit)
        {
            if(unit == null || unit.CheckMonsterIsDead())return;
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.Enwind);
            if (trigger == null) return;
            trigger.trigger.TryGetValue("prob", out var probValue);
            var prob = probValue * GameConst.AttributeDivisor+skillData.attrMgr.Calc(AttributeType.EnwindProb);
            if(Lodash.RandRangeFloat(0, 1) > prob) return;
            CheckEntanglementBuff(unit, trigger);//添加buff
        }
        /// <summary>
        /// 检测死亡蔓藤属性
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="pos"></param>
        private void CheckDeathVine(MonsterController unit,Vector3 pos)
        {
            if(unit == null || unit.CheckMonsterIsDead())return;
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.EnableDeathEnwind);
            if (trigger == null) return;
            trigger.trigger.TryGetValue("prob", out var probValue);
            var prob = probValue * GameConst.AttributeDivisor+skillData.attrMgr.Calc(AttributeType.DeathEnwindProb);
            if(Lodash.RandRangeFloat(0, 1) > prob) return;
            if (skillData.owner.buffMgr.FindBuffById((int)AttributeType.DeathEnwindDmg) != null) return;
            skillData.owner.buffMgr.AddBuff(new Buff {  
                id = (int)AttributeType.DeathEnwindDmg,
                attrName = AttributeName.DeathEnwindDmg,
                startTime = GameTime.Instance.GTime,
                valueType = BuffValueType.Negative,
                duration = 2.1f, interval = 0, value = 0, multi = true});
            var playerStartPos = BattleManager.Instance.fightingManagerIns.playerCtrl.transform.position;
            var effectStartPos = new Vector3(playerStartPos.x+0.6f,playerStartPos.y,0);
            var bullet = curBulletPool.InstantiateObj(trumpBulletPath, effectStartPos, Quaternion.identity, BattleManager.Instance.fightingManagerIns.fightPanelTrans);
            var bulletComp = bullet.GetComponent<PlayerBullet2500Trump>();
            bulletComp.dmgValue = atkDmg * trigger.attrMgr.Calc(AttributeType.DeathEnwindDmg);
            bulletComp.InitWithTarget(curDamageArgs.Clone(), curParamGetter, effectStartPos, pos, curBulletPool);
        }
        /// <summary>
        /// 检测范围伤害属性
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="pos"></param>
        private void CheckRangeDmg(MonsterController unit, Vector3 pos)
        {
            if (skillData.attrMgr.Calc(AttributeType.EnableRangeDmg) <= 0.5f) return;
            var rangeDmg = skillData.attrMgr.Calc(AttributeType.RangeDmg);
            var rangeDmgDis = skillData.attrMgr.Calc(AttributeType.RangeDmgDis);
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.RangeDmgHit,AttributeType.Enwind);
            var layerMask = LayerMask.GetMask("Enemy");
            var count = Physics2D.OverlapCircleNonAlloc(pos, rangeDmgDis , PhysicsUtility.CacheCollider, layerMask);
            PlayRangeDmgEffect(rangeDmgDis, pos);
            for (var i = 0; i < count; ++i)
            {
                var target = PhysicsUtility.CacheCollider[i];
                if (target == null) continue;
                var monster = target.GetComponent<MonsterController>();
                if (monster == null || monster.CheckMonsterIsDead()) continue;
                if (monster != unit) monster.DecHp((int)(atkDmg * rangeDmg), WeaponModelId);
                if (monster.CheckMonsterIsDead() || trigger == null) continue;
                if (monster != unit) CheckEntanglementBuff(monster, trigger);//添加buff
            }
        }
        /// <summary>
        /// 添加藤曼缠绕束缚buff
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="trigger"></param>
        private void CheckEntanglementBuff(MonsterController unit, SkillTrigger trigger)
        {
            var enWindBuff = unit.Data.buffMgr.FindBuffById((int)AttributeType.Enwind);
            if (enWindBuff != null)
            {
                enWindBuff.startTime = GameTime.Instance.GTime;
            }
            else
            {
                var enWindTime = (trigger.attrMgr.Calc(AttributeType.EnwindTime)+skillData.attrMgr.Calc(AttributeType.EnwindTime)) * GameConst.TimeDivisor;
                var dmgValue = atkDmg * trigger.attrMgr.Calc(AttributeType.EnwindDmg);
                unit.AddEnwindBuff(new Buff
                {
                    id = (int)AttributeType.Enwind,
                    attrName = AttributeName.Enwind,
                    startTime = GameTime.Instance.GTime,
                    duration = enWindTime, multi = true,
                    interval = 1, value = dmgValue,
                    valueType = BuffValueType.Negative,
                });
            }
        }
        private void PlayRangeDmgEffect(float rangeRadius, Vector3 pos)
        {
            var effectRadius = 1.5f;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var obj = InstantiateObj(explodePath, pos, Quaternion.identity, fightingManager.fightPanelTrans);
            if (!obj) return;
            obj.transform.localPosition = new Vector3(obj.transform.localPosition.x,obj.transform.localPosition.y,obj.transform.localPosition.z);
            obj.transform.localScale = Vector3.one * rangeRadius/effectRadius;
            fightingManager.AddAutoReleaseUnit(obj, 1f, this);
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