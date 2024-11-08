using System;
using DH.Config;
using DH.Data;
using UnityEngine;
using UnityEngine.Pool;

namespace DH.Game
{
    public partial class PlayerBullet1400 : BaseBullet
    {
        private Vector3 direction;
        private Skill skillData;
        private float duration = 1f;
        private float bTime = 0f;
        [AssetPath] public string rangeDmgPath;
        public MonsterController TargetMonster { get; set; }

        public override void InitWithTarget(DamageArgs damageArgs, Func<string, float> paramGetter,
            Vector3 startPosition, Vector3 targetPosition, IPool<GameObject> pool)
        {
            duration = 1f;
            base.InitWithTarget(damageArgs, paramGetter, startPosition, targetPosition, pool);
            skillData = damageArgs.skillData;
            bulletSize = skillData.BulletSize();
            cacheTransform.localScale = Vector3.one * bulletSize;
            if (TargetMonster != null && !TargetMonster.CheckMonsterIsDead()) TargetMonster.OnDamage(damageArgs.Clone());//目标伤害
            if(skillData.attrMgr.Calc(AttributeType.EnableRangeDmg) > 0.5f) CheckRangeDmg(startPosition);//磁暴伤害
            if (WeaponModelId != 1406) return;
            var allThunderTrigger = skillData.GetTriggerByNameAndComplete(AttributeName.ThunderWandHit, AttributeType.AllThunderDmg);//全体雷击检测
            if (allThunderTrigger == null) return;
            allThunderTrigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var atkValue = damageArgs.skillData.CalcAtk();
            var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(WeaponModelId);
            if (modelCfg != null) atkValue = Mathf.FloorToInt(atkValue * modelCfg.Compose[0] * GameConst.AttributeDivisor + 0.5f);
            var prob = probValue * GameConst.AttributeDivisor+skillData.attrMgr.Calc(AttributeType.AllThunderProb);
            if (Lodash.RandRangeFloat(0, 1f) <= prob) 
                CheckAllThunder(allThunderTrigger,atkValue);
        }
        /// <summary>
        /// 磁暴伤害检测
        /// </summary>
        /// <param name="pos"></param>
        private void CheckRangeDmg(Vector3 pos)
        {
            var rangeDmgDis = skillData.attrMgr.Calc(AttributeType.RangeDmgDis);
            PlayRangeDmgEffect(rangeDmgDis, pos);
            var rangeDmg = skillData.attrMgr.Calc(AttributeType.RangeDmg);
            var atk = skillData.CalcWeaponAtk(WeaponModelId);
            var dmg = atk * rangeDmg;
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.MagneticHit, AttributeType.Electrify);
            var layerMask = LayerMask.GetMask("Enemy");
            var count = Physics2D.OverlapCircleNonAlloc(pos, rangeDmgDis , PhysicsUtility.CacheCollider, layerMask);
            for (var i = 0; i < count; ++i)
            {
                var target = PhysicsUtility.CacheCollider[i];
                if (target == null) continue;
                var monster = target.GetComponent<MonsterController>();
                if (monster == null) continue;
                monster.DecHp(Lodash.RoundToInt(dmg), WeaponModelId);
                if (trigger != null) CheckElectrify(monster, trigger);
            }
        }
        private void PlayRangeDmgEffect(float range, Vector3 pos)
        {
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var obj = InstantiateObj(rangeDmgPath, pos, Quaternion.identity, fightingManager.fightPanelTrans);
            if (!obj) return;
            obj.transform.localScale = Vector3.one * range*0.5f;
            fightingManager.AddAutoReleaseUnit(obj, 2f, this);
        }
        /// <summary>
        /// 磁暴伤害目标感电
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="trigger"></param>
        private void CheckElectrify(MonsterController unit, SkillTrigger trigger)
        {
            if(unit == null || unit.CheckMonsterIsDead())return;
            trigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if (Lodash.RandRangeFloat(0, 1f) > prob) return;
            var electrifyTime = skillData.attrMgr.Calc(AttributeType.ElectrifyTime);
            var electrifyDmg = skillData.attrMgr.Calc(AttributeType.ElectrifyDmg);
            unit.AddElectrifyBuff(new Buff
            {
                id = (int)AttributeType.Electrify,
                attrName = AttributeName.Electrify,
                startTime = GameTime.Instance.GTime,
                duration = electrifyTime,
                interval = 10000,
                value = electrifyDmg,
                valueType = BuffValueType.Negative,
                multi = false,
            });
        }
        /// <summary>
        /// 全体雷击触发检测
        /// </summary>
        private void CheckAllThunder(SkillTrigger allThunderTrigger,float damagePoint)
        {
            var dmgValue = damagePoint*allThunderTrigger.attrMgr.Calc(AttributeType.AllThunderDmg);
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var monsterList = fightingManager.GetAllMonsterInScreen();
            if (monsterList.Count <= 0) return;
            foreach (var monsterObj in monsterList)
            {
                if (monsterObj == null || monsterObj.CheckMonsterIsDead() || monsterObj == TargetMonster) continue;
                var obj = InstantiateObj(modelPath, monsterObj.transform.position, Quaternion.identity, fightingManager.fightPanelTrans);
                if (!obj) continue;
                monsterObj.DecHp(Lodash.RoundToInt(dmgValue));
                obj.transform.localScale = Vector3.one * bulletSize;
                fightingManager.AddAutoReleaseUnit(obj, 0.4f, this);
            }
            ListPool<MonsterController>.Release(monsterList);
        }
        public override void OnUpdate(float elapseSeconds)
        {
            if(Recycled)return;
            bTime += elapseSeconds;
            if (bTime > duration) Recycle();
        }
    }
}