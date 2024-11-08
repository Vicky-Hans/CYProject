using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerBullet2200 : BaseBullet
    {
        public AttackBox attackBox;
        private float bulletSpeed;
        private Vector3 startPos;
        private float range;
        private Vector3 direction;
        private Skill skillData;
        private Rect panelRect;
        private int pierce = 0;
        private float launchTime = 0f;
        private float flyTime = 0f;
        private float offsetAngle = 0f;
        private bool enableLaunch = false;

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
            launchTime = skillData.attrMgr.Calc(AttributeType.LaunchTime) * GameConst.TimeDivisor;
            offsetAngle = skillData.attrMgr.Calc(AttributeType.OffsetAngle);
            enableLaunch = skillData.attrMgr.Calc(AttributeType.EnableLaunch) > 0.5f;
            pierce = skillData.Pierce();
            attackBox.ResetData();
            attackBox.ignoreTargetProtect = true;
            attackBox.damageArgs = damageArgs;
            attackBox.Damage += AttackBoxOnDamage;
            var panelTrans = BattleManager.Instance.fightingManagerIns.fightPanelTrans.GetComponent<RectTransform>();
            panelRect = panelTrans.rect;
        }

        private void AttackBoxOnDamage(DamageArgs arg1, IDamageable arg2, DamageStatus arg3)
        {
            if((arg3 & DamageStatus.Ignore) != 0)return;
            var unit = arg2 as MonsterController;
            if(unit == null)return;
            var immunePierce = unit.MonsterData.ImmunePierce();
            var unitPos = unit.transform.position;
            if(enableLaunch)
            {
                CollisionReflect(unitPos);
            }
            // 穿透次数
            if (immunePierce)
            {
                Recycle();
            }
            else if (!enableLaunch)
            {
                if (pierce < arg1.dmgCount)
                {
                    Recycle();
                }
            }
        }
        
        public override void OnUpdate(float elapseSeconds)
        {
            if(Recycled)return;
            flyTime += elapseSeconds;
            if(enableLaunch && flyTime >= launchTime)
            {
                Recycle();
                return;
            }
            var position = cacheTransform.position;
            position += cacheTransform.up * (bulletSpeed * elapseSeconds);
            cacheTransform.position = position;
            if (!enableLaunch)
            {
                if (Vector3.Distance(startPos, position) > range)
                {
                    Recycle();
                }
                return;
            }
            CheckCatapult(cacheTransform.localPosition);
        }

        private void CheckCatapult(Vector3 pos)
        {
            if(BattleManager.Instance.IsStageForest())return;
            if (!panelRect.Contains(pos))
            {
                var oldUp = cacheTransform.up;
                if(pos.x < panelRect.xMin || pos.x > panelRect.xMax)
                {
                    oldUp.x = -oldUp.x;
                }
                if(pos.y < panelRect.yMin || pos.y > panelRect.yMax)
                {
                    oldUp.y = -oldUp.y;
                }
                cacheTransform.up = oldUp;
            }
        }

        private void CollisionReflect(Vector3 pos)
        {
            var tmpDirection = cacheTransform.up * -1f;
            var tmpAngle = Lodash.Direction2Angle(tmpDirection);
            tmpAngle += Lodash.RandRangeFloat(0, 1) > 0.5f ? offsetAngle:-offsetAngle;
            tmpDirection = Lodash.Angle2Direction(tmpAngle);
            cacheTransform.up = tmpDirection;
        }
        
        protected override void Recycle()
        {
            attackBox.Damage -= AttackBoxOnDamage;
            base.Recycle();
        }
        
    }
}