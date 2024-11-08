using System;
using System.Collections.Generic;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    /// <summary>
    /// 作为辅助组件释放攻击，可以统一伤害输出逻辑
    /// </summary>
    public class  AttackComponent : MonoBehaviour
    {
        [TagSelector] public string[] acceptTags;
        public DamageArgs damageArgs;
        
        public int damageCount;
        
        /// <summary>
        /// 是否忽略攻击
        /// </summary>
        internal bool ignoreAttack;
        
        /// <summary>
        /// 取消对单次激活对单个目标只造成一次伤害的功能
        /// </summary>
        internal bool ignoreTargetProtect;

        private Rigidbody2D rig;
        private Collider2D trigger;
        private bool attackEnable = true;
        private readonly HashSet<IDamageable> damagables = new();

        public Collider2D Trigger => trigger;

        public LayerMask GetLayerMask()
        {
            return trigger.includeLayers;
        }
        
        public event Action<DamageArgs, IDamageable, DamageStatus> Damage;

        protected void OnDisable()
        {
            damagables.Clear();
        }

        public virtual bool CheckInRange(Vector3 target)
        {
            return true;
        }

        public bool IsEnable()
        {
            return attackEnable;
        }

        public void ResetData()
        {
            damagables.Clear();
            damageCount = 0;
        }

        public float GetRigRadius()
        {
            CircleCollider2D circle = GetComponent<CircleCollider2D>();
            return circle ? circle.radius : 0f;
        }

        public void EnableAttack()
        {
            if (attackEnable) return;

            if (!rig)
            {
                rig = GetComponent<Rigidbody2D>();
                trigger = GetComponent<Collider2D>();
            }

            if (!rig) return;

#if UNITY_EDITOR
            if (!rig.isKinematic) throw new Exception("Rigidbody must set isKinematic true");
#endif

            trigger.enabled = true;
            rig.WakeUp();
            attackEnable = true;
            damagables.Clear();
        }

        public void EnableAttackWithRange(float current, Vector2 range)
        {
            if (current > range.y)
            {
                DisableAttack();
                return;
            }

            if (current > range.x) EnableAttack();
        }

        public void DisableAttack()
        {
            if (!attackEnable) return;

            if (!trigger) trigger = GetComponent<Collider2D>();

            trigger.enabled = false;
            attackEnable = false;
            damagables.Clear();
        }

        public DamageStatus OnTargetEnter(Collider2D other)
        {
            if (ignoreAttack) return DamageStatus.Ignore;
            var baseMonoUnit = other.GetComponent<BaseMonoUnit>();
            if (baseMonoUnit == null) return DamageStatus.Ignore;
            if(baseMonoUnit.Data.IsDead()) return DamageStatus.Ignore;

            if (!CheckInRange(other.transform.position)) return DamageStatus.Ignore;

            var accept = TagHelper.CheckTags(acceptTags, other);

            if (!accept) return DamageStatus.Ignore;

            var target = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
            var damagable = target.GetComponent<IDamageable>();
            if (damagable == null) return DamageStatus.Ignore;

            return DoAttack(damagable, target);
        }
        
        public void AddDamagable(IDamageable damagable)
        {
            if (damagables.Contains(damagable)) return;
            damagables.Add(damagable);
        }

        public DamageStatus DoAttack(IDamageable damagable, GameObject target)
        {
            if (!ignoreTargetProtect && damagables.Contains(damagable)) return DamageStatus.Ignore;
            if (damageArgs.sender != null && damageArgs.sender.Data is Player)
            {
                var skillData = damageArgs.skillData;
                var pierce = skillData.Pierce(damageArgs.weaponSkill);
                if (damageCount > pierce) return DamageStatus.Ignore;
            }
            if (!ignoreTargetProtect) damagables.Add(damagable);
            damageCount++;
            damageArgs.dmgCount++;
            var args = damageArgs.Clone();
            args.target = target;
            args.dmgCount = damageCount;
            var status = args.ApplyDamage(damagable);
            if ((status & DamageStatus.Miss) > 0)
            {
                return status;
            }
            Damage?.Invoke(args, damagable, status);
            return status;
        }
    }
}