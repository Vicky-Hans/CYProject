using System;
using System.Collections;
using System.Collections.Generic;
using DHFramework;
using UnityEngine;

namespace DH.Game
{
    /// <summary>
    /// 适用于方形的连续伤害攻击模式
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class ContinuousAttackAdapter : MonoBehaviour
    {
        private class TargetItem : IReference
        {
            public BaseMonoUnit unit;
            public float nextDamageTime;
            
            public void Clear()
            {
                unit = null;
                nextDamageTime = 0;
            }
        }
        
        /// <summary>
        /// 用于获取目标Tag
        /// </summary>
        internal AttackComponent attackBox;
        internal float attackInterval;

        private float timer;
        private readonly Dictionary<BaseMonoUnit, TargetItem> targetItems = new Dictionary<BaseMonoUnit, TargetItem>();

        public void ResetData()
        {
            timer = 0;
            foreach (var item in targetItems)
            {
                ReferencePool.Release(item.Value);
            }
            targetItems.Clear();
        }

        public void Release()
        {
            ResetData();
        }

        public void OnUpdate(float deltaTime)
        {
            timer += deltaTime;
            foreach (var item in targetItems)
            {
                if (timer > item.Value.nextDamageTime)
                {
                    item.Value.nextDamageTime += attackInterval;
                    attackBox.DoAttack((IDamageable)item.Value.unit, item.Value.unit.gameObject);
                }
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (!TagHelper.CheckTags(attackBox.acceptTags, other))
            {
                return;
            }
            
            var target = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
            var baseMonoUnit = target.GetComponent<BaseMonoUnit>();
            if (baseMonoUnit == null)
            {
                return;
            }

            if (targetItems.ContainsKey(baseMonoUnit))
            {
                return;
            }

            var item = ReferencePool.Acquire<TargetItem>();
            item.unit = baseMonoUnit;
            item.nextDamageTime = timer;
            targetItems.Add(baseMonoUnit,item);
        }
        
        public void OnTriggerExit(Collider other)
        {
            if (!TagHelper.CheckTags(attackBox.acceptTags, other))
            {
                return;
            }
            
            var target = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
            var baseMonoUnit = target.GetComponent<BaseMonoUnit>();
            if (baseMonoUnit == null)
            {
                return;
            }
            
            targetItems.Remove(baseMonoUnit);
        }
    }
}