using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class BossTraceBullet : BaseBullet
    {
        public AttackBox attackBox;
        [AssetPath] public string boomPath;
        [Tooltip("是否需要调整子弹朝向")]
        public bool orientation = true;
        private float bulletSpeed;
        private Vector3 startPos;
        private Vector3 direction;
        private Skill skillData;
        private float duration;
        private float bulletTime;
        private bool inited;

        public Transform TargetTrans { get; set; }

        public override void InitWithTarget(DamageArgs damageArgs, Func<string, float> paramGetter,
            Vector3 startPosition, Vector3 targetPosition, IPool<GameObject> pool)
        {
            base.InitWithTarget(damageArgs, paramGetter, startPosition, targetPosition, pool);
            skillData = damageArgs.skillData;
            startPos = startPosition;
            bulletSpeed = skillData.BulletSpeed();
            direction = targetPosition - startPos;
            duration = skillData.attrMgr.Calc(AttributeType.Duration) * GameConst.TimeDivisor;
            // bulletSize = skillData.attrMgr.Calc(AttributeType.BulletSize);
            if(orientation)
            {
                cacheTransform.up = direction;
            }
            // cacheTransform.localScale = Vector3.one * bulletSize;
            attackBox.ResetData();
            attackBox.damageArgs = damageArgs;
            attackBox.Damage += AttackBoxOnDamage;
            inited = true;
        }
        
        private void AttackBoxOnDamage(DamageArgs arg1, IDamageable arg2, DamageStatus arg3)
        {
            PlayBoom();
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
        
        public override void OnUpdate(float elapseSeconds)
        {
            if(Recycled)return;
            if(!inited)
            {
                return;
            }
            bulletTime += elapseSeconds;
            if (bulletTime > duration)
            {
                Recycle();
                return;
            }
            if(TargetTrans == null)
            {
                return;
            }
            var position = cacheTransform.position;
            direction = (TargetTrans.position - position).normalized;
            position += direction.normalized * (bulletSpeed * elapseSeconds);
            cacheTransform.position = position;
        }
        
        protected override void Recycle()
        {
            attackBox.Damage -= AttackBoxOnDamage;
            base.Recycle();
        }
    }
}