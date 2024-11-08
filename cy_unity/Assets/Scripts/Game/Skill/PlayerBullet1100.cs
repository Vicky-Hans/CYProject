using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerBullet1100 : BaseBullet
    {
        public AttackBox attackBox;
        private float bulletSpeed;
        private Vector3 startPos;
        private float range;
        private Vector3 direction;
        private Skill skillData;
        private int pierce;
        
        public bool CanReturn { get; set; }  // 是否能回旋
        private bool IsReturned { get; set; }
        private RotateSelf rotateSelfComp;

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
            // cacheTransform.up = direction;
            if (WeaponModelId == 1105)
            {
                var landCircleBulletSize = skillData.attrMgr.Calc(AttributeType.LandCircleBulletSize);
                var equipCount = Get1100Count();
                bulletSize *= (1 + landCircleBulletSize * equipCount);
            }
            cacheTransform.localScale = Vector3.one * bulletSize;
            attackBox.ResetData();
            attackBox.ignoreTargetProtect = true;
            attackBox.damageArgs = damageArgs;
            attackBox.Damage += AttackBoxOnDamage;

            var rotateSpd = skillData.attrMgr.Calc(AttributeType.RotateSpd);
            rotateSelfComp = gameObject.AddComponent<RotateSelf>();
            rotateSelfComp.BulletIns = this;
            rotateSelfComp.Spd = rotateSpd;
        }

        private int Get1100Count()
        {
            var weapons = BattleManager.Instance.fightingManagerIns.playerCtrl.WeaponSkillController.weapons;
            if(weapons == null)return 0;
            var equipCount = 0;
            var count = weapons.Count;
            for (int i = 0; i < count; ++i)
            {
                if (weapons[i].weaponData.EquipId == 11)
                {
                    equipCount++;
                }
            }
            return equipCount;
        }

        private void AttackBoxOnDamage(DamageArgs arg1, IDamageable arg2, DamageStatus arg3)
        {
            if((arg3 & DamageStatus.Ignore) != 0)return;
            var unit = arg2 as MonsterController;
            if(unit == null)return;
            var immunePierce = unit.MonsterData.ImmunePierce();
            var unitPos = unit.transform.position;
            CheckWaterDownSpd(unit, arg1);
            if (immunePierce)
            {
                Recycle();
            }
        }

        private void CheckWaterDownSpd(MonsterController unit, DamageArgs args)
        {
            if(unit == null || unit.CheckMonsterIsDead())return;
            if(WeaponModelId != 1106)return;
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.WaterCircleHit, AttributeType.Decelerate);
            if(trigger == null)return;
            trigger.trigger.TryGetValue("prob", out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1) > prob)return;
            var decelerateTime = trigger.attrMgr.Calc(AttributeType.DecelerateTime) * GameConst.TimeDivisor;
            var downAtkSpdTime = trigger.attrMgr.Calc(AttributeType.DownAtkSpdTime) * GameConst.TimeDivisor;
            var downSpd = trigger.attrMgr.Calc(AttributeType.DownSpd);
            downSpd += skillData.attrMgr.Calc(AttributeType.WaterDownSpd);
            var downAtkSpd = trigger.attrMgr.Calc(AttributeType.DownAtkSpd);
            downAtkSpd += skillData.attrMgr.Calc(AttributeType.WaterDownAtkSpd);
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
            if(Recycled)return;
            rotateSelfComp.OnUpdate(elapseSeconds);
            var position = cacheTransform.position;
            position += direction * (bulletSpeed * elapseSeconds);
            cacheTransform.position = position;
            if (IsReturned)
            {
                if(cacheTransform.position.x < startPos.x)
                    Recycle();
                return;
            }
            if (Vector3.Distance(startPos, position) > range)
            {
                if (CanReturn)
                {
                    IsReturned = true;
                    direction = -direction;
                }
                else
                {
                    Recycle();
                }
            }
        }
        
        protected override void Recycle()
        {
            attackBox.Damage -= AttackBoxOnDamage;
            base.Recycle();
        }
        
    }
}