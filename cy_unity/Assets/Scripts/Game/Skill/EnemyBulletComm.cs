using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class EnemyBulletComm : BaseBullet
    {
        public AttackBox attackBox;
        [AssetPath] public string boomPath;
        [Tooltip("是否需要调整子弹朝向")]
        public bool orientation = true;
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
            // bulletSize = skillData.attrMgr.Calc(AttributeType.BulletSize);
            if(orientation)
            {
                cacheTransform.up = direction;
            }
            // cacheTransform.localScale = Vector3.one * bulletSize;
            attackBox.ResetData();
            attackBox.damageArgs = damageArgs;
            attackBox.Damage += AttackBoxOnDamage;
            if(!BattleManager.Instance.IsStageForest())
            {
                var collider2D = attackBox.Trigger;
                if (collider2D is BoxCollider2D boxCollider2D)
                {
                    boxCollider2D.size = new Vector2(0.1f, 0.1f);
                }
                else if (collider2D is CircleCollider2D circleCollider2D)
                {
                    circleCollider2D.radius = 0.05f;
                }
            }
        }

        private void AttackBoxOnDamage(DamageArgs arg1, IDamageable arg2, DamageStatus arg3)
        {
            PlayBoom();
            var trigger =
                skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget,
                    AttributeType.Firing);
            if (trigger != null)
            {
                trigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
                var prob = probValue * GameConst.AttributeDivisor;
                if(Lodash.RandRangeFloat(0, 1f) > prob)return;
                var playerCtrl = arg2 as CharacterController;
                if (playerCtrl == null)return;
                var atk = arg1.damagePoint;
                var firingDmg = skillData.attrMgr.Calc(AttributeType.FiringDmg);
                var brulTime = skillData.attrMgr.Calc(AttributeType.BrulTime) *
                               GameConst.TimeDivisor;
                var dmg = (int)(atk * firingDmg);
                var buff = new Buff
                {
                    id = (int)AttributeType.Firing,
                    attrName = AttributeName.Firing,
                    startTime = GameTime.Instance.GTime,
                    duration = brulTime,
                    interval = 1f,
                    value = dmg,
                    valueType = BuffValueType.Negative,
                    multi = true,
                };
                playerCtrl.AddFiringBuff(buff);
            }
        }
        
        private void PlayBoom()
        {
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var pos = transform.position;
            var obj = InstantiateObj(boomPath, pos, Quaternion.identity, fightingManager.fightPanelTrans);
            if (!obj)
            {
                return;
            }
            Recycle();
            fightingManager.AddAutoReleaseUnit(obj, 2, this);
        }

        private void CheckFiring(DamageArgs damageArgs)
        {
            
        }
        
        public override void OnUpdate(float elapseSeconds)
        {
            var position = cacheTransform.position;
            position += direction.normalized * (bulletSpeed * elapseSeconds);
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