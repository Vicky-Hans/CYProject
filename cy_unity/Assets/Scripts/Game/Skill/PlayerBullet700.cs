using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerBullet700 : BaseBullet
    {
        public AttackBox attackBox;
        [SyncAssetPath]public string splitPrefab;
        [SyncAssetPath]public string crystalPrefab;
        [SyncAssetPath]public string blizzardPrefab;
        private float bulletSpeed;
        private Vector3 startPos;
        private float range;
        private Vector3 direction;
        private Skill skillData;
        private int pierce;

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
            var unitPos = unit.transform.position;
            CheckFrozen(unit, arg1);
            CheckSplit(unitPos, arg1, arg2);
            CheckCrystal(unitPos, arg1);
            CheckBlizzard(unitPos, arg1);
            // 穿透次数
            if (immunePierce || pierce < arg1.dmgCount)
            {
                Recycle();
            }
        }

        private void CheckFrozen(MonsterController unit, DamageArgs args)
        {
            if(unit == null || unit.CheckMonsterIsDead())return;
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.Frozen);
            if (trigger == null) return;
            trigger.trigger.TryGetValue("prob", out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            prob += skillData.attrMgr.Calc(AttributeType.UpFrozenPro);
            if(Lodash.RandRangeFloat(0, 1) > prob)return;
            var frozenTime = trigger.attrMgr.Calc(AttributeType.FrozenTime) * GameConst.TimeDivisor;
            var buff = new Buff
            {
                id = (int)AttributeType.Frozen,
                attrName = AttributeName.Frozen,
                startTime = GameTime.Instance.GTime,
                duration = frozenTime,
                interval = 0,
                value = 1f,
                valueType = BuffValueType.Negative,
                multi = true,
            };
            unit.AddFrozenBuff(buff);
        }

        private void CheckSplit(Vector3 startPos, DamageArgs args, IDamageable damageable)
        {
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.SplitNum);
            if (trigger == null) return;
            if(!trigger.trigger.TryGetValue(AttributeName.Prob, out var probValue))return;
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1) > prob)return;
            var splitNum = Lodash.RoundToInt(trigger.attrMgr.Calc(AttributeType.SplitNum));
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            // var monsterList = fightingManager.GetRandMonstersInScreen(splitNum);
            // if (monsterList.Count <= 0)
            // {
            //     ListPool<MonsterController>.Release(monsterList);
            //     return;
            // }
            // var targetPoss = new List<Vector3>();
            // foreach (var target in monsterList)
            // {
            //     targetPoss.Add(target.GetHurtPos());
            // }
            // ListPool<MonsterController>.Release(monsterList);
            var tmpBulletPath = splitPrefab;
            for (int i = 0; i < splitNum; i++)
            {
                // var tmpTargetPos = i < targetPoss.Count ? targetPoss[i] : targetPoss[0];
                var angle = Lodash.RandRangeFloat(-60, 60);
                var tmpDirection = Lodash.Angle2Direction(angle);
                var tmpTargetPos = startPos + tmpDirection * 10f;
                var bullet = InstantiateObj(tmpBulletPath, startPos, Quaternion.identity,
                    fightingManager.fightPanelTrans);
                var bulletComp = bullet.GetComponent<PlayerBullet700Small>();
                var dmgArgs = args.Clone();
                dmgArgs.weaponModelId = WeaponModelId;
                bulletComp.originalMonster = damageable as MonsterController;
                bulletComp.InitWithTarget(dmgArgs, configGetter, startPos, tmpTargetPos, this);
            }
        }

        private void CheckCrystal(Vector3 startPosition, DamageArgs args)
        {
            if(WeaponModelId != 705)return;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var tmpBulletPath = crystalPrefab;
            var bullet = InstantiateObj(tmpBulletPath, startPosition, Quaternion.identity, fightingManager.fightPanelTrans);
            var bulletComp = bullet.GetComponent<PlayerBullet700Storm>();
            var dmgArgs = args.Clone();
            dmgArgs.weaponModelId = WeaponModelId;
            bulletComp.InitWithTarget(dmgArgs, configGetter, startPosition, startPosition, this);
        }
        
        private void CheckBlizzard(Vector3 startPosition, DamageArgs args)
        {
            if(WeaponModelId != 706)return;
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.EnableBlizzard);
            if (trigger == null) return;
            trigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1) > prob)return;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var tmpBulletPath = blizzardPrefab;
            var bullet = InstantiateObj(tmpBulletPath, startPosition, Quaternion.identity, fightingManager.fightPanelTrans);
            var bulletComp = bullet.GetComponent<PlayerBullet700Blizzard>();
            var dmgArgs = args.Clone();
            dmgArgs.weaponModelId = WeaponModelId;
            bulletComp.InitWithTarget(dmgArgs, configGetter, startPosition, startPosition, this);
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